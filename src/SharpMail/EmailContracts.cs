using System.Collections.Immutable;
using System.Net.Mail;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SharpMail;

public sealed record Mailbox(string Address, string? DisplayName = null);

public sealed record RenderedContent(string Subject, string? HtmlBody, string? TextBody);

public sealed record EmailMessage(Mailbox Sender, ImmutableArray<Mailbox> To,
    ImmutableArray<Mailbox> Cc, ImmutableArray<Mailbox> Bcc, Mailbox? ReplyTo, RenderedContent Content)
{
    public void Validate()
    {
        ArgumentNullException.ThrowIfNull(Sender);
        ArgumentNullException.ThrowIfNull(Content);
        if (To.IsDefault || Cc.IsDefault || Bcc.IsDefault || To.Length + Cc.Length + Bcc.Length == 0)
            throw new ArgumentException("Supply recipient lists and at least one recipient.");
        if (string.IsNullOrWhiteSpace(Content.Subject) || Content.Subject.IndexOfAny(['\r', '\n']) >= 0)
            throw new ArgumentException("A single-line subject is required.");
        if (Content.HtmlBody is null && Content.TextBody is null)
            throw new ArgumentException("Supply an HTML or plain-text body.");
        foreach (var mailbox in To.Concat(Cc).Concat(Bcc).Append(Sender).Concat(ReplyTo is null ? [] : new[] { ReplyTo }))
        {
            if (mailbox is null || !MailAddress.TryCreate(mailbox.Address, out var parsed)
                || parsed.Address != mailbox.Address || mailbox.DisplayName?.IndexOfAny(['\r', '\n']) >= 0)
                throw new ArgumentException("Invalid mailbox.");
        }
    }
}

public enum SubmissionStatus { Pending, Accepted, Failed }

public sealed record SdkOutcome(bool Accepted, string? ProviderMessageId = null, string? FailureCode = null);

public sealed record EmailAttempt(Guid AttemptId, string? CorrelationId, string Transport,
    EmailMessage Message, DateTimeOffset CreatedAtUtc, SubmissionStatus Status = SubmissionStatus.Pending,
    DateTimeOffset? CompletedAtUtc = null, DateTimeOffset? AcceptedBySdkAtUtc = null,
    string? ProviderMessageId = null, string? FailureCode = null, int SchemaVersion = 1);

public enum DeliveryKind { Delivered, Deferred, Bounced }

public sealed record DeliveryNotification(string EventId, string RecipientRole, int RecipientIndex,
    DeliveryKind Kind, DateTimeOffset OccurredAtUtc, string? Reason = null);

public sealed record DeliveryEvent(DeliveryNotification Notification, DateTimeOffset RecordedAtUtc);
public sealed record AuditDetail(EmailAttempt Attempt, IReadOnlyList<DeliveryEvent> DeliveryEvents);
public sealed record SubmissionResult(EmailAttempt Attempt, bool Reused, bool OutcomePersisted);

public interface IEmailSender
{
    string Transport { get; }
    Task<SdkOutcome> SubmitAsync(Guid attemptId, EmailMessage message, CancellationToken cancellationToken);
}

public interface ITemplateRenderer
{
    Task<RenderedContent> RenderAsync(string templateKey, IReadOnlyDictionary<string, string> model,
        CancellationToken cancellationToken);
}

public interface IAuditStore
{
    Task<bool> TryCreateAsync(EmailAttempt attempt, CancellationToken cancellationToken);
    Task<EmailAttempt?> FindAsync(Guid attemptId, CancellationToken cancellationToken);
    Task CompleteAsync(EmailAttempt completed, CancellationToken cancellationToken);
    Task<IReadOnlyList<Guid>> FindByProviderIdAsync(string transport, string messageId, CancellationToken cancellationToken);
    Task<bool> TryAppendAsync(Guid attemptId, DeliveryEvent delivery, CancellationToken cancellationToken);
    Task<IReadOnlyList<DeliveryEvent>> ReadEventsAsync(Guid attemptId, CancellationToken cancellationToken);
}

public static class AuditJson
{
    public static JsonSerializerOptions Options { get; } = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        options.MakeReadOnly(populateMissingResolver: true);
        return options;
    }

    public static string Write<T>(T value) => JsonSerializer.Serialize(value, Options);
    public static T Read<T>(string json) => JsonSerializer.Deserialize<T>(json, Options)!;
}
