// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2021 Aman Agnihotri

namespace Branca
{
  using System;

  public interface ITimer
  {
    uint UnixNow { get; }
  }

  internal sealed class Timer : ITimer
  {
    public uint UnixNow =>
      Convert.ToUInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
  }
}
