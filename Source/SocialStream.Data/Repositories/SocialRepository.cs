using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using SocialStream.Data.Objects;

namespace SocialStream.Data.Repositories
{
	public class SocialRepository
	{
		public void StoreSocial(List<SocialItem> socialItems)
		{
			var sb = new StringBuilder();

			sb.Append("VALUES ");

			foreach (SocialItem socialItem in socialItems)
			{
				sb.AppendFormat(" ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', GETDATE(), GETDATE()),",
					socialItem.SocialNetwork,
					socialItem.Url,
					socialItem.Tweet,
					socialItem.TweetAuthor,
					socialItem.TweetScreenName,
					socialItem.Thumbnail,
					socialItem.Timestamp,
					socialItem.Hide);
			}

			string values = sb.ToString().Substring(0, sb.Length - 1);

			string query =
				string.Format(
					"INSERT INTO SocialData(SocialNetwork, Url, Tweet, TweetAuthor, TweetScreenName, Thumbnail, Timestamp, Hide, Created, Updated) {0}",
					values);

			Sql.ExecuteNonQuery(
				query,
				"socialstream",
				null);
		}

		public List<SocialItem> GetSocialItems()
		{
			return new List<SocialItem>(
				Sql.ExecuteReader(@"
						SELECT ID, SocialNetwork, Url, COALESCE(Tweet, '') AS Tweet, TweetAuthor, TweetScreenName, Thumbnail, Timestamp, Hide, Pick
						FROM SocialData
						ORDER BY Timestamp DESC",
					"socialstream",
					null,
					SocialLoader.Load));
		}

		public List<SocialItem> GetSocialItems(string socialNetwork)
		{
			return new List<SocialItem>(
				Sql.ExecuteReader(@"
					SELECT ID, SocialNetwork, Url, COALESCE(Tweet, '') AS Tweet, TweetAuthor, TweetScreenName, Thumbnail, Timestamp, Hide, Pick
					FROM SocialData
					WHERE SocialNetwork = @SocialNetwork
					ORDER BY Timestamp DESC",
					"socialstream",
					new[]
					{
						new SqlParameter("SocialNetwork", socialNetwork)
					},
					SocialLoader.Load));
		}

		public List<SocialItem> GetSocialItemsByNetworks(params string[] socialNetworks)
		{
			IEnumerable<string> formattedSocialNetworks = socialNetworks.Select(x => string.Format("'{0}'", x));
			string socialList = string.Join(", ", formattedSocialNetworks);

			string query = string.Format(@"
						SELECT ID, SocialNetwork, Url, COALESCE(Tweet, '') AS Tweet, TweetAuthor, TweetScreenName, Thumbnail, Timestamp, Hide, Pick
						FROM SocialData
						WHERE Hide = 0
						AND Pick = 0
						AND SocialNetwork IN ({0})
						ORDER BY Timestamp DESC", socialList);

			return new List<SocialItem>(
				Sql.ExecuteReader(
					query,
					"socialstream",
					null,
					SocialLoader.Load));
		}

		public List<SocialItem> GetSocialItemsByNetworks(DateTime startDateTime, params string[] socialNetworks)
		{
			IEnumerable<string> formattedSocialNetworks = socialNetworks.Select(x => string.Format("'{0}'", x));
			string socialList = string.Join(", ", formattedSocialNetworks);

			string query = string.Format(@"
						SELECT ID, SocialNetwork, Url, COALESCE(Tweet, '') AS Tweet, TweetAuthor, TweetScreenName, Thumbnail, Timestamp, Hide, Pick
						FROM SocialData
						WHERE Hide = 0
						AND Pick = 0
						AND SocialNetwork IN ({0})
						AND Timestamp <= ('{1}')
						ORDER BY Timestamp DESC", socialList, startDateTime);

			return new List<SocialItem>(
				Sql.ExecuteReader(
					query,
					"socialstream",
					null,
					SocialLoader.Load));
		}

		public void SetHiddenTrue(Guid id)
		{
			Sql.ExecuteNonQuery(@"
					UPDATE SocialData
					SET Hide = 1
					WHERE ID = @Id",
				"socialstream",
				new[]
				{
					new SqlParameter("Id", id)
				});
		}

		public void SetHiddenFalse(Guid id)
		{
			Sql.ExecuteNonQuery(@"
					UPDATE SocialData
					SET Hide = 0
					WHERE ID = @Id",
				"socialstream",
				new[]
				{
					new SqlParameter("Id", id)
				});
		}

		public void SetPickTrue(Guid id)
		{
			Sql.ExecuteNonQuery(@"
					UPDATE SocialData
					SET Pick = 0

					UPDATE SocialData
					SET Pick = 1
					WHERE ID = @Id",
				"socialstream",
				new[]
				{
					new SqlParameter("Id", id)
				});
		}

		public void SetPickFalse(Guid id)
		{
			Sql.ExecuteNonQuery(@"
					UPDATE SocialData
					SET Pick = 0
					WHERE ID = @Id",
				"socialstream",
				new[]
				{
					new SqlParameter("Id", id)
				});
		}

		public SocialItem GetPick()
		{
			return Sql.ExecuteReader(@"
					SELECT ID, SocialNetwork, Url, COALESCE(Tweet, '') AS Tweet, TweetAuthor, TweetScreenName, Thumbnail, Timestamp, Hide, Pick
					FROM SocialData
					WHERE Pick = 1",
				"socialstream",
				null,
				SocialLoader.Load).FirstOrDefault();
		}
	}
}