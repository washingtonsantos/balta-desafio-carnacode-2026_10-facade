using Facade.Checkout;
using Facade.Dto;
using Facade.SubSistemas;
using Facade.SubSistemas.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// Registro dos serviços
builder.Services.AddScoped<IInventorySystem, InventorySystem>();
builder.Services.AddScoped<IPaymentGateway, PaymentGateway>();
builder.Services.AddScoped<IShippingService, ShippingService>();
builder.Services.AddScoped<ICouponSystem, CouponSystem>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddScoped<ICheckoutFacade, CheckoutFacade>();

var app = builder.Build();
    
Console.WriteLine("=== Sistema de E-commerce ===\n");

var _checkoutFacade = app.Services.GetRequiredService<ICheckoutFacade>();

_checkoutFacade
    .Process(new OrderDTO
    {
        ProductId = "PROD001",
        Quantity = 2,
        CustomerEmail = "cliente@email.com",
        CreditCard = "1234567890123456",
        Cvv = "123",
        ShippingAddress = "Rua Exemplo, 123",
        ZipCode = "12345-678",
        CouponCode = "PROMO10",
        ProductPrice = 100.00m
    });