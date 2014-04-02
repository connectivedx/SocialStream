using System;

namespace SocialStream.Data.Objects
{
	public class SocialItem
	{
		// From the DB (hence the guid)
		public SocialItem(Guid id, string socialNetwork, string url, string tweet, string tweetAuthor, string tweetScreenName,
			string thumbnail, DateTime timestamp, bool hide, bool pick)
		{
			Id = id;
			SocialNetwork = socialNetwork;
			Url = url;
			Tweet = tweet;
			TweetAuthor = tweetAuthor;
			TweetScreenName = tweetScreenName;
			Thumbnail = thumbnail;
			Timestamp = timestamp;
			Hide = hide;
			Pick = pick;
		}

		// Goes into the DB (hence the lack of a guid)
		public SocialItem(string socialNetwork, string url, string tweet, string tweetAuthor, string tweetScreenName,
			string thumbnail, DateTime timestamp)
		{
			SocialNetwork = socialNetwork;
			Url = url;
			Tweet = tweet;
			TweetAuthor = tweetAuthor;
			TweetScreenName = tweetScreenName;
			Thumbnail = thumbnail;
			Timestamp = timestamp;
		}

		public Guid Id { get; set; }
		public string SocialNetwork { get; set; }
		public string Url { get; set; }
		public string Tweet { get; set; }
		public string TweetAuthor { get; set; }
		public string TweetScreenName { get; set; }
		public string Thumbnail { get; set; }
		public DateTime Timestamp { get; set; }
		public bool Hide { get; set; }
		public bool Pick { get; set; }
	}
}