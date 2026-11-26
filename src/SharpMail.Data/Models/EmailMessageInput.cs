using System.Collections.Immutable;
using System.Net.Mail;

namespace SharpMail.Data.Models;

public sealed record EmailMessageInput(
    MailboxInput Sender,
    ImmutableArray<MailboxInput> To,
    ImmutableArray<MailboxInput> Cc,
    ImmutableArray<MailboxInput> Bcc,
    MailboxInput? ReplyTo,
    TemplateRenderResult Content)
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
