using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace KS.Fiks.IO.Encryption.Asic;

public interface IEncryptionServiceFactory
{
    IEncryptionService Create(X509Certificate certificate);
}