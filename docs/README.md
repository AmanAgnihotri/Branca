<p align="center"><img src="/docs/logo.png" width=300px height=300px/></p>

# Branca
> A .NET 5 library for generating and validating Branca tokens.

![Status][1] [![Nuget][2]][5] [![Downloads][3]][5] ![License][4]

### Overview

Branca tokens are authenticated and encrypted API tokens using modern crypto.

From [Branca.io][6]:

> Branca is a secure easy to use token format which makes it hard to shoot yourself in the foot. It uses IETF XChaCha20-Poly1305 AEAD symmetric encryption to create encrypted and tamperproof tokens. The encrypted token is base62 encoded which makes it URL safe. Payload itself is an arbitrary sequence of bytes. You could use a JSON object, plain text string or even binary data serialized by [MessagePack][8] or [Protocol Buffers][9].

You can read the [Branca Token Specification][7] for design-specific details.

### Usage

```c#
using Branca;
```

#### Issuing Secret Key

The secret key is to be 32 bytes in size. You can generate a random one using the following:

```c#
byte[] key = new byte[32];
RandomNumberGenerator.Fill(key);
```

Alternatively, you can setup a hexadecimal string consisting of exactly 64 characters:

```c#
HexKey key = "73757065727365637265746b6579796f7573686f756c646e6f74636f6d6d6974";
```

#### Configuring Branca

Once you have a secret key, be it a `byte[]` type or a `HexKey` type, you can configure Branca as follows:

```c#
BrancaService branca = new(key);
```

Branca can optionally take in `BrancaSettings`, which has a few parameters for configuration. They come with sensible defaults so you may not need to change it at all.

```c#
BrancaService branca = new(key, new BrancaSettings
{
  MaxStackLimit = 1024,
  TokenLifetimeInSeconds = 3600
});
```

For the sake of performance, Branca allocates various resources during encoding and decoding on the stack. To prevent abuse of the stack, a stack limit is enforced. If the limit is reached due to a large payload, it defaults to allocating it on the heap for the particular encoding/decoding. This limit is by default set to 1024 bytes.

Branca also considers all tokens it generates to be valid for an hour from the time of their generation. You can configure this value based on your use-case. Alternatively, set it up as `null` to make Branca tokens valid forever.

There exists a `Timer` configuration for `BrancaSettings` too. It comes with an internal implementation that uses current time accordingly. You can pass on your own implementation of this interface if you want to control the way time flows for Branca. This will not be needed in all general cases.

#### Encoding/Encrypting Payload

Now that Branca is configured, you can encode your payload. Branca can accept `string` or `ReadOnlySpan<byte>` as payload. So, you can send a `byte[]` as payload if you are using some binary serialiser to save space.

If your payload is `"Hello, World"`, you can generate a Branca token as follows:

```c#
string token = branca.Encode("Hello, World!");
```

The above will generate a token like:

```
XZAXYkd0ZbbCjxgr5xOelGcPXWhNFUy1oNfbQ3vfbi5LL4TIeyGq5rBKESaXXpyfjNBSOjbaOTlhWG
```

Likewise, you can make use of MessagePack or Protocol Buffer or even System.Text.Json to serialize your payload and pass in binary data to branca for generating a specific token.

#### Decoding/Decrypting Payload

Decoding a Branca token for validation is just as simple:

```c#
if (branca.TryDecode(token, out Span<byte> payload))
{
  // Hello, World!
  string message = Encoding.UTF8.GetString(payload);
}
```

You get to decide how to interpret the payload which comes out as an array of bytes. If you used MessagePack, deserialize the payload using it to the object it is supposed to be.

Decoding validates the token and even confirms the expiry (if you have setup some lifetime for the token, default being one hour). In case of failure, the `TryDecode` returns false and you can reject the token accordingly.

For whatever reason, if you also want to get the time the Branca token was created, you can do the following:

```c#
if (branca.TryDecode(token, out Span<byte> payload, out uint createTime))
{
  // successful decoding
}
else
{
  // unauthorized attempt
}
```

---

### License

Branca is a .NET 5 library for generating and validating Branca tokens.  
Copyright © 2021 Aman Agnihotri (amanagnihotri@pm.me)  

Branca is free software: you can redistribute it and/or modify  
it under the terms of the GNU General Public License as published  
by the Free Software Foundation, either version 3 of the License, or  
(at your option) any later version.  

Branca is distributed in the hope that it will be useful,  
but WITHOUT ANY WARRANTY; without even the implied warranty of  
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the  
GNU General Public License for more details.  

You should have received a copy of the GNU General Public License  
along with Branca. If not, see [GNU Licenses](https://www.gnu.org/licenses/).  

---

[1]: https://img.shields.io/github/workflow/status/AmanAgnihotri/Branca/.NET?style=for-the-badge
[2]: https://img.shields.io/nuget/v/Branca?style=for-the-badge
[3]: https://img.shields.io/nuget/dt/Branca?style=for-the-badge
[4]: https://img.shields.io/github/license/AmanAgnihotri/Branca?style=for-the-badge
[5]: https://www.nuget.org/packages/Branca/
[6]: https://branca.io
[7]: https://github.com/tuupola/branca-spec
[8]: https://msgpack.org/
[9]: https://developers.google.com/protocol-buffers/
