using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebBTL.Startup))]
namespace WebBTL
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
