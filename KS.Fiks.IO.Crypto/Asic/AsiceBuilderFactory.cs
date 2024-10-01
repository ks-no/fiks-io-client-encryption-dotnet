namespace KS.Fiks.IO.Crypto.Asic;

public class AsiceBuilderFactory : IAsiceBuilderFactory
{
    public IAsiceBuilder<AsiceArchive> GetBuilder(
        Stream outStream,
        MessageDigestAlgorithm messageDigestAlgorithm)
    {
        return AsiceBuilder.Create(outStream, messageDigestAlgorithm, null);
    }

    public IAsiceBuilder<AsiceArchive> GetBuilder(
        Stream outStream,
        MessageDigestAlgorithm messageDigestAlgorithm,
        ICertificateHolder? certificateHolder)
    {
        return AsiceBuilder.Create(outStream, messageDigestAlgorithm, certificateHolder);
    }
}