using System.Text;
using KS.Fiks.ASiC_E.Crypto;
using KS.Fiks.ASiC_E.Model;
using KS.Fiks.IO.Crypto.Models;
using Moq;
using Org.BouncyCastle.X509;
using Shouldly;

namespace KS.Fiks.IO.Crypto.Tests.Asic;

public class AsicEncrypterTests : IDisposable
{
    private AsicEncrypterFixture _fixture;
    private readonly Mock<X509Certificate> _publicKeyMock = new();

    public AsicEncrypterTests()
    {
        _fixture = new AsicEncrypterFixture();
    }

    [Fact]
    public void ReturnsANonNullStream()
    {
        var sut = _fixture.CreateSut();

        var payload = new StreamPayload(_fixture.RandomStream, "filename.file");

        var outStream = sut.Encrypt(_publicKeyMock.Object, new List<IPayload> {payload});

        outStream.ShouldNotBeNull();
    }

    [Fact]
    public void CallsAsiceBuilderAddFile()
    {
        var sut = _fixture.CreateSut();

        var payload = new StreamPayload(_fixture.RandomStream, "filename.file");

        var outStream = sut.Encrypt(_publicKeyMock.Object, new List<IPayload> {payload});

        _fixture.AsiceBuilderMock.Verify(_ => _.AddFile(payload.Payload, payload.Filename));
    }

    [Fact]
    public void AsiceBuilderIsDisposed()
    {
        var sut = _fixture.CreateSut();

        var payload = new StreamPayload(_fixture.RandomStream, "filename.file");

        var outStream = sut.Encrypt(_publicKeyMock.Object, new List<IPayload> {payload});

        _fixture.AsiceBuilderMock.Verify(_ => _.Dispose());
    }

    [Fact]
    public void ReturnsExpectedStream()
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
    public void CallsEncryptWithoutSigning()
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
    public void CallsEncryptWithSigning()
    {
        var expectedOutputString = "myStringToSend";
        var expectedZipStream = new MemoryStream(Encoding.UTF8.GetBytes(expectedOutputString));

        var sut = _fixture.WithContentAsZipStreamed(expectedZipStream).CreateSutWithAsicSigning();

        var payload = new StreamPayload(_fixture.RandomStream, "filename.file");

        var outStream = sut.Encrypt(_publicKeyMock.Object, new List<IPayload> {payload});

        _fixture.EncryptionServiceMock.Verify(
            _ => _.Encrypt(
                It.IsAny<Stream>(),
                It.IsAny<Stream>()));

        _fixture.AsiceBuilderFactoryMock.Verify(
            _ => _.GetBuilder(It.IsAny<Stream>(),
                It.IsAny<MessageDigestAlgorithm>(),
                It.IsAny<ICertificateHolder>()));

    }

    [Fact]
    public void ThrowsIfPayloadIsEmpty()
    {
        var sut = _fixture.CreateSut();
        Assert.Throws<ArgumentException>(() => { sut.Encrypt(_publicKeyMock.Object, new List<IPayload>()); });
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