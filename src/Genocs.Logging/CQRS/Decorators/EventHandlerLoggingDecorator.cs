using Genocs.Common.Types;
using Genocs.Core.CQRS.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartFormat;

namespace Genocs.Logging.CQRS.Decorators;

[Decorator]
internal sealed class EventHandlerLoggingDecorator<TEvent>(IEventHandler<TEvent> handler, ILogger<EventHandlerLoggingDecorator<TEvent>> logger, IServiceProvider serviceProvider) : IEventHandler<TEvent>
    where TEvent : class, IEvent
{
    private readonly IEventHandler<TEvent> _handler = handler;
    private readonly ILogger<EventHandlerLoggingDecorator<TEvent>> _logger = logger;
    private readonly IMessageToLogTemplateMapper _mapper = serviceProvider.GetService<IMessageToLogTemplateMapper>() ?? new EmptyMessageToLogTemplateMapper();

    public async Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default)
    {
        var template = _mapper.Map(@event);

        if (template is null)
        {
            await _handler.HandleAsync(@event, cancellationToken);
            return;
        }

        try
        {
            Log(@event, template.Before);
            await _handler.HandleAsync(@event, cancellationToken);
            Log(@event, template.After);
        }
        catch (Exception ex)
        {
            string? exceptionTemplate = template.GetExceptionTemplate(ex);

            Log(@event, exceptionTemplate, isError: true);
            throw;
        }
    }

    private void Log(TEvent @event, string? message, bool isError = false)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        if (isError)
        {
            _logger.LogError(Smart.Format(message, @event));
        }
        else
        {
            _logger.LogInformation(Smart.Format(message, @event));
        }
    }

    /// <summary>
    /// Null Message to log template.
    /// </summary>
    private class EmptyMessageToLogTemplateMapper : IMessageToLogTemplateMapper
    {
        public HandlerLogTemplate? Map<TMessage>(TMessage message)
            where TMessage : class => null;
    }
}