using KS.Fiks.IO.Crypto.Exceptions;
using KS.Fiks.IO.Crypto.Models;

namespace KS.Fiks.IO.Crypto.Asic;

internal class AsicDecrypter(IDecryptionService decryptionService) : IAsicDecrypter
{
    public async Task WriteDecrypted(Task<Stream> encryptedZipStream, string outPath)
    {
        await using var fileStream = new FileStream(outPath, FileMode.OpenOrCreate);
        
        try
        {
            await decryptionService.Decrypt(await encryptedZipStream.ConfigureAwait(false))
                .CopyToAsync(fileStream).ConfigureAwait(false);
            await fileStream.FlushAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new FiksIODecryptionException("Unable to decrypt melding. Is your private key correct?", ex);
        }
    }

    public async Task<Stream> Decrypt(Task<Stream> encryptedZipStream)
    {
        try
        {
            return decryptionService.Decrypt(await encryptedZipStream.ConfigureAwait(false));
        }
        catch (Exception ex)
        {
            throw new FiksIODecryptionException("Unable to decrypt melding. Is your private key correct?", ex);
        }
    }

    public async Task<IEnumerable<IPayload>> DecryptAndExtractPayloads(Task<Stream> encryptedZipStream)
    {
        var payloads = new List<StreamPayload>();

        await using var stream = await Decrypt(encryptedZipStream).ConfigureAwait(false);
        var asiceReader = new AsiceReader().Read(stream);

        foreach (var entry in asiceReader.Entries)
        {
            await using var entryStream = entry.OpenStream();
            var memoryStream = new MemoryStream();

            await entryStream.CopyToAsync(memoryStream).ConfigureAwait(false);

            var payload = new StreamPayload(memoryStream, entry.FileName);

            payloads.Add(payload);
        }

        return payloads;
    }
}