using System.Data.Common;

namespace SocialStream.Data.Objects
{
	internal class SocialLoader
	{
		internal static SocialItem Load(DbDataReader reader)
		{
			return new SocialItem(
				reader.GetGuid(0),
				reader.GetString(1),
				reader.GetString(2),
				reader.GetString(3),
				reader.GetString(4),
				reader.GetString(5),
				reader.GetString(6),
				reader.GetDateTime(7),
				reader.GetBoolean(8),
				reader.GetBoolean(9));
		}
	}
}