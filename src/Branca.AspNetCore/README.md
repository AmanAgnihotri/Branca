# Branca.AspNetCore

ASP.NET Core authentication for [Branca](https://branca.io) tokens, letting you protect endpoints with `[Authorize]` using encrypted Branca tokens instead of JWTs.

Branca tokens are symmetric: the service that validates a token can also issue one. That makes this a good fit for first-party session and API tokens, where the same application (or a trusted set sharing the secret key) both issues and validates. For federated scenarios that need third parties to validate without the secret, an asymmetric JWT remains the better tool.

## Usage

Register the scheme with a 32-byte key:

```c#
using Branca;

builder.Services
  .AddAuthentication(BrancaDefaults.AuthenticationScheme)
  .AddBranca(options =>
  {
    options.Key = BrancaKey.FromBase64Url(builder.Configuration["Branca:Key"]!);
    options.TokenLifetimeInSeconds = 3600;
  });

builder.Services.AddAuthorization();
```

```c#
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/me", (ClaimsPrincipal user) => user.Identity!.Name)
  .RequireAuthorization();
```

Clients send the token in the `Authorization` header:

```
Authorization: Bearer <branca-token>
```

## Claims

By default the decrypted payload is read as a flat JSON object, and each property becomes a claim. Array values (such as `"role": ["admin", "user"]`) yield one claim per element, so `[Authorize(Roles = "admin")]` works out of the box. The name and role claim types are configurable via `NameClaimType` and `RoleClaimType`, and the whole mapping can be replaced through `MapClaims` for non-JSON payloads such as MessagePack.

Token expiry is enforced by the token format itself through `TokenLifetimeInSeconds`; there is no separate `exp` claim to validate.

See the [repository](https://github.com/AmanAgnihotri/Branca) for more.

## License

Free software, licensed under the GNU Lesser General Public License v3.0 or later.
