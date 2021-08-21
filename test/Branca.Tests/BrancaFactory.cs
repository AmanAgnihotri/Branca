// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2021 Aman Agnihotri

namespace Branca.Tests
{
  public sealed class BrancaFactory
  {
    public static Branca Create(HexKey key, uint timestamp)
    {
      return new(key, new() {Timer = new MockTimer(timestamp)});
    }
  }
}
