using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocialStream.Data.Objects;
using SocialStream.Data.Repositories;

namespace SocialStream.Data
{
	/// <summary>
	///     Pulls in a social stream and stores it into a database
	/// </summary>
	public class Agent
	{
		/// <summary>
		///     Collection of unique urls
		/// </summary>
		private Dictionary<string, SocialItem> _socialItems;

		/// <summary>
		///     Acquires and stores all of the feed data for each social network
		/// </summary>
		public void GetSocialStream()
		{
			if (_socialItems == null)
			{
				List<SocialItem> socialItems = new SocialRepository().GetSocialItems();

				_socialItems = new Dictionary<string, SocialItem>();

				if (socialItems.Any())
				{
					foreach (SocialItem item in socialItems)
					{
						_socialItems.Add(item.Url, item);
					}
				}
			}

			ConstructCall("facebook");
			ConstructCall("youtube");
			ConstructCall("twitter");
			ConstructCall("flickr");
			ConstructCall("instagram");
		}

		/// <summary>
		///     Calls the social network provided, grabs the feed and stores the data
		/// </summary>
		/// <param name="social">The social network being requested</param>
		private void ConstructCall(string social)
		{
			switch (social)
			{
				case "facebook":
					const string facebookUrl =
						"";
					string facebook = CallSocial(facebookUrl);
					StoreSocial(facebook, ParseAllFacebookAlbumPhotos);
					break;
				case "youtube":
					const string youtubeUrl = "";
					string youtube = CallSocial(youtubeUrl);
					StoreSocial(youtube, ParseYouTube);
					break;
				case "twitter":
					const string twitterUrl = "";
					string token = GetTwitterAuth();
					string twitter = CallSocial(twitterUrl, token);
					StoreSocial(twitter, ParseTwitter);
					break;
				case "flickr":
					const string flickrUrl = "";
					string flickr = CallSocial(flickrUrl);
					StoreSocial(flickr, ParseFlickr);
					break;
				case "instagram":
					const string instagramUrl = "";
					string instagram = CallSocial(instagramUrl);
					StoreSocial(instagram, ParseInstagram);
					break;
			}
		}

		/// <summary>
		///     Call's the social API requesting it's feed
		/// </summary>
		/// <param name="url">The URL the request is hitting</param>
		/// <returns>The json response as a string</returns>
		private string CallSocial(string url)
		{
			// Makes the call to the given url and returns the correct json object
			WebRequest req = WebRequest.Create(url);
			req.Method = "Get";

			// Execute Request
			WebResponse response = req.GetResponse();
			Stream stream = response.GetResponseStream();
			if (stream == null)
			{
				string nullResponse = string.Format("The response stream for {0} was null", url);
				throw new NullReferenceException(nullResponse);
			}

			var sr = new StreamReader(stream);
			string result = sr.ReadToEnd();

			return result;
		}

		/// <summary>
		///     Overload for API's requiring extra Authorization (Twitter)
		/// </summary>
		/// <param name="url">The URL the request is hitting</param>
		/// <param name="token">The token used in the Authorization header</param>
		/// <returns>The json response as a string</returns>
		private string CallSocial(string url, string token)
		{
			// Makes the call to the given url and returns the correct json object
			WebRequest req = WebRequest.Create(url);
			req.Method = "GET";
			req.Headers.Add("Authorization", "Bearer " + token);

			// Execute Request
			var response = (HttpWebResponse) req.GetResponse();
			Stream stream = response.GetResponseStream();
			if (stream == null)
			{
				string nullResponse = string.Format("The response stream for {0} was null", url);
				throw new NullReferenceException(nullResponse);
			}
			var sr = new StreamReader(stream);
			string result = sr.ReadToEnd();

			return result;
		}

