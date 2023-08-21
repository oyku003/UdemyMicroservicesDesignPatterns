using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Models;
using Microsoft.Extensions.DependencyInjection;
using Order.API.Consumer;
using Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlCon"));

});

builder.Services.AddMassTransit(x =>
{
    //x.AddConsumer<PaymentCompletedEventConsumer>();
    //x.AddConsumer<PaymentFailedEventConsumer>();
    //x.AddConsumer<StockNotReservedEventConsumer>();
    x.AddConsumer<OrderRequestCompletedEventConsumer>();
    x.AddConsumer<OrderRequestFailedEventConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ"));
        //cfg.ReceiveEndpoint(RabbitMQSettingsConst.OrderPaymentCompletedEventQueueName, e =>
        //{
        //    e.ConfigureConsumer<PaymentCompletedEventConsumer>(context);
        //});
        //cfg.ReceiveEndpoint(RabbitMQSettingsConst.OrderPaymentFailedEventQueueName, e =>
        //{
        //    e.ConfigureConsumer<PaymentFailedEventConsumer>(context);
        //});
        //cfg.ReceiveEndpoint(RabbitMQSettingsConst.OrderStockNotReservedEventQueueName, e =>
        //{
        //    e.ConfigureConsumer<StockNotReservedEventConsumer>(context);
        //});
        cfg.ReceiveEndpoint(RabbitMQSettingsConst.OrderRequestCompletedQueueName, e =>
        {
            e.ConfigureConsumer<OrderRequestCompletedEventConsumer>(context);
        });
        
        cfg.ReceiveEndpoint(RabbitMQSettingsConst.OrderRequestfailedQueueName, e =>
        {
            e.ConfigureConsumer<OrderRequestFailedEventConsumer>(context);
        });
    });
});

builder.Services.AddMassTransitHostedService();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
