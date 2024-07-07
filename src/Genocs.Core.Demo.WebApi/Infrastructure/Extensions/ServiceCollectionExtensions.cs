﻿using Genocs.Core.Builders;
using Genocs.Core.Demo.WebApi.Options;
using Genocs.ServiceBusAzure.Configurations;
using Genocs.ServiceBusAzure.Queues;
using Genocs.ServiceBusAzure.Queues.Interfaces;
using Genocs.ServiceBusAzure.Topics;
using Genocs.ServiceBusAzure.Topics.Interfaces;
using MassTransit;

namespace Genocs.Core.Demo.WebApi.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAzureServiceBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAzureServiceBusTopic(configuration);
        services.AddAzureServiceBusQueue(configuration);

        return services;
    }

    public static IServiceCollection AddAzureServiceBusTopic(this IServiceCollection services, IConfiguration configuration)
    {
        // Register IOptions<TopicSettings>
        services.Configure<AzureServiceBusTopicSettings>(configuration.GetSection(AzureServiceBusTopicSettings.Position));

        // HOW to Register TopicSettings instead of IOptions<TopicSettings>
        ////var topicSetting = new TopicOptions();
        ////configuration.GetSection(TopicSettings.Position).Bind(topicSetting);
        ////services.AddSingleton(topicSetting);

        services.AddSingleton<IAzureServiceBusQueue, AzureServiceBusQueue>();

        return services;
    }

    public static IServiceCollection AddAzureServiceBusQueue(this IServiceCollection services, IConfiguration configuration)
    {
        // Register IOptions<QueueSettings>
        services.Configure<AzureServiceBusQueueSettings>(configuration.GetSection(AzureServiceBusQueueSettings.Position));

        // HOW to Register QueueSettings instead of IOptions<QueueSettings>
        ////var queueSetting = new QueueSettings();
        ////configuration.GetSection(QueueSettings.Position).Bind(queueSetting);
        ////services.AddSingleton(queueSetting);

        services.AddSingleton<IAzureServiceBusTopic, AzureServiceBusTopic>();

        return services;
    }

    public static IServiceCollection AddCustomMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        RabbitMQOptions options = configuration.GetOptions<RabbitMQOptions>(RabbitMQOptions.Position);

        services.AddSingleton(options);

        services.AddMassTransit(x =>
        {
            //x.AddConsumersFromNamespaceContaining<MerchantStatusChangedEvent>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
                //cfg.UseHealthCheck(context);
                cfg.Host(options.HostName, options.VirtualHost,
                    h =>
                    {
                        h.Username(options.UserName);
                        h.Password(options.Password);
                    }
                );
            });
        });

        return services;
    }
}
