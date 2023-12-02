using Basket.API.GrpcServices;
using Basket.API.Repositories;
using Discount.Grpc.Protos;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetValue<string>("CacheSettings:ConnectionString");
});

builder.Services.AddScoped<IBasketRepository, BasketRepository>();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>
    (opt =>
        {
            var uri = builder.Configuration.GetValue<string>("GrpcSettings:DiscountUrl");
            opt.Address = new Uri(uri);
        }
    );
builder.Services.AddScoped<DiscountGrpcServices>();

builder.Services.AddMassTransit(config =>
{
    config.UsingRabbitMq((ctx, cfg) =>
    {
        var host = builder.Configuration.GetValue<string>("EventBusSettings:HostAddress");
        cfg.Host(host);
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
