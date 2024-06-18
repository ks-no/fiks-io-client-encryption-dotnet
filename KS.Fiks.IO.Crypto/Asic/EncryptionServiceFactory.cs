using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace KS.Fiks.IO.Encryption.Asic;

public class EncryptionServiceFactory : IEncryptionServiceFactory
{
    public IEncryptionService Create(X509Certificate certificate)
    {
        return EncryptionService.Create(certificate);
    }
}