// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2026 Aman Agnihotri

namespace Branca.AspNetCore;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;

public sealed class BrancaHandler(
  IOptionsMonitor<BrancaOptions> options,
  ILoggerFactory logger,
  UrlEncoder encoder
) : AuthenticationHandler<BrancaOptions>(options, logger, encoder)
{
  private new BrancaEvents Events
  {
    get => (BrancaEvents)base.Events!;
    set => base.Events = value;
  }

  protected override Task<object> CreateEventsAsync()
  {
    return Task.FromResult<object>(new BrancaEvents());
  }

  protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
  {
    BrancaMessageReceivedContext context = new(Context, Scheme, Options);

    await Events.MessageReceived(context);

    if (context.Result is not null)
    {
      return context.Result;
    }

    string? token = context.Token ?? ReadToken();

    if (string.IsNullOrEmpty(token))
    {
      return AuthenticateResult.NoResult();
    }

    if (Options.Service is not { } service)
    {
      return await Fail("The Branca handler has no configured key.");
    }

    if (!service.TryDecode(token, out byte[] payload, out uint createTime))
    {
      return await Fail("The Branca token is invalid or has expired.");
    }

    IEnumerable<Claim>? claims;

    try
    {
      claims = Options.MapClaims(payload);
    }
    catch (Exception exception)
    {
      return await Fail(
        "The Branca token payload could not be mapped.", exception);
    }

    if (claims is null)
    {
      return await Fail("The Branca token payload could not be mapped.");
    }

    ClaimsIdentity identity = new(
      claims, Scheme.Name, Options.NameClaimType, Options.RoleClaimType);

    BrancaTokenValidatedContext validated = new(Context, Scheme, Options)
    {
      Principal = new ClaimsPrincipal(identity),
      Payload = payload,
      CreateTime = createTime,
    };

    await Events.TokenValidated(validated);

    if (validated.Result is not null)
    {
      return validated.Result;
    }

    validated.Success();

    return validated.Result!;
  }

  protected override Task HandleChallengeAsync(
    AuthenticationProperties properties)
  {
    Response.StatusCode = StatusCodes.Status401Unauthorized;

    Response.Headers.Append(HeaderNames.WWWAuthenticate, Options.BearerScheme);

    return Task.CompletedTask;
  }

  private string? ReadToken()
  {
    string authorization = Request.Headers.Authorization.ToString();
    string prefix = Options.BearerScheme + " ";

    if (authorization.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
    {
      return authorization[prefix.Length..].Trim();
    }

    if (Options.CookieName is { Length: > 0 } cookie &&
        Request.Cookies.TryGetValue(cookie, out string? value))
    {
      return value;
    }

    return null;
  }

  private async Task<AuthenticateResult> Fail(
    string message, Exception? exception = null)
  {
    BrancaFailedContext context = new(Context, Scheme, Options)
    {
      FailureMessage = message,
      Exception = exception,
    };

    await Events.AuthenticationFailed(context);

    return context.Result ?? AuthenticateResult.Fail(message);
  }
}
