// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2026 Aman Agnihotri

namespace Branca.AspNetCore;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

public sealed class BrancaMessageReceivedContext(
  HttpContext context,
  AuthenticationScheme scheme,
  BrancaOptions options
) : ResultContext<BrancaOptions>(context, scheme, options)
{
  public string? Token { get; set; }
}

public sealed class BrancaTokenValidatedContext(
  HttpContext context,
  AuthenticationScheme scheme,
  BrancaOptions options
) : ResultContext<BrancaOptions>(context, scheme, options)
{
  public ReadOnlyMemory<byte> Payload { get; init; }

  public uint CreateTime { get; init; }
}

public sealed class BrancaFailedContext(
  HttpContext context,
  AuthenticationScheme scheme,
  BrancaOptions options
) : ResultContext<BrancaOptions>(context, scheme, options)
{
  public string? FailureMessage { get; init; }

  public Exception? Exception { get; init; }
}
