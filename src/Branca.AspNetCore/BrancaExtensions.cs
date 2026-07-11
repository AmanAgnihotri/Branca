// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2026 Aman Agnihotri

namespace Microsoft.Extensions.DependencyInjection;

using AspNetCore.Authentication;
using Branca.AspNetCore;
using Extensions;
using Options;

public static class BrancaExtensions
{
  public static AuthenticationBuilder AddBranca(
    this AuthenticationBuilder builder,
    Action<BrancaOptions> configureOptions)
  {
    return builder.AddBranca(
      BrancaDefaults.AuthenticationScheme, configureOptions);
  }

  public static AuthenticationBuilder AddBranca(
    this AuthenticationBuilder builder,
    string authenticationScheme,
    Action<BrancaOptions> configureOptions)
  {
    return builder.AddBranca(authenticationScheme, null, configureOptions);
  }

  public static AuthenticationBuilder AddBranca(
    this AuthenticationBuilder builder,
    string authenticationScheme,
    string? displayName,
    Action<BrancaOptions> configureOptions)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.Services.TryAddEnumerable(
      ServiceDescriptor.Singleton<
        IPostConfigureOptions<BrancaOptions>,
        BrancaPostConfigureOptions
      >());

    return builder.AddScheme<BrancaOptions, BrancaHandler>(
      authenticationScheme, displayName, configureOptions);
  }
}
