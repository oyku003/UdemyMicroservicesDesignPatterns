using MassTransit;
using Microsoft.EntityFrameworkCore;
using Stock.API.Models;
using Microsoft.Extensions.DependencyInjection;
using Stock.API.Consumers;
using Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseInMemoryDatabase("StockDb");
});

builder.Services.AddMassTransit(x =>
{
    //x.AddConsumer<OrderCreatedEventConsumer>();
    //x.AddConsumer<PaymentFailedEventConsumer>();
    x.AddConsumer<StockRollBackMessageConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ"));
        //cfg.ReceiveEndpoint(RabbitMQSettingsConst.StockOrderCreatedEventQueueName, e =>
        //{
        //    e.ConfigureConsumer<OrderCreatedEventConsumer>(context);
        //});
        //cfg.ReceiveEndpoint(RabbitMQSettingsConst.StockPaymentFailedEventQueueName, e =>
        //{
        //    e.ConfigureConsumer<PaymentFailedEventConsumer>(context);
        //});

        cfg.ReceiveEndpoint(RabbitMQSettingsConst.StockRollBackMessageQueueName, e =>
        {
            e.ConfigureConsumer<StockRollBackMessageConsumer>(context);
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

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Stocks.Add(new Stock.API.Models.Stock { Id = 1, ProductId = 1, Count = 100 });
    context.Stocks.Add(new Stock.API.Models.Stock { Id = 2, ProductId = 2, Count = 100 });
    context.SaveChanges();
}//async yapma sebebimiz sürekli çalýþssýn demek için , burada sadece uygulama ayaga ilk kalktýgýnda bir kez çalýþacagý için async yapmadýk.
app.Run();
