// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2026 Aman Agnihotri

namespace Branca.AspNetCore.Tests;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using Xunit;

public sealed class DependencyInjectionTests
{
  [Fact(DisplayName = "The registered service issues and validates tokens.")]
  public async Task TheRegisteredServiceIssuesAndValidatesTokens()
  {
    BrancaKey key = BrancaKey.Generate();

    await using WebApplication app = await StartAsync(
      services => services.AddBranca(key));
    using HttpClient client = app.GetTestClient();

    using HttpResponseMessage login = await client.GetAsync("/login");
    string token = await login.Content.ReadAsStringAsync();

    using HttpResponseMessage response =
      await GetAsync(client, "/secure", token);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.Equal("grace", await response.Content.ReadAsStringAsync());
  }

  [Fact(DisplayName = "The registered service honours its settings.")]
  public async Task TheRegisteredServiceHonoursItsSettings()
  {
    BrancaKey key = BrancaKey.Generate();
    BrancaKey previous = BrancaKey.Generate();

    await using WebApplication app = await StartAsync(services =>
      services.AddBranca(key, new BrancaSettings
      {
        PreviousKeys = [previous],
      }));
    using HttpClient client = app.GetTestClient();

    using BrancaService old = new(previous);
    string token = old.Encode("""{ "name": "heidi" }""");

    using HttpResponseMessage response =
      await GetAsync(client, "/secure", token);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.Equal("heidi", await response.Content.ReadAsStringAsync());
  }

  [Fact(DisplayName = "An explicit options key overrides the service.")]
  public async Task AnExplicitOptionsKeyOverridesTheService()
  {
    BrancaKey registered = BrancaKey.Generate();
    BrancaKey explicitKey = BrancaKey.Generate();

    await using WebApplication app = await StartAsync(
      services => services.AddBranca(registered),
      options => options.Key = explicitKey);
    using HttpClient client = app.GetTestClient();

    using HttpResponseMessage login = await client.GetAsync("/login");
    string issuedByRegistered = await login.Content.ReadAsStringAsync();

    using HttpResponseMessage rejected =
      await GetAsync(client, "/secure", issuedByRegistered);

    Assert.Equal(HttpStatusCode.Unauthorized, rejected.StatusCode);

    using BrancaService branca = new(explicitKey);
    string issuedByExplicit = branca.Encode("""{ "name": "grace" }""");

    using HttpResponseMessage accepted =
      await GetAsync(client, "/secure", issuedByExplicit);

    Assert.Equal(HttpStatusCode.OK, accepted.StatusCode);
  }

  [Fact(DisplayName = "Authentication without any key fails loudly.")]
  public async Task AuthenticationWithoutAnyKeyFailsLoudly()
  {
    await using WebApplication app = await StartAsync(_ => { });
    using HttpClient client = app.GetTestClient();

    await Assert.ThrowsAsync<InvalidOperationException>(
      () => GetAsync(client, "/secure", "irrelevant"));
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
    Action<IServiceCollection> registerBranca,
    Action<BrancaOptions>? configureOptions = null)
  {
    WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();

    builder.WebHost.UseTestServer();

    registerBranca(builder.Services);

    AuthenticationBuilder authentication = builder.Services
      .AddAuthentication(BrancaDefaults.AuthenticationScheme);

    if (configureOptions is null)
    {
      authentication.AddBranca();
    }
    else
    {
      authentication.AddBranca(configureOptions);
    }

    builder.Services.AddAuthorization();

    WebApplication app = builder.Build();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapGet("/login", ([FromServices] IBrancaService branca) =>
      branca.Encode("""{ "name": "grace" }"""));

    app.MapGet("/secure", (ClaimsPrincipal user) => user.Identity?.Name ?? "")
      .RequireAuthorization();

    await app.StartAsync();

    return app;
  }
}
