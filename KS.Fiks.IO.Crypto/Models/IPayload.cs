namespace KS.Fiks.IO.Crypto.Models;

public interface IPayload
{
    string Filename { get; }

    Stream Payload { get; }
}