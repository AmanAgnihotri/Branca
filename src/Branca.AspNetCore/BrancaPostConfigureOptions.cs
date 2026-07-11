// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2026 Aman Agnihotri

namespace Branca.AspNetCore;

using Microsoft.Extensions.Options;

internal sealed class BrancaPostConfigureOptions(IBrancaService? branca = null)
  : IPostConfigureOptions<BrancaOptions>
{
  public void PostConfigure(string? name, BrancaOptions options)
  {
    ArgumentNullException.ThrowIfNull(options);

    if (options.Key is { } key)
    {
      options.Service = new BrancaService(key, new BrancaSettings
      {
        TokenLifetimeInSeconds = options.TokenLifetimeInSeconds,
        ClockSkewInSeconds = options.ClockSkewInSeconds,
        MaxStackLimit = options.MaxStackLimit,
        PreviousKeys = options.PreviousKeys,
      });

      return;
    }

    if (branca is not null)
    {
      options.Service = branca;

      return;
    }

    throw new InvalidOperationException(
      "No Branca key is configured. Set BrancaOptions.Key or register " +
      "a service with AddBranca on the service collection.");
  }
}