		/// <summary>
		///     Gets the Authorization header for Twitter
		/// </summary>
		/// <returns>The Authorization token</returns>
		private string GetTwitterAuth()
		{
			// Setting Up OAuth Key
			string consumerKey = HttpUtility.UrlEncode("");
			string consumerSecret = HttpUtility.UrlEncode("");
			string authString = string.Format("{0}:{1}", consumerKey, consumerSecret);
			authString = Convert.ToBase64String(Encoding.UTF8.GetBytes(authString));

			// Creating OAuth Request
			const string twitterUri = "https://api.twitter.com/oauth2/token";
			WebRequest req = WebRequest.Create(twitterUri);
			req.Method = "POST";
			req.Headers.Add("Authorization", "Basic " + authString);
			req.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
			const string body = "grant_type=client_credentials";

			Stream reqStream = req.GetRequestStream();
			reqStream.Write(Encoding.UTF8.GetBytes(body), 0, body.Length);
			reqStream.Close();

			// Get the response and return the Access Token
			var response = (HttpWebResponse) req.GetResponse();
			Stream responseStream = response.GetResponseStream();
			if (responseStream == null) return null;
			var sr = new StreamReader(responseStream);
			JObject token = JObject.Parse(sr.ReadToEnd());

			var tokenStr = (string) token.SelectToken("access_token");

			return tokenStr;
		}

		/// <summary>
		///     Calls each specific loop method to return the social items parsed, then handles storage
		/// </summary>
		/// <param name="json">Json returned from the given API</param>
		/// <param name="social">Method to be called</param>
		private void StoreSocial(string json, Func<dynamic, List<SocialItem>> social)
		{
			dynamic socialJsonResult = JsonConvert.DeserializeObject(json);

			List<SocialItem> socialItems = social(socialJsonResult);

			if (!socialItems.Any()) return;

			var repo = new SocialRepository();

			repo.StoreSocial(socialItems);

			foreach (SocialItem item in socialItems)
			{
				_socialItems.Add(item.Url, item);
			}
		}

		/// <summary>
		///     Parses the Twitter feed using a dynamic object
		/// </summary>
		/// <param name="twitterTweets">The dynamic object to be parsed</param>
		/// <returns>A List of Social Items for each tweet</returns>
		private List<SocialItem> ParseTwitter(dynamic twitterTweets)
		{
			var twitterItems = new List<SocialItem>();

			foreach (dynamic item in twitterTweets)
			{
				// How to construct Twitter url
				// https://twitter.com/username/status/{id_str}
				var id = (string) item["id_str"];
				string url = string.Format("https://twitter.com/user/status/{0}", id);

				if (_socialItems.ContainsKey(url)) continue;

				var timeStr = (string) item["created_at"];
				DateTime timestamp = ParseTwitterDateTime(timeStr);

				var tweet = (string) item["text"];
				var tweetAuthor = (string) item["user"]["name"];
				var tweetScreenName = (string) item["user"]["screen_name"];

				// Escaping ' for SQL insertion
				tweet = tweet.Replace("'", "''");

				var twitterItem = new SocialItem("Twitter", url, tweet, tweetAuthor, tweetScreenName, "", timestamp);
				twitterItems.Add(twitterItem);
			}

			return twitterItems;
		}

		/// <summary>
		///     Parses the Twitter search result using a dynamic object
		/// </summary>
		/// <param name="twitterTweets">The dynamic object to be parsed</param>
		/// <returns>A List of Social Items for each tweet</returns>
		private List<SocialItem> ParseTwitterHashtag(dynamic twitterTweets)
		{
			var twitterItems = new List<SocialItem>();

			foreach (dynamic item in twitterTweets["statuses"])
			{
				// Excluding retweets
				if (item["retweeted_status"] != null) continue;
				// How to construct Twitter url
				// https://twitter.com/user/status/{id_str}
				var id = (string) item["id_str"];
				string url = string.Format("https://twitter.com/user/status/{0}", id);

				if (_socialItems.ContainsKey(url)) continue;

				var timeStr = (string) item["created_at"];
				DateTime timestamp = ParseTwitterDateTime(timeStr);

				var tweet = (string) item["text"];
				var tweetAuthor = (string) item["user"]["name"];
				var tweetScreenName = (string) item["user"]["screen_name"];

				// Escaping ' for SQL insertion
				tweet = tweet.Replace("'", "''");

				var twitterItem = new SocialItem("Twitter", url, tweet, tweetAuthor, tweetScreenName, "", timestamp);
				twitterItems.Add(twitterItem);
			}

			return twitterItems;
		}

		/// <summary>
		///     Twitter has a funky date time, this does an exact parse of it
		/// </summary>
		/// <param name="twitterDateTime">The date/time string to be parsed</param>
		/// <returns></returns>
		private DateTime ParseTwitterDateTime(string twitterDateTime)
		{
			const string format = "ddd MMM dd HH:mm:ss zzz yyyy";

			return DateTime.ParseExact(twitterDateTime, format, CultureInfo.InvariantCulture).ToLocalTime();
		}

