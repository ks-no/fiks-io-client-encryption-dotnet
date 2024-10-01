namespace KS.Fiks.IO.Crypto.Asic;

public interface IAsiceBuilderFactory
{
    IAsiceBuilder<AsiceArchive> GetBuilder(
        Stream outStream,
        MessageDigestAlgorithm messageDigestAlgorithm);

    IAsiceBuilder<AsiceArchive> GetBuilder(
        Stream outStream,
        MessageDigestAlgorithm messageDigestAlgorithm,
        ICertificateHolder? certificateHolder);
}