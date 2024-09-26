using KS.Fiks.ASiC_E;
using KS.Fiks.ASiC_E.Crypto;
using KS.Fiks.ASiC_E.Model;
using KS.Fiks.Crypto;
using KS.Fiks.IO.Crypto.Asic;
using KS.Fiks.IO.Crypto.Models;
using Moq;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using Shouldly;

namespace KS.Fiks.IO.Crypto.Tests;

public class AsicEncrypterTests
{
    private readonly Mock<IAsiceBuilderFactory> _mockAsiceBuilderFactory;
    private readonly Mock<IEncryptionServiceFactory> _mockEncryptionServiceFactory;
    private readonly Mock<ICertificateHolder> _mockCertificateHolder;
    private readonly AsicEncrypter _asicEncrypter;

    public AsicEncrypterTests()
    {
        _mockAsiceBuilderFactory = new Mock<IAsiceBuilderFactory>();
        _mockEncryptionServiceFactory = new Mock<IEncryptionServiceFactory>();
        _mockCertificateHolder = new Mock<ICertificateHolder>();
        _asicEncrypter = new AsicEncrypter(_mockAsiceBuilderFactory.Object, _mockEncryptionServiceFactory.Object,
            _mockCertificateHolder.Object);
    }

    [Fact]
    public void Encrypt_ShouldThrowArgumentNullException_WhenPublicKeyIsNull()
    {
        X509Certificate publicKey = null;
        IList<IPayload> payloads = new List<IPayload> { new Mock<IPayload>().Object };

        Should.Throw<ArgumentNullException>(() => _asicEncrypter.Encrypt(publicKey, payloads));
    }

    [Fact]
    public void Encrypt_ShouldThrowArgumentException_WhenPayloadsIsEmpty()
    {
        X509Certificate publicKey = GenerateCertificate();
        IList<IPayload> payloads = new List<IPayload>();

        Should.Throw<ArgumentException>(() => _asicEncrypter.Encrypt(publicKey, payloads));
    }

    [Fact]
    public void Encrypt_ShouldThrowArgumentException_WhenPayloadsIsNull()
    {
        X509Certificate publicKey = GenerateCertificate();
        IList<IPayload> payloads = null;

        Should.Throw<ArgumentException>(() => _asicEncrypter.Encrypt(publicKey, payloads));
    }

    [Fact]
    public void Encrypt_ShouldNotThrow_WhenGivenValidInputs()
    {
        X509Certificate publicKey = GenerateCertificate();

        var mockPayload = new Mock<IPayload>();
        mockPayload.Setup(p => p.Payload).Returns(new MemoryStream("Test payload"u8.ToArray()));
        _mockAsiceBuilderFactory
            .Setup(b => b.GetBuilder(It.IsAny<MemoryStream>(), It.IsAny<MessageDigestAlgorithm>(),
                It.IsAny<ICertificateHolder>())).Returns(new Mock<IAsiceBuilder<AsiceArchive>>().Object);
        _mockEncryptionServiceFactory.Setup(f => f.Create(publicKey)).Returns(new Mock<IEncryptionService>().Object);
        IList<IPayload> payloads = new List<IPayload> { mockPayload.Object };

        Should.NotThrow(() => _asicEncrypter.Encrypt(publicKey, payloads));
    }

    public X509Certificate GenerateCertificate()
    {
        var randomGenerator = new SecureRandom();
        var certificateGenerator = new X509V3CertificateGenerator();

        var serialNumber =
            BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(long.MaxValue), randomGenerator);
        certificateGenerator.SetSerialNumber(serialNumber);

        var dirName = "CN=Test Certificate";
        var name = new X509Name(dirName);
        certificateGenerator.SetIssuerDN(name);
        certificateGenerator.SetSubjectDN(name);

        var notBefore = DateTime.UtcNow.Date;
        var notAfter = notBefore.AddYears(1);

        certificateGenerator.SetNotBefore(notBefore);
        certificateGenerator.SetNotAfter(notAfter);

        var keyGenerationParameters = new KeyGenerationParameters(randomGenerator, 2048);

        var keyPairGenerator = new RsaKeyPairGenerator();
        keyPairGenerator.Init(keyGenerationParameters);
        var subjectKeyPair = keyPairGenerator.GenerateKeyPair();

        certificateGenerator.SetPublicKey(subjectKeyPair.Public);

        var issuerKeyPair = subjectKeyPair;
        var signatureFactory = new Asn1SignatureFactory("SHA256WITHRSA", issuerKeyPair.Private);

        return certificateGenerator.Generate(signatureFactory);
    }
}