		/// <summary>
		///     Parses the Flickr feed using a dynamic object
		/// </summary>
		/// <param name="flickrImages">The dynamic object to be parsed</param>
		/// <returns>A List of Social Items for each picture</returns>
		private List<SocialItem> ParseFlickr(dynamic flickrImages)
		{
			var flickrItems = new List<SocialItem>();

			foreach (dynamic item in flickrImages["photos"]["photo"])
			{
				var urlC = (string) item["url_c"];
				var urlO = (string) item["url_o"];

				// How to construct Flickr url
				// http://farm{farm-id}.staticflickr.com/{server-id}/{id}_{secret}.jpg
				var id = (string) item["id"];
				var secret = (string) item["secret"];
				var server = (string) item["server"];
				var farm = (int) item["farm"];
				string thumbnailUrl = string.Format("http://farm{0}.staticflickr.com/{1}/{2}_{3}.jpg", farm, server, id, secret);

				string url = urlC ?? urlO ?? thumbnailUrl;

				if (_socialItems.ContainsKey(url)) continue;

				var timeStr = (string) item["dateupload"];
				double timeSec = Convert.ToDouble(timeStr);
				DateTime timestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime().AddSeconds(timeSec);

				var flickrItem = new SocialItem("Flickr", url, "", "", "", thumbnailUrl, timestamp);
				flickrItems.Add(flickrItem);
			}

			return flickrItems;
		}

		/// <summary>
		///     Parses the YouTube feed using a dynamic object
		/// </summary>
		/// <param name="youtubeVideos">The dynamic object to be parsed</param>
		/// <returns>A List of Social Items for each video</returns>
		private List<SocialItem> ParseYouTube(dynamic youtubeVideos)
		{
			var youtubeItems = new List<SocialItem>();

			foreach (dynamic item in youtubeVideos["feed"]["entry"])
			{
				var idUrl = (string) item["id"]["$t"];
				string id = idUrl.TrimStart("http://gdata.youtube.com/feeds/api/videos/".ToCharArray());
				string url = string.Format("http://www.youtube.com/embed/{0}", id);

				if (_socialItems.ContainsKey(url)) continue;

				var thumbnail = (string) item["media$group"]["media$thumbnail"][0]["url"];

				var timeStr = (string) item["published"]["$t"];
				DateTime timestamp = DateTime.Parse(timeStr).ToLocalTime();

				var youtubeItem = new SocialItem("YouTube", url, "", "", "", thumbnail, timestamp);
				youtubeItems.Add(youtubeItem);
			}

			return youtubeItems;
		}

		/// <summary>
		///     Parses the Instagram feed using a dynamic object
		/// </summary>
		/// <param name="instagramPictures">The dynamic object to be parsed</param>
		/// <returns>A List of Social Items for each picture</returns>
		private List<SocialItem> ParseInstagram(dynamic instagramPictures)
		{
			var instagramItems = new List<SocialItem>();

			foreach (dynamic item in instagramPictures["data"])
			{
				var url = (string) item["images"]["standard_resolution"]["url"];

				if (_socialItems.ContainsKey(url)) continue;

				var timeStr = (string) item["created_time"];
				double timeSec = Convert.ToDouble(timeStr);
				DateTime timestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime().AddSeconds(timeSec);

				var instagramItem = new SocialItem("Instagram", url, "", "", "", url, timestamp);
				instagramItems.Add(instagramItem);
			}

			return instagramItems;
		}

		/// <summary>
		///     Parses all Facebook albums for a given account and stores all photos in each album.
		/// </summary>
		/// <param name="facebookAlbums">The dynamic object to be parsed</param>
		/// <returns>A List of Social Items for each picture</returns>
		private List<SocialItem> ParseAllFacebookAlbumPhotos(dynamic facebookAlbums)
		{
			var facebookItems = new List<SocialItem>();

			foreach (dynamic album in facebookAlbums["albums"]["data"])
			{
				foreach (dynamic item in album["photos"]["data"])
				{
					var url = (string) item["source"];

					if (_socialItems.ContainsKey(url)) continue;

					var timeStr = (string) item["created_time"];
					DateTime timestamp = DateTime.Parse(timeStr).ToLocalTime();

					var facebookItem = new SocialItem("Facebook", url, "", "", "", url, timestamp);
					facebookItems.Add(facebookItem);
				}
			}

			return facebookItems;
		}
	}
}