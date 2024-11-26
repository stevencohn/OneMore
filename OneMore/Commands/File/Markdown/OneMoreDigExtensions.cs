//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Markdig;
	using Markdig.Extensions.Emoji;
	using System.Collections.Generic;
	using System.Linq;
	using River.OneMoreAddIn.Helpers.Office;
	using System.Web.UI.WebControls;

	internal static class OneMoreDigExtensions
	{

		public static MarkdownPipelineBuilder UseOneMoreExtensions(
			this MarkdownPipelineBuilder pipeline)
		{
			var emojiDic = EmojiMapping.GetDefaultEmojiShortcodeToUnicode();
			var emojiDicNew = new Dictionary<string, string>();
			foreach (var mappings in emojiDic)
			{
				var tagName = Models.Page.taglist.FirstOrDefault(x => x.name.Equals(mappings.Key)).name;
				if (tagName.IsNullOrEmpty())
				{
					emojiDicNew.Add(mappings.Key,mappings.Value);
				}
			}
			var DefaultEmojisAndSmileysMapping = new EmojiMapping(
								emojiDicNew, EmojiMapping.GetDefaultSmileyToEmojiShortcode());
			//			var emojiMapping = EmojiMapping.DefaultEmojisAndSmileysMapping;
			pipeline.Extensions.Add(new EmojiExtension(DefaultEmojisAndSmileysMapping));

			pipeline.Extensions.Add(new OneMoreDigExtension());
			return pipeline;
		}
	}
}
