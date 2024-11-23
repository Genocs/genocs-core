using Genocs.Core.Domain.Entities;

namespace Genocs.MessageBrokers.Outbox.Messages;

public sealed class InboxMessage : IEntity<string>
{
    public string Id { get; set; }
    public DateTime ProcessedAt { get; set; }

    public bool IsTransient()
    {
        throw new NotImplementedException();
    }
}