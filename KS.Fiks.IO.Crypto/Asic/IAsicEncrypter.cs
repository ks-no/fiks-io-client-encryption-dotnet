using KS.Fiks.IO.Crypto.Models;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace KS.Fiks.IO.Crypto.Asic;

public interface IAsicEncrypter
{
    Stream Encrypt(X509Certificate publicKey, IList<IPayload> payload);
}