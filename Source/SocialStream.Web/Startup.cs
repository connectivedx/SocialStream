using Microsoft.Owin;
using Owin;
using SocialStream;

[assembly: OwinStartup(typeof (Startup))]

namespace SocialStream
{
	public partial class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			ConfigureAuth(app);
		}
	}
}