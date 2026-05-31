using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ESAM.GrowTracking.API.ConfigureOptions
{
    internal sealed class ApiBehaviorOptionsSetup : IConfigureOptions<ApiBehaviorOptions>
    {
        public void Configure(ApiBehaviorOptions options)
        {
            options.SuppressModelStateInvalidFilter = true;
        }
    }
}