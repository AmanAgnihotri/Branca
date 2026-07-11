// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright © 2026 Aman Agnihotri

namespace Branca.AspNetCore;

using System.Security.Claims;
using System.Text.Json;

public static class BrancaClaims
{
  public static IEnumerable<Claim>? FromJson(byte[] payload)
  {
    ArgumentNullException.ThrowIfNull(payload);

    JsonDocument document;

    try
    {
      document = JsonDocument.Parse(payload);
    }
    catch (JsonException)
    {
      return null;
    }

    using (document)
    {
      if (document.RootElement.ValueKind is not JsonValueKind.Object)
      {
        return null;
      }

      List<Claim> claims = [];

      foreach (JsonProperty property in document.RootElement.EnumerateObject())
      {
        AddClaims(claims, property.Name, property.Value);
      }

      return claims;
    }
  }

  private static void AddClaims(
    List<Claim> claims,
    string name,
    JsonElement value)
  {
    switch (value.ValueKind)
    {
      case JsonValueKind.Array:
        foreach (JsonElement item in value.EnumerateArray())
        {
          AddClaims(claims, name, item);
        }

        break;

      case JsonValueKind.String:
        claims.Add(new Claim(name, value.GetString() ?? string.Empty));

        break;

      case JsonValueKind.Number:
      case JsonValueKind.True:
      case JsonValueKind.False:
        claims.Add(new Claim(name, value.GetRawText()));

        break;

      case JsonValueKind.Undefined:
      case JsonValueKind.Object:
      case JsonValueKind.Null:
      default:
        break;
    }
  }
}
