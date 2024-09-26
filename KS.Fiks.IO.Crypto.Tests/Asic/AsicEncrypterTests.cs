using System.Text;
using KS.Fiks.ASiC_E;
using KS.Fiks.ASiC_E.Crypto;
using KS.Fiks.ASiC_E.Model;
using KS.Fiks.Crypto;
using KS.Fiks.IO.Crypto.Models;
using Moq;
using Org.BouncyCastle.X509;
using Shouldly;

namespace KS.Fiks.IO.Crypto.Tests.Asic;

public class AsicEncrypterTests : IDisposable
{
    private readonly AsicEncrypterFixture _fixture = new();
    private readonly Mock<X509Certificate> _publicKeyMock = new();
    
    [Fact]
    public void Encrypt_ShouldThrowArgumentNullException_WhenPublicKeyIsNull()
    {
        var sut = _fixture.CreateSut();
        Should.Throw<ArgumentNullException>(() => sut.Encrypt(null, new List<IPayload>()));
    }

    [Fact]
    public void Encrypt_ShouldThrowArgumentException_WhenPayloadsIsEmpty()
    {
        var sut = _fixture.CreateSut();
        Should.Throw<ArgumentException>(() => sut.Encrypt(_publicKeyMock.Object, new List<IPayload>()));
    }

    [Fact]
    public void Encrypt_ShouldThrowArgumentException_WhenPayloadsIsNull()
    {
        var sut = _fixture.CreateSut();
        Should.Throw<ArgumentException>(() => sut.Encrypt(_publicKeyMock.Object, null));
    }

    [Fact]
    public void Encrypt_ShouldNotThrow_WhenGivenValidInputs()
    {
        var mockPayload = new Mock<IPayload>();
        mockPayload.Setup(p => p.Payload).Returns(new MemoryStream("Test payload"u8.ToArray()));
        _fixture.AsiceBuilderFactoryMock
            .Setup(b => b.GetBuilder(It.IsAny<MemoryStream>(), It.IsAny<MessageDigestAlgorithm>(),
                It.IsAny<ICertificateHolder>())).Returns(new Mock<IAsiceBuilder<AsiceArchive>>().Object);
        _fixture.EncryptionServiceFactoryMock.Setup(f => f.Create(_publicKeyMock.Object)).Returns(new Mock<IEncryptionService>().Object);
        IList<IPayload> payloads = new List<IPayload> { mockPayload.Object };
        var sut = _fixture.CreateSut();

        Should.NotThrow(() => sut.Encrypt(_publicKeyMock.Object, payloads));
    }

    [Fact]
    public void Encrypt_ReturnsNonNullStream_WhenValidInput()
    {
        var sut = _fixture.CreateSut();
        var payload = new StreamPayload(_fixture.RandomStream, "filename.file");

        var outStream = sut.Encrypt(_publicKeyMock.Object, new List<IPayload> {payload});

        outStream.ShouldNotBeNull();
    }

    [Fact]
    public void Encrypt_CallsAsiceBuilderAddFile_WhenValidInput()
    {
        var sut = _fixture.CreateSut();
        var payload = new StreamPayload(_fixture.RandomStream, "filename.file");

        var outStream = sut.Encrypt(_publicKeyMock.Object, new List<IPayload> {payload});

        _fixture.AsiceBuilderMock.Verify(builder => builder.AddFile(payload.Payload, payload.Filename));
    }

    [Fact]
    public void Encrypt_AsiceBuilderIsDisposed()
    {
        var sut = _fixture.CreateSut();
        var payload = new StreamPayload(_fixture.RandomStream, "filename.file");

        var outStream = sut.Encrypt(_publicKeyMock.Object, new List<IPayload> {payload});

        _fixture.AsiceBuilderMock.Verify(builder => builder.Dispose());
    }

    [Fact]
    public void Encrypt_ReturnsExpectedStream_WhenValidInput()
    {
        var expectedOutputString = "myStringToSend";
        var expectedOutStream = new MemoryStream(Encoding.UTF8.GetBytes(expectedOutputString));

        var sut = _fixture.WithContentAsEncryptedZipStreamed(expectedOutStream).CreateSut();

        var payload = new StreamPayload(_fixture.RandomStream, "filename.file");

        var outStream = sut.Encrypt(_publicKeyMock.Object, new List<IPayload> {payload});

        using var streamReader = new BinaryReader(outStream);
        var outStreamBytes = streamReader.ReadBytes((int) outStream.Length);
        var outputAsString = Encoding.UTF8.GetString(outStreamBytes);

        outputAsString.ShouldBe(expectedOutputString);
    }

    [Fact]
    public void Encrypt_CallsBuilderWithoutSigning()
    {
        var expectedOutputString = "myStringToSend";
        var expectedZipStream = new MemoryStream(Encoding.UTF8.GetBytes(expectedOutputString));

        var sut = _fixture.WithContentAsZipStreamed(expectedZipStream).CreateSut();

        var payload = new StreamPayload(_fixture.RandomStream, "filename.file");

        var outStream = sut.Encrypt(_publicKeyMock.Object, new List<IPayload> {payload});

        _fixture.EncryptionServiceMock.Verify(
            _ => _.Encrypt(
                It.IsAny<Stream>(),
                It.IsAny<Stream>()));

        _fixture.AsiceBuilderFactoryMock.Verify(
            _ => _.GetBuilder(It.IsAny<Stream>(),
                It.IsAny<MessageDigestAlgorithm>()));
    }

    [Fact]
    public void Encrypt_CallsBuilderWithSigning()
    {
        var expectedOutputString = "myStringToSend";
        var expectedZipStream = new MemoryStream(Encoding.UTF8.GetBytes(expectedOutputString));
        var sut = _fixture.WithContentAsZipStreamed(expectedZipStream).CreateSutWithAsicSigning();
        var payload = new StreamPayload(_fixture.RandomStream, "filename.file");

        sut.Encrypt(_publicKeyMock.Object, new List<IPayload> {payload});
        
        _fixture.EncryptionServiceMock.Verify(
            _ => _.Encrypt(
                It.IsAny<Stream>(),
                It.IsAny<Stream>()));

        _fixture.AsiceBuilderFactoryMock.Verify(
            _ => _.GetBuilder(It.IsAny<Stream>(),
                It.IsAny<MessageDigestAlgorithm>(),
                It.IsAny<ICertificateHolder>()));

    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _fixture.Dispose();
        }
    }
}