using KS.Fiks.IO.Crypto.Models;

namespace KS.Fiks.IO.Crypto.Asic;

public interface IAsicDecrypter
{
    Task WriteDecrypted(Task<Stream> encryptedZipStream, string outPath);

    Task<Stream> Decrypt(Task<Stream> encryptedZipStream);

    Task<IEnumerable<IPayload>> DecryptAndExtractPayloads(Task<Stream> encryptedZipStream);
}