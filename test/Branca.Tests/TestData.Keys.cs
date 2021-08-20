// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2021 Aman Agnihotri

namespace Branca.Tests
{
  public static partial class TestData
  {
    public const string Key = ValidKey01;

    public const string ValidKey01 =
      "73757065727365637265746b6579796f7573686f756c646e6f74636f6d6d6974";

    public const string ValidKey02 =
      "77726f6e677365637265746b6579796f7573686f756c646e6f74636f6d6d6974";

    public const string InvalidKey01 = "746f6f73686f72746b6579";

    public const string InvalidKey02 =
      "73757065727365637265746b6579796f7573686f756c646e6f74636f6d6d697Z";
  }
}
