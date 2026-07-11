// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2026 Aman Agnihotri

namespace Microsoft.Extensions.DependencyInjection;

using AspNetCore.Authentication;
using Branca;
using Branca.AspNetCore;
using Extensions;
using Options;

public static class BrancaExtensions
{
  public static AuthenticationBuilder AddBranca(
    this AuthenticationBuilder builder)
  {
    return builder.AddBranca(BrancaDefaults.AuthenticationScheme);
  }

  public static AuthenticationBuilder AddBranca(
    this AuthenticationBuilder builder,
    string authenticationScheme)
  {
    return builder.AddBranca(authenticationScheme, static _ => { });
  }

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

  public static IServiceCollection AddBranca(
    this IServiceCollection services,
    BrancaKey key)
  {
    return services.AddBranca(key, new BrancaSettings());
  }

  public static IServiceCollection AddBranca(
    this IServiceCollection services,
    BrancaKey key,
    BrancaSettings settings)
  {
    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(key);
    ArgumentNullException.ThrowIfNull(settings);

    services.TryAddSingleton<IBrancaService>(
      _ => new BrancaService(key, settings));

    return services;
  }
}
