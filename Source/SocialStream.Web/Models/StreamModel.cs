using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SocialStream.Data.Objects;

namespace SocialStream.Models
{
	public class StreamModel
	{
		public List<SocialItem> FacebookPictures { get; set; }
		public List<SocialItem> TwitterTweets { get; set; } 
	}
}