namespace KS.Fiks.IO.Crypto.Models;

public class StreamPayload(Stream payload, string filename) : IPayload
{
    public string Filename { get; } = filename;

    public Stream Payload { get; } = payload;
}