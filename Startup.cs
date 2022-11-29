using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SystemWspomaganiaNauczania.Startup))]
namespace SystemWspomaganiaNauczania
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
