using ESAM.GrowTracking.API.Converters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ESAM.GrowTracking.API.ConfigureOptions
{
    internal sealed class JsonOptionsSetup : IConfigureOptions<JsonOptions>
    {
        public void Configure(JsonOptions options)
        {
            options.JsonSerializerOptions.Converters.Add(new JsonEnumConverterFactory());
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        }
    }
}