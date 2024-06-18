namespace KS.Fiks.IO.Encryption.Models;

public interface IPayload
{
    string Filename { get; }

    Stream Payload { get; }
}