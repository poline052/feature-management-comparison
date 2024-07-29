using Flagsmith;
using StackExchange.Redis;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.FeatureManagement;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Load configuration from Azure App Configuration
builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect("Endpoint=https://own-app-config.azconfig.io;Id=9itW;Secret=9kwQMyKuAuTQbnQZ0eO4PbFNVpLWVJcmHyfBQE5K0cl86ctXTVKJJQQJ99AGACYeBjFF6AZlAAACAZACtO6U")
             .Select("*").ConfigureRefresh(refresh =>
             {
                 refresh.Register("Com")
                        .Register("Language", refreshAll: true)
                        .SetCacheExpiration(TimeSpan.FromHours(10));
             }).UseFeatureFlags(featureFlagOptions =>
    {
        featureFlagOptions.CacheExpirationInterval = TimeSpan.FromMinutes(5);
    });
});

builder.Services.AddAzureAppConfiguration()
                .AddFeatureManagement();

builder.Services.AddControllers();
builder.Services.AddSingleton<IFlagsmithClient>(sp => new FlagsmithClient("ser.iXPhDgWnuuZP8MBYfhVbUW"));
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("redis-19014.c282.east-us-mz.azure.redns.redis-cloud.com:19014,password=SspECRV2G3C1PdNEDkFvNnq1Rb7okO57"));
builder.Services.AddHttpClient();
var app = builder.Build();

// Configure the HTTP request pipeline.
// Use Azure App Configuration middleware for dynamic configuration refresh.
app.UseAzureAppConfiguration();
app.UseAuthorization();

app.MapControllers();

app.Run();
