using MassTransit;
using Payment.API.Consumer;
using Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMassTransit(x =>
{
    //x.AddConsumer<StockReservedEventConsumer>();
    x.AddConsumer<StockReservedRequestPaymentConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ"));
        //cfg.ReceiveEndpoint(RabbitMQSettingsConst.StockReservedEventQueueName, e =>
        //{
        //    e.ConfigureConsumer<StockReservedEventConsumer>(context);
        //});

        cfg.ReceiveEndpoint(RabbitMQSettingsConst.PaymentStockReservedRequestQueueName, e =>
        {
            e.ConfigureConsumer<StockReservedRequestPaymentConsumer>(context);
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
