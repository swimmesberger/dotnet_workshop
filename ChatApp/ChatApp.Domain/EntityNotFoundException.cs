namespace CAP;

public sealed class EntityNotFoundException : Exception {
    public EntityNotFoundException() { }

    public EntityNotFoundException(string? message) : base(message) { }

    public EntityNotFoundException(string? message, Exception? innerException) : base(message, innerException) { }
    
    public EntityNotFoundException(Exception? innerException) : base(null, innerException) { }
}
