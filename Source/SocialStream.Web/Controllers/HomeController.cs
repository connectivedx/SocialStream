using System.Web.Mvc;
using SocialStream.Data;
using SocialStream.Data.Repositories;
using SocialStream.Models;

namespace SocialStream.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}

		public ActionResult APIs()
		{
			return View();
		}

		public ActionResult Postman()
		{
			return View();
		}

		public ActionResult Code()
		{
			return View();
		}

		public ActionResult Overview()
		{
			return View();
		}

		public ActionResult Stream()
		{
			//var agent = new Agent();

			//agent.GetSocialStream();

			var model = new StreamModel();

			var repository = new SocialRepository();

			var facebookPictures = repository.GetSocialItemsByNetworks("facebook");

			if (facebookPictures != null) model.FacebookPictures = facebookPictures;

			var twitterTweets = repository.GetSocialItemsByNetworks("twitter");

			if (twitterTweets != null) model.TwitterTweets = twitterTweets;

			return View(model);
		}
	}
}