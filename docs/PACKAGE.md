# Branca

A .NET Standard 2.1 library for generating and validating [Branca](https://branca.io) tokens.

Branca tokens are authenticated and encrypted API tokens using modern crypto. They use IETF XChaCha20-Poly1305 AEAD symmetric encryption to create encrypted and tamperproof tokens, base62 encoded to be URL safe. The payload is an arbitrary sequence of bytes: a JSON object, plain text or binary data serialized by MessagePack or Protocol Buffers.

## Usage

Configure a `BrancaService` with a 32-byte secret key, given either as a `byte[]` or as a 64-character hexadecimal `HexKey`:

```c#
using Branca;

HexKey key = "73757065727365637265746b6579796f7573686f756c646e6f74636f6d6d6974";

BrancaService branca = new(key);
```

Encode a payload into a token:

```c#
string token = branca.Encode("Hello, World!");
```

Decode and validate a token:

```c#
if (branca.TryDecode(token, out byte[] payload))
{
  string message = Encoding.UTF8.GetString(payload);
}
```

Tokens are valid for an hour by default; configure `BrancaSettings` to change the token lifetime and other options. A single `BrancaService` instance is safe for concurrent use.

See the [repository](https://github.com/AmanAgnihotri/Branca) for detailed documentation and examples.

## License

Branca is free software, licensed under the GNU Lesser General Public License v3.0 or later.
