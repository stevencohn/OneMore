//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Markdig;


	internal static class OneMoreDigExtensions
	{
		public static MarkdownPipelineBuilder UseOneMoreExtensions(
			this MarkdownPipelineBuilder pipeline)
		{
			pipeline.Extensions.Add(new OneMoreDigExtension());
			return pipeline;
		}
	}
}
