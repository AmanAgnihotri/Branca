// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2026 Aman Agnihotri

namespace Branca.AspNetCore.Tests;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

public sealed class BrancaAuthenticationTests
{
  private static readonly BrancaKey _key = BrancaKey.Generate();

  [Fact(DisplayName = "A request without a token is unauthorized.")]
  public async Task RequestWithoutTokenIsUnauthorized()
  {
    await using WebApplication app = await StartAsync();
    using HttpClient client = app.GetTestClient();

    using HttpResponseMessage response =
      await GetAsync(client, "/secure", null);

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [Fact(DisplayName = "A valid token authenticates the principal.")]
  public async Task ValidTokenAuthenticatesThePrincipal()
  {
    await using WebApplication app = await StartAsync();
    using HttpClient client = app.GetTestClient();

    string token = Encode("""{ "name": "alice" }""");

    using HttpResponseMessage response =
      await GetAsync(client, "/secure", token);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.Equal("alice", await response.Content.ReadAsStringAsync());
  }

  [Fact(DisplayName = "A malformed token is unauthorized.")]
  public async Task MalformedTokenIsUnauthorized()
  {
    await using WebApplication app = await StartAsync();
    using HttpClient client = app.GetTestClient();

    using HttpResponseMessage response =
      await GetAsync(client, "/secure", "not-a-real-token");

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [Fact(DisplayName = "A tampered token is unauthorized.")]
  public async Task TamperedTokenIsUnauthorized()
  {
    await using WebApplication app = await StartAsync();
    using HttpClient client = app.GetTestClient();

    string token = Encode("""{ "name": "alice" }""");
    string tampered = token[..^1] + (token[^1] == 'a' ? 'b' : 'a');

    using HttpResponseMessage response =
      await GetAsync(client, "/secure", tampered);

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [Fact(DisplayName = "An expired token is unauthorized.")]
  public async Task ExpiredTokenIsUnauthorized()
  {
    await using WebApplication app = await StartAsync();
    using HttpClient client = app.GetTestClient();

    // Issued at the Unix epoch, well beyond the one-hour lifetime.
    string token = Encode("""{ "name": "alice" }""", 0);

    using HttpResponseMessage response =
      await GetAsync(client, "/secure", token);

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [Fact(DisplayName = "A token from a different key is unauthorized.")]
  public async Task TokenFromDifferentKeyIsUnauthorized()
  {
    await using WebApplication app = await StartAsync();
    using HttpClient client = app.GetTestClient();

    using BrancaService other = new(BrancaKey.Generate());
    string token = other.Encode("""{ "name": "alice" }""");

    using HttpResponseMessage response =
      await GetAsync(client, "/secure", token);

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [Fact(DisplayName = "A future token within the clock skew is accepted.")]
  public async Task FutureTokenWithinTheClockSkewIsAccepted()
  {
    await using WebApplication app =
      await StartAsync(o => o.ClockSkewInSeconds = 300);
    using HttpClient client = app.GetTestClient();

    uint future =
      (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 200;

    string token = Encode("""{ "name": "frank" }""", future);

    using HttpResponseMessage response =
      await GetAsync(client, "/secure", token);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.Equal("frank", await response.Content.ReadAsStringAsync());
  }

  [Fact(DisplayName =
    "A token from a previous key is accepted after rotation.")]
  public async Task TokenFromPreviousKeyIsAcceptedAfterRotation()
  {
    BrancaKey previous = BrancaKey.Generate();

    await using WebApplication app =
      await StartAsync(o => o.PreviousKeys = [previous]);
    using HttpClient client = app.GetTestClient();

    using BrancaService old = new(previous);
    string token = old.Encode("""{ "name": "erin" }""");

    using HttpResponseMessage response =
      await GetAsync(client, "/secure", token);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.Equal("erin", await response.Content.ReadAsStringAsync());
  }

  [Fact(DisplayName = "A matching role is authorized.")]
  public async Task MatchingRoleIsAuthorized()
  {
    await using WebApplication app = await StartAsync();
    using HttpClient client = app.GetTestClient();

    string token = Encode("""{ "name": "bob", "role": ["admin", "user"] }""");

    using HttpResponseMessage response =
      await GetAsync(client, "/admin", token);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact(DisplayName = "A missing role is forbidden.")]
  public async Task MissingRoleIsForbidden()
  {
    await using WebApplication app = await StartAsync();
    using HttpClient client = app.GetTestClient();

    string token = Encode("""{ "name": "carol", "role": ["user"] }""");

    using HttpResponseMessage response =
      await GetAsync(client, "/admin", token);

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }

  [Fact(DisplayName = "A token can be read from a configured cookie.")]
  public async Task TokenCanBeReadFromCookie()
  {
    await using WebApplication app =
      await StartAsync(o => o.CookieName = "branca");
    using HttpClient client = app.GetTestClient();

    string token = Encode("""{ "name": "dave" }""");

    using HttpRequestMessage request = new(HttpMethod.Get, "/secure");
    request.Headers.Add("Cookie", $"branca={token}");

    using HttpResponseMessage response = await client.SendAsync(request);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.Equal("dave", await response.Content.ReadAsStringAsync());
  }

  [Fact(DisplayName = "A custom claims mapping is applied.")]
  public async Task CustomClaimsMappingIsApplied()
  {
    await using WebApplication app = await StartAsync(o => o.MapClaims =
      payload => [new Claim("name", Encoding.UTF8.GetString(payload))]);

    using HttpClient client = app.GetTestClient();

    using BrancaService service = new(_key);
    string token = service.Encode("eve");

    using HttpResponseMessage response =
      await GetAsync(client, "/secure", token);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.Equal("eve", await response.Content.ReadAsStringAsync());
  }

  private static string Encode(string json, uint? createTime = null)
  {
    using BrancaService service = new(_key);

    byte[] payload = Encoding.UTF8.GetBytes(json);

    return createTime is { } time
      ? service.Encode(payload, time)
      : service.Encode(payload);
  }

  private static async Task<HttpResponseMessage> GetAsync(
    HttpClient client, string path, string? token)
  {
    using HttpRequestMessage request = new(HttpMethod.Get, path);

    if (token is not null)
    {
      request.Headers.Authorization =
        new AuthenticationHeaderValue("Bearer", token);
    }

    return await client.SendAsync(request);
  }

  private static async Task<WebApplication> StartAsync(
    Action<BrancaOptions>? configure = null)
  {
    WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();

    builder.WebHost.UseTestServer();

    builder.Services
      .AddAuthentication(BrancaDefaults.AuthenticationScheme)
      .AddBranca(options =>
      {
        options.Key = _key;
        configure?.Invoke(options);
      });

    builder.Services.AddAuthorization();

    WebApplication app = builder.Build();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapGet("/secure", (ClaimsPrincipal user) => user.Identity?.Name ?? "")
      .RequireAuthorization();

    app.MapGet("/admin", () => "admin")
      .RequireAuthorization(policy => policy.RequireRole("admin"));

    await app.StartAsync();

    return app;
  }
}
