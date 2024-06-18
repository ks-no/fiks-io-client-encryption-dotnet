namespace KS.Fiks.IO.Encryption.Exceptions;

public class FiksIODecryptionException : Exception
{
    public FiksIODecryptionException(string message) : base(message)
    {
    }

    public FiksIODecryptionException()
    {
    }

    public FiksIODecryptionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}