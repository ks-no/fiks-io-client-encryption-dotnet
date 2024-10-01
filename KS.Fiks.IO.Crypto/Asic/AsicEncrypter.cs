using KS.Fiks.IO.Crypto.Models;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace KS.Fiks.IO.Crypto.Asic;

public class AsicEncrypter(
    IAsiceBuilderFactory asiceBuilderFactory,
    IEncryptionServiceFactory encryptionServiceFactory,
    ICertificateHolder? signingCertificateHolder = null)
    : IAsicEncrypter
{
    private readonly IAsiceBuilderFactory _asiceBuilderFactory = asiceBuilderFactory ?? new AsiceBuilderFactory();

    public Stream Encrypt(X509Certificate publicKey, IList<IPayload> payloads)
    {
        if (publicKey == null)
        {
            throw new ArgumentNullException(nameof(publicKey));
        }
        
        ThrowIfEmpty(payloads);
        return ZipAndEncrypt(publicKey, payloads);
    }

    private static void ThrowIfEmpty(IEnumerable<IPayload> payloads)
    {
        if (payloads == null)
        {
            throw new ArgumentNullException(nameof(payloads));
        }

        if (!payloads.Any())
        {
            throw new ArgumentException("Payloads cannot be empty");
        }
    }

    private Stream ZipAndEncrypt(X509Certificate publicKey, IEnumerable<IPayload> payloads)
    {
        var outStream = new MemoryStream();
        var encryptionService = encryptionServiceFactory.Create(publicKey);
        using var zipStream = new MemoryStream();
        
        if (signingCertificateHolder == null)
        {
            BuildAsiceWithoutSigning(payloads, zipStream);
        }
        else
        {
            BuildAsiceWithSigning(payloads, zipStream);
        }

        zipStream.Seek(0, SeekOrigin.Begin);
        encryptionService.Encrypt(zipStream, outStream);

        return outStream;
    }

    private void BuildAsiceWithoutSigning(IEnumerable<IPayload> payloads, MemoryStream zipStream)
    {
        using var asiceBuilder = _asiceBuilderFactory.GetBuilder(zipStream, MessageDigestAlgorithm.SHA256);
        BuildAsice(payloads, asiceBuilder);
    }

    private void BuildAsiceWithSigning(IEnumerable<IPayload> payloads, MemoryStream zipStream)
    {
        using var asiceBuilder = _asiceBuilderFactory.GetBuilder(zipStream, MessageDigestAlgorithm.SHA256, signingCertificateHolder);
        BuildAsice(payloads, asiceBuilder);
    }

    private static void BuildAsice(IEnumerable<IPayload> payloads, IAsiceBuilder<AsiceArchive> asiceBuilder)
    {
        foreach (var payload in payloads)
        {
            payload.Payload.Seek(0, SeekOrigin.Begin);
            asiceBuilder.AddFile(payload.Payload, payload.Filename);
            asiceBuilder.Build();
        }
    }
}