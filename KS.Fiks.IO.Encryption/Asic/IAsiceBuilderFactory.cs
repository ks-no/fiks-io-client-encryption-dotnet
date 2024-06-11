namespace KS.Fiks.IO.Encryption.Asic;

public interface IAsiceBuilderFactory
{
    IAsiceBuilder<AsiceArchive> GetBuilder(
        Stream outStream,
        MessageDigestAlgorithm messageDigestAlgorithm);

    IAsiceBuilder<AsiceArchive> GetBuilder(
        Stream outStream,
        MessageDigestAlgorithm messageDigestAlgorithm,
        ICertificateHolder certificateHolder);
}