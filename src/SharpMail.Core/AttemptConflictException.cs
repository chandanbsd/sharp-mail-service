namespace SharpMail.Core;

public sealed class AttemptConflictException(Guid id) : Exception($"Attempt {id} already has different message content.");
