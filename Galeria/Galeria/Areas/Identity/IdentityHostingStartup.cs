using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(Galeria.Areas.Identity.IdentityHostingStartup))]
namespace Galeria.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
            });
        }
    }
}