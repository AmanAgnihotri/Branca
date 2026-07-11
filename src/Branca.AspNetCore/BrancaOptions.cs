// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2026 Aman Agnihotri

namespace Branca.AspNetCore;

using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

public sealed class BrancaOptions : AuthenticationSchemeOptions
{
  public BrancaKey? Key { get; set; }

  public uint? TokenLifetimeInSeconds { get; set; } = 3600;

  public uint MaxStackLimit { get; set; } = 1024;

  public string BearerScheme { get; set; } = "Bearer";

  public string? CookieName { get; set; }

  public string NameClaimType { get; set; } = "name";

  public string RoleClaimType { get; set; } = "role";

  public Func<byte[], IEnumerable<Claim>?> MapClaims { get; set; } =
    BrancaClaims.FromJson;

  public new BrancaEvents Events
  {
    get => (BrancaEvents)base.Events!;
    set => base.Events = value;
  }

  internal BrancaService? Service { get; set; }
}
