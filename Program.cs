using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace WebhooksReceiver;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                // Required to listen on http to be able to use ngrok
                webBuilder.UseUrls("http://localhost:5000");
                webBuilder.UseStartup<Startup>();
            });
}
