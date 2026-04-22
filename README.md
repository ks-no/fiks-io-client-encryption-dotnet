# KS.Fiks.IO.Crypto

[![NuGet](https://img.shields.io/nuget/v/KS.Fiks.IO.Crypto.svg)](https://www.nuget.org/packages/KS.Fiks.IO.Crypto)
[![Build](https://github.com/ks-no/fiks-io-client-encryption-dotnet/actions/workflows/dotnet-build-release.yml/badge.svg)](https://github.com/ks-no/fiks-io-client-encryption-dotnet/actions/workflows/dotnet-build-release.yml)
[![License: MIT](https://img.shields.io/github/license/ks-no/fiks-io-client-encryption-dotnet)](LICENSE)

.NET library for ASiC-E signing and encryption used by [fiks-io-client-dotnet](https://github.com/ks-no/fiks-io-client-dotnet) and [fiks-io-send-client-dotnet](https://github.com/ks-no/fiks-io-send-client-dotnet).

## Prerequisites

Targets `net8.0`, `netstandard2.1`, and `netstandard2.0`.

## Installation

```sh
dotnet add package KS.Fiks.IO.Crypto
```

## Usage

### Encrypting payloads

Use `AsicEncrypter` to bundle one or more payloads into an ASiC-E archive and encrypt it with the recipient's X.509 public key.

```csharp
using KS.Fiks.IO.Crypto.Asic;
using KS.Fiks.IO.Crypto.Models;
using Org.BouncyCastle.X509;

X509Certificate recipientPublicKey = /* load from certificate */;

var encrypter = new AsicEncrypter(
    new AsiceBuilderFactory(),
    new EncryptionServiceFactory());

IList<IPayload> payloads = new List<IPayload>
{
    new StreamPayload(File.OpenRead("document.pdf"), "document.pdf"),
    new StringPayload("Hello, Fiks IO!", "message.txt"),
};

Stream encryptedStream = encrypter.Encrypt(recipientPublicKey, payloads);
```

### Encrypting with ASiC-E signing

Pass an `AsiceSigningConfiguration` to sign the archive before encryption.

```csharp
using KS.Fiks.IO.Crypto.Asic;
using KS.Fiks.IO.Crypto.Configuration;

// Using PEM key files
var signingConfig = new AsiceSigningConfiguration(
    publicKeyPath: "signing-cert.pem",
    privateKeyPath: "signing-key.pem");

// Or using an X509Certificate2 that holds the private key
var signingConfig = new AsiceSigningConfiguration(myCertificate);

var signingCertHolder = AsicSigningCertificateHolderFactory.Create(signingConfig);

var encrypter = new AsicEncrypter(
    new AsiceBuilderFactory(),
    new EncryptionServiceFactory(),
    signingCertHolder);

Stream encryptedStream = encrypter.Encrypt(recipientPublicKey, payloads);
```

### Decrypting

Use `AsicDecrypter` to decrypt an incoming ASiC-E stream.

```csharp
using KS.Fiks.IO.Crypto.Asic;
using KS.Fiks.Crypto;

IDecryptionService decryptionService = /* provided by KS.Fiks.Crypto */;
var decrypter = new AsicDecrypter(decryptionService);

// Decrypt to a stream
Stream decryptedStream = await decrypter.Decrypt(encryptedStreamTask);

// Decrypt and write directly to a file
await decrypter.WriteDecrypted(encryptedStreamTask, "output.zip");

// Decrypt and extract individual payloads
IEnumerable<IPayload> payloads = await decrypter.DecryptAndExtractPayloads(encryptedStreamTask);
foreach (var payload in payloads)
{
    Console.WriteLine(payload.Filename);
    // payload.Payload is a Stream with the file contents
}
```

## Payload types

| Type | Description |
|------|-------------|
| `StreamPayload(Stream, string filename)` | Wrap an existing stream |
| `FilePayload(string filePath)` | Read from a file on disk |
| `StringPayload(string content, string filename)` | Encode a string as UTF-8 |

## License

[MIT](LICENSE) — Copyright (c) 2019 KS
