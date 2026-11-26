using System.Text.Json;
using Microsoft.OpenApi;
using SharpMail.Api.Models;
using SharpMail.Data;
using SharpMail.Data.Models;

namespace SharpMail.Api;

internal static class OpenApiExamples
{
    public static RouteHandlerBuilder WithRequestExample<T>(this RouteHandlerBuilder route, T example) =>
        route.AddOpenApiOperationTransformer((operation, _, _) =>
        {
            if (operation.RequestBody?.Content?["application/json"] is OpenApiMediaType media)
                media.Example = JsonSerializer.SerializeToNode(example, AuditJson.Options);
            return Task.CompletedTask;
        });

    public static SubmitRequest Receipt => new("order-42", new EmailMessage(
        new("store@demo.invalid"), [new("customer@demo.invalid")], [new("accounts@demo.invalid")],
        [], null, new("Receipt for order 42", "<p>Thank you for your order.</p>", "Thank you for your order.")));
}
