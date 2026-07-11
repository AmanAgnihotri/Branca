// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2026 Aman Agnihotri

namespace Branca.AspNetCore;

using Microsoft.Extensions.Options;

internal sealed class BrancaPostConfigureOptions
  : IPostConfigureOptions<BrancaOptions>
{
  public void PostConfigure(string? name, BrancaOptions options)
  {
    ArgumentNullException.ThrowIfNull(options);

    if (options.Key is null)
    {
      throw new InvalidOperationException(
        "A Branca key must be set on BrancaOptions.Key.");
    }

    options.Service = new BrancaService(options.Key, new BrancaSettings
    {
      TokenLifetimeInSeconds = options.TokenLifetimeInSeconds,
      MaxStackLimit = options.MaxStackLimit,
      PreviousKeys = options.PreviousKeys,
    });
  }
}
