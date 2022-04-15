// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2022 Aman Agnihotri

namespace Branca.Tests;

public sealed class MockTimer : ITimer
{
  public uint UnixNow { get; }

  public MockTimer(uint fixedTime)
  {
    UnixNow = fixedTime;
  }
}
