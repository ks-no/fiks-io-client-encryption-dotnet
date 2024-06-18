namespace KS.Fiks.IO.Encryption.Asic;

public interface IAsicDecrypter
{
    Task WriteDecrypted(Task<Stream> encryptedZipStream, string outPath);

    Task<Stream> Decrypt(Task<Stream> encryptedZipStream);

    Task<IEnumerable<IPayload>> DecryptAndExtractPayloads(Task<Stream> encryptedZipStream);
}