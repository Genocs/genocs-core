using Genocs.Common.Types;
using Genocs.Core.CQRS.Commands;
using Genocs.MessageBrokers;
using Genocs.MessageBrokers.Outbox;
using Genocs.MessageBrokers.Outbox.Configurations;

namespace Genocs.Identities.Application.Decorators;

[Decorator]
internal sealed class OutboxCommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
    where TCommand : class, ICommand
{
    private readonly ICommandHandler<TCommand> _handler;
    private readonly IMessageOutbox _outbox;
    private readonly string _messageId;
    private readonly bool _enabled;

    public OutboxCommandHandlerDecorator(
                                            ICommandHandler<TCommand> handler,
                                            IMessageOutbox outbox,
                                            OutboxOptions outboxOptions,
                                            IMessagePropertiesAccessor messagePropertiesAccessor)
    {
        _handler = handler;
        _outbox = outbox;
        _enabled = outboxOptions.Enabled;

        var messageProperties = messagePropertiesAccessor.MessageProperties;
        _messageId = string.IsNullOrWhiteSpace(messageProperties?.MessageId)
                                                                            ? Guid.NewGuid().ToString("N")
                                                                            : messageProperties.MessageId;
    }

    public Task HandleAsync(TCommand command, CancellationToken cancellationToken = default)
        => _enabled
            ? _outbox.HandleAsync(_messageId, () => _handler.HandleAsync(command))
            : _handler.HandleAsync(command);
}