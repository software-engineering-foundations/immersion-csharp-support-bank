using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using SupportBank;

var servicesProvider = new ServiceCollection()
    .AddTransient<Bank>()
    .AddLogging(loggingBuilder =>
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddNLog();
    })
    .BuildServiceProvider();

var bank = servicesProvider.GetRequiredService<Bank>();
bank.ServeCustomer();
