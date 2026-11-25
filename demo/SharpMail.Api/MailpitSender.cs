using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SharpMail;

namespace SharpMail.Api;

internal sealed class MailpitSender(IConfiguration configuration) : IEmailSender
{
    public string Transport => "local-mailpit";

    public async Task<SdkOutcome> SubmitAsync(Guid attemptId, EmailMessage message, CancellationToken cancellationToken)
    {
        var endpoint = new Uri(configuration["MailpitEndpoint"] ?? throw new InvalidOperationException("Mailpit endpoint missing."));
        using var mime = new MimeMessage();
        mime.From.Add(Address(message.Sender));
        mime.To.AddRange(message.To.Select(Address));
        mime.Cc.AddRange(message.Cc.Select(Address));
        mime.Bcc.AddRange(message.Bcc.Select(Address));
        if (message.ReplyTo is { } reply) mime.ReplyTo.Add(Address(reply));
        mime.Subject = message.Content.Subject;
        mime.Body = new BodyBuilder { HtmlBody = message.Content.HtmlBody, TextBody = message.Content.TextBody }.ToMessageBody();
        mime.MessageId = $"{attemptId:D}@sharpmail.invalid";
        mime.Headers.Add("X-SharpMail-Attempt", attemptId.ToString("D"));
        using var client = new SmtpClient();
        await client.ConnectAsync(endpoint.Host, endpoint.Port, SecureSocketOptions.None, cancellationToken);
        await client.SendAsync(new MailKit.SendRequest(mime), cancellationToken);
        return new(true, mime.MessageId);
    }

    private static MailboxAddress Address(Mailbox mailbox) => new(mailbox.DisplayName ?? "", mailbox.Address);
}
