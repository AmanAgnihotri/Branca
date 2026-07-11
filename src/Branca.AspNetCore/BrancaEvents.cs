// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2026 Aman Agnihotri

namespace Branca.AspNetCore;

public sealed class BrancaEvents
{
  public Func<BrancaMessageReceivedContext, Task> OnMessageReceived
  {
    get;
    set;
  } = static _ => Task.CompletedTask;

  public Func<BrancaTokenValidatedContext, Task> OnTokenValidated
  {
    get;
    set;
  } = static _ => Task.CompletedTask;

  public Func<BrancaFailedContext, Task> OnAuthenticationFailed
  {
    get;
    set;
  } = static _ => Task.CompletedTask;

  public Task MessageReceived(BrancaMessageReceivedContext context)
  {
    return OnMessageReceived(context);
  }

  public Task TokenValidated(BrancaTokenValidatedContext context)
  {
    return OnTokenValidated(context);
  }

  public Task AuthenticationFailed(BrancaFailedContext context)
  {
    return OnAuthenticationFailed(context);
  }
}
