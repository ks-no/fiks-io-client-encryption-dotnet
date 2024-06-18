namespace KS.Fiks.IO.Encryption.Models;

public class StringPayload(string payload, string filename) : IPayload
{
    public string Filename { get; } = filename;

    public Stream Payload => new MemoryStream(Encoding.UTF8.GetBytes(payload));
}