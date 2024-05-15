using System;
using System.Diagnostics.Metrics;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<HatCoMetrics>();

var app = builder.Build();

app.MapPost("/complete-sale", async (HttpContext context, HatCoMetrics metrics) => {
    var request = await context.Request.ReadFromJsonAsync<SaleModel>();
    if (request == null)
    {
        context.Response.StatusCode = 400; // Bad Request
        return;
    }

    metrics.HatsSold(request.QuantitySold);
    context.Response.StatusCode = 200; // OK
});

var meter = new Meter("HatCo.Store");
var hatsSold = meter.CreateCounter<int>("hatco.store.hats_sold");
var task = Task.Run(() => {
    Console.WriteLine("Press any key to exit");
    while (!Console.KeyAvailable)
    {
        Thread.Sleep(1000);
        hatsSold.Add(4); 
    }
});

app.Run(); 
public class HatCoMetrics
{
    private readonly Counter<int> _hatsSold;

    public HatCoMetrics()
    {
        var meter = new Meter("HatCo.Store");
        _hatsSold = meter.CreateCounter<int>("hatco.store.hats_sold");
    }

    public void HatsSold(int quantity)
    {
        _hatsSold.Add(quantity);
    }
}


public class SaleModel
{
    public int QuantitySold { get; set; }
}
