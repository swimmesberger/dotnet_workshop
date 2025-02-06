namespace ChatApp.Domain;

public sealed class ConcurrencyException : Exception {
    public ConcurrencyException() { }

    public ConcurrencyException(string? message) : base(message) { }

    public ConcurrencyException(string? message, Exception? innerException) : base(message, innerException) { }
    
    public ConcurrencyException(Exception? innerException) : base(null, innerException) { }
}
