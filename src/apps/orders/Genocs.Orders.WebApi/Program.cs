﻿using Genocs.Core.Builders;
using Genocs.Core.CQRS.Commands;
using Genocs.Core.CQRS.Events;
using Genocs.Core.CQRS.Queries;
using Genocs.Discovery.Consul;
using Genocs.HTTP;
using Genocs.LoadBalancing.Fabio;
using Genocs.Logging;
using Genocs.MessageBrokers.CQRS;
using Genocs.MessageBrokers.Outbox;
using Genocs.MessageBrokers.Outbox.MongoDB;
using Genocs.MessageBrokers.RabbitMQ;
using Genocs.Metrics.Prometheus;
using Genocs.Orders.WebApi;
using Genocs.Orders.WebApi.Commands;
using Genocs.Orders.WebApi.Domain;
using Genocs.Orders.WebApi.DTO;
using Genocs.Orders.WebApi.Events.External;
using Genocs.Orders.WebApi.Queries;
using Genocs.Persistence.MongoDb.Legacy;
using Genocs.Persistence.Redis;
using Genocs.Secrets.Vault;
using Genocs.Tracing.Jaeger;
using Genocs.Tracing.Jaeger.RabbitMQ;
using Genocs.WebApi;
using Genocs.WebApi.CQRS;
using Genocs.WebApi.Security;
using Genocs.WebApi.Swagger;
using Genocs.WebApi.Swagger.Docs;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("MassTransit", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);

builder.Host
        .UseLogging()
        .UseVault();

var services = builder.Services;

services.AddGenocs()
        .AddErrorHandler<ExceptionToResponseMapper>()
        .AddServices()
        .AddHttpClient()
        .AddCorrelationContextLogging()
        .AddConsul()
        .AddFabio()
        .AddJaeger()
        .AddMongo()
        .AddMongoRepository<Order, Guid>("orders")
        .AddCommandHandlers()
        .AddEventHandlers()
        .AddQueryHandlers()
        .AddInMemoryCommandDispatcher()
        .AddInMemoryEventDispatcher()
        .AddInMemoryQueryDispatcher()
        .AddPrometheus()
        .AddRedis()
        .AddRabbitMq(plugins: p => p.AddJaegerRabbitMqPlugin())
        .AddMessageOutbox(o => o.AddMongo())
        .AddWebApi()
        .AddSwaggerDocs()
        .AddWebApiSwaggerDocs()
        .Build();

var app = builder.Build();

app.UseGenocs()
    .UserCorrelationContextLogging()
    .UseErrorHandler()
    .UsePrometheus()
    .UseRouting()
    .UseCertificateAuthentication()
    .UseEndpoints(r => r.MapControllers())
    .UseDispatcherEndpoints(endpoints => endpoints
        .Get("", ctx => ctx.Response.WriteAsync("Orders Service"))
        .Get("ping", ctx => ctx.Response.WriteAsync("pong"))
        .Get<GetOrder, OrderDto>("orders/{orderId}")
        .Post<CreateOrder>("orders",
            afterDispatch: (cmd, ctx) => ctx.Response.Created($"orders/{cmd.OrderId}")))
    .UseJaeger()
    .UseSwaggerDocs()
    .UseRabbitMq()
    .SubscribeEvent<DeliveryStarted>();

app.Run();

Log.CloseAndFlush();

