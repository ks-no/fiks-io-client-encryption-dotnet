using KS.Fiks.IO.Crypto.Configuration;

namespace KS.Fiks.IO.Crypto.Asic;

public static class AsicSigningCertificateHolderFactory
{
    public static PreloadedCertificateHolder Create(AsiceSigningConfiguration configuration)
    {
        return configuration.Certificate != null ? Create(configuration.Certificate) : Create(configuration.PublicKeyPath, configuration.PrivateKeyPath);
    }

    private static PreloadedCertificateHolder Create(string publicKeyPath, string privateKeyPath)
    {
        using var publicKeyStream = new FileStream(publicKeyPath, FileMode.Open, FileAccess.Read);
        using var privateKeyStream = new FileStream(privateKeyPath, FileMode.Open, FileAccess.Read);
        using var publicKeyBufferStream = new MemoryStream();
        using var privateKeyBufferStream = new MemoryStream();
        
        publicKeyStream.CopyTo(publicKeyBufferStream);
        privateKeyStream.CopyTo(privateKeyBufferStream);
        return PreloadedCertificateHolder.Create(publicKeyBufferStream.ToArray(),
            privateKeyBufferStream.ToArray());
    }

    private static PreloadedCertificateHolder Create(X509Certificate2 x509Certificate2)
    {
        return PreloadedCertificateHolder.Create(x509Certificate2);
    }
}