namespace Backend.SharedKernel.Exceptions;
public class EmailSendException : Exception
{
    public EmailSendException(string message, Exception innerException) : base(message, innerException) { }
}