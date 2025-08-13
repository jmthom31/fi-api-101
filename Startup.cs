using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using WebhooksReceiver.Endpoints;

namespace WebhooksReceiver;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        services.AddSingleton<AuthProfile>(provider => new AuthProfile
        {
            //ApiUrl = @"https://institution-api-sim.clearbank.co.uk",
            ApiUrl = @"https://testnarwhal-institution-api-uksouth.test-cbinnovation.uk",
            ApiToken = @"",
            ClientPrivateKey = @"-----BEGIN PRIVATE KEY-----

-----END PRIVATE KEY-----",
            ClearBankPublicKey = @"-----BEGIN PUBLIC KEY-----

-----END PUBLIC KEY-----"
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        // Required to be able to read request body multiple times - https://devblogs.microsoft.com/aspnet/re-reading-asp-net-core-request-bodies-with-enablebuffering/
        app.Use(next => context =>
        {
            context.Request.EnableBuffering();
            return next(context);
        });

        app.UseEndpoints(endpoints =>
        {
            //endpoints.MapControllers();
            endpoints.MapWebhookTriggerEndpoints();
            endpoints.MapApiTestEndpoints();
            endpoints.MapWebhooksReceiverEndpoints();
        });
    }
}
