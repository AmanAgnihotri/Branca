// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2022-2026 Aman Agnihotri

namespace Branca.Tests;

public sealed class BrancaFactory
{
  public static BrancaService Create(HexKey key, uint timestamp)
  {
    return new BrancaService(key, new BrancaSettings
    {
      Timer = new MockTimer(timestamp),
    });
  }
}
