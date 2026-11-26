#:property PublishAot=false
#:project ../src/SharpMail.Core/SharpMail.Core.csproj

using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using SharpMail.Core;
using SharpMail.Data;

namespace SharpMail.Demo;

internal sealed class Walkthrough(HttpClient api, HttpClient inbox)
{
    public static async Task<int> Main(string[] args)
    {
        var ports = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["sql"] = 18081,
            ["blobs"] = 18082,
            ["cosmos"] = 18083
        };
        var stores = args.Length == 0 ? ports.Keys.ToArray() : args;
        if (stores.Any(store => !ports.ContainsKey(store)))
        {
            Console.Error.WriteLine("Usage: dotnet run demo/Walkthrough.cs -- [sql] [blobs] [cosmos]");
            return 2;
        }

        using var inbox = new HttpClient { BaseAddress = new Uri("http://localhost:18025") };
        foreach (var store in stores)
        {
            using var api = new HttpClient { BaseAddress = new Uri($"http://localhost:{ports[store]}") };
            await new Walkthrough(api, inbox).RunAsync();
        }
        return 0;
    }

    private async Task RunAsync()
    {
        using var preview = await api.PostAsJsonAsync("/preview", new
        {
            templateKey = "orders/receipt",
            model = new { orderNumber = "42", customerName = "Pat & Jo" }
        });
        var content = await ReadAsync<RenderedContent>(preview);
        Require(content.HtmlBody?.Contains("Pat &amp; Jo") == true && content.TextBody?.Contains("Pat & Jo") == true,
            "Expected HTML escaping and plain-text rendering.");
        var message = new EmailMessage(new("store@demo.invalid"), [new("customer@demo.invalid")],
            [new("accounts@demo.invalid")], [new("archive@demo.invalid")], null, content);

        await VerifyReceiptAsync(message);
        await VerifyRejectionAsync(message);
        await VerifyConcurrentSubmissionsAsync(message);
    }

    private async Task VerifyReceiptAsync(EmailMessage message)
    {
        var url = await NewAttemptAsync();
        using var first = await SubmitAsync(url, message);
        var receipt = await ReadAsync<SubmissionResult>(first);
        Require(receipt.Attempt.Status == SubmissionStatus.Accepted && receipt.OutcomePersisted, "Expected persisted acceptance.");
        using var repeat = await SubmitAsync(url, message);
        Require((await ReadAsync<SubmissionResult>(repeat)).Reused, "Expected the existing attempt.");
        using var conflict = await SubmitAsync(url, message with { Content = message.Content with { Subject = "Changed" } });
        Require(conflict.StatusCode == HttpStatusCode.Conflict, "Changed content must conflict.");

        foreach (var (role, kind) in new[] { ("To", DeliveryKind.Delivered), ("Cc", DeliveryKind.Bounced) })
        {
            var callback = new
            {
                transport = "local-mailpit",
                receipt.Attempt.ProviderMessageId,
                notification = new DeliveryNotification($"event-{kind}", role, 0, kind, DateTimeOffset.UtcNow)
            };
            foreach (var expectedAppend in new[] { true, false })
            {
                using var response = await api.PostAsJsonAsync("/delivery-events", callback, AuditJson.Options);
                Require((await ReadAsync<JsonObject>(response))["appended"]!.GetValue<bool>() == expectedAppend, "Unexpected callback deduplication result.");
            }
        }

        var detail = await LookupAsync(url);
        Require(detail.Attempt.Message.Content == message.Content && detail.Attempt.AcceptedBySdkAtUtc.HasValue,
            "Expected the original receipt and acceptance time.");
        Require(detail.DeliveryEvents.Count == 2, "Expected two distinct delivery events.");
        await VerifySingleCaptureAsync(detail.Attempt.AttemptId);
        Console.WriteLine($"{url} — accepted once, original content retained, two delivery events");
    }

    private async Task VerifyRejectionAsync(EmailMessage message)
    {
        var url = await NewAttemptAsync();
        using var response = await SubmitAsync(url, message with { To = [new("rejected@example.test")], Cc = [], Bcc = [] });
        var result = await ReadAsync<SubmissionResult>(response);
        Require(result.OutcomePersisted && result.Attempt.Status == SubmissionStatus.Failed && result.Attempt.AcceptedBySdkAtUtc is null,
            "Expected persisted rejection without an acceptance time.");
        Console.WriteLine($"{url} — local SMTP rejection recorded");
    }

    private async Task VerifyConcurrentSubmissionsAsync(EmailMessage message)
    {
        var url = await NewAttemptAsync();
        var subjects = await Task.WhenAll(Enumerable.Range(1, 6).Select(async number =>
        {
            var content = new RenderedContent($"Concurrent receipt {number}", null, "Plain receipt");
            using var response = await SubmitAsync(url, message with { Content = content });
            if (response.StatusCode == HttpStatusCode.Conflict) return null;
            var result = await ReadAsync<SubmissionResult>(response);
            Require(result.Attempt.Status == SubmissionStatus.Accepted && result.OutcomePersisted, "Expected a persisted winner.");
            return content.Subject;
        }));
        var winner = subjects.OfType<string>().Single();
        var detail = await LookupAsync(url);
        Require(detail.Attempt.Message.Content.Subject == winner, "The stored subject must match the winning submission.");
        await VerifySingleCaptureAsync(detail.Attempt.AttemptId);
        Console.WriteLine($"{url} — six competing submissions, one accepted message");
    }

    private async Task<Uri> NewAttemptAsync()
    {
        using var response = await api.PostAsync("/attempt-ids", null);
        var reference = await ReadAsync<JsonObject>(response);
        return new Uri(api.BaseAddress!, $"/attempts/{reference["attemptId"]!.GetValue<Guid>()}");
    }

    private Task<HttpResponseMessage> SubmitAsync(Uri url, EmailMessage message) =>
        api.PutAsJsonAsync(url, new { correlationId = "order-42", message }, AuditJson.Options);

    private async Task<AuditDetail> LookupAsync(Uri url)
    {
        using var response = await api.GetAsync(url);
        return await ReadAsync<AuditDetail>(response);
    }

    private async Task VerifySingleCaptureAsync(Guid id)
    {
        using var response = await inbox.GetAsync($"/api/v1/search?query=message-id:{id}@sharpmail.invalid");
        Require((await ReadAsync<JsonObject>(response))["messages_count"]!.GetValue<int>() == 1, $"Expected one captured message for {id}.");
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response) where T : class
    {
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(AuditJson.Options)
            ?? throw new InvalidDataException($"Empty response from {response.RequestMessage?.RequestUri}.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
