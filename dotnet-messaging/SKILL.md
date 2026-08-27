---
name: dotnet-messaging
version: 1.0.0
description: "Use for reliable asynchronous messaging and RabbitMQ integration in C#/.NET microservices."
author: mag1art
license: Apache-2.0
tags: [dotnet, csharp, rabbitmq, messaging, microservices, retries, idempotency, outbox]
triggers:
  - RabbitMQ
  - message consumer
  - message publisher
  - queue
  - dead-letter
  - retry
  - idempotency
  - outbox
metadata:
  hermes:
    tags: [dotnet, csharp, rabbitmq, messaging, microservices, reliability]
---

# .NET Messaging

## When to Use

Use for RabbitMQ publishers, consumers, background workers, retries, dead-letter queues, delivery guarantees, and message-driven microservices.

## When Not to Use

Do not claim exactly-once delivery without proving complete system semantics. Do not acknowledge before durable processing succeeds.

## Delivery Model

Assume at-least-once delivery. Design handlers to be idempotent using a message ID, business key, inbox table, or equivalent deduplication mechanism.

## Consumer Rules

- Use explicit acknowledgements.
- Ack only after the business side effect and required persistence complete.
- Nack transient failures for controlled requeue; dead-letter poison messages.
- Use bounded prefetch and bounded local concurrency.
- Add cancellation and graceful shutdown.
- Propagate correlation and causation IDs in message headers.
- Make retry count and dead-letter routing observable.
- Version schemas and preserve backward compatibility.

## Reliability Patterns

Use an outbox when database and publication must be consistent. Use an inbox/idempotency record when duplicate delivery can repeat side effects. Make retries finite, delayed, and distinguish transient from permanent errors.

## Quality Gate

Test duplicate delivery, ordering assumptions, cancellation, broker reconnect, publish confirmation, nack/requeue, dead-lettering, schema evolution, and shutdown.
## Example: Ack After the Side Effect

```csharp
await foreach (var delivery in consumer.ReadAllAsync(ct))
{
    try
    {
        if (await inbox.IsProcessedAsync(delivery.MessageId, ct))
        {
            await consumer.AckAsync(delivery, ct);
            continue;
        }

        await handler.HandleAsync(delivery.Payload, ct);
        await inbox.MarkProcessedAsync(delivery.MessageId, ct);
        await consumer.AckAsync(delivery, ct);
    }
    catch (TransientMessageException) when (!ct.IsCancellationRequested)
    {
        await consumer.NackAsync(delivery, requeue: true, ct);
    }
    catch (Exception ex) when (!ct.IsCancellationRequested)
    {
        logger.LogError(ex, "Message processing failed {MessageId}", delivery.MessageId);
        await consumer.DeadLetterAsync(delivery, ct);
    }
}
```

The abstraction hides RabbitMQ client details, but the ordering is intentional: idempotency check, business side effect, processed marker, then acknowledgement. Make the inbox/outbox transaction strategy explicit for the selected database.

