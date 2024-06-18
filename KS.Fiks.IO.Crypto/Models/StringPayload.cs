namespace KS.Fiks.IO.Crypto.Models;

public class StringPayload(string payload, string filename) : IPayload
{
    public string Filename { get; } = filename;

    public Stream Payload => new MemoryStream(Encoding.UTF8.GetBytes(payload));
}