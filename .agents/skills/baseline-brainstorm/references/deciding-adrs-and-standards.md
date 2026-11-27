# Deciding what to record

A conversation can settle a standard, an ADR, both, or neither. The engineer decides: they say "this is an ADR" or "make this a standard". You may suggest one when you notice it, in the chat, and you write it only after their yes.

The rule of thumb: a standard captures coding taste, and an ADR captures an architectural decision.

## Standard

Coding taste: a blanket rule about how code is written, with very limited exceptions. State the end result, and add short mnemonic snippets where they help.

Example: "C# payloads and contracts are records, not classes."

```csharp
public sealed record CreateOrder(Guid CustomerId, IReadOnlyList<OrderLine> Lines);   // yes
public sealed class CreateOrderDto { public Guid CustomerId { get; set; } }          // no
```

Suggest one when the engineer says "always", "never", "from now on" or "every ..." about how code is written.

## ADR

An architectural decision. It may carry domain reasoning for one pathway, and some coding preference. Test it against these six: a new technology or dependency, a data model or a public interface, a security or trust boundary, the deployment topology, a contract between two parts of the system, anything hard to reverse. State the decision as the desired output, then one or two lines of why. The alternatives and why-not go in the course, not here.

## Neither

A one-off choice that touches one place and is easy to change. Write no document for it.

## Telling them apart

| The engineer said | Record |
|---|---|
| "Every payload type is a record." | Standard |
| "We will use Postgres, not DynamoDB." | ADR |
| "In billing, a refund keeps the tax rate of the order." | ADR, domain reasoning for one pathway |
| "Call that variable `total`." | Neither |

One decision per file. Write it from the `TEMPLATE.md` in `baseline/adr/` or `baseline/standards/`.
