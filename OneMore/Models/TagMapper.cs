//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;


	/// <summary>
	/// Manages Tags and TagDefs on a page and merges TagDefs from one page to another.
	/// Originally part of the Page class.
	/// </summary>
	internal class TagMapper
	{
		#region class Mapping
		private sealed class Mapping
		{
			public TagDef TagDef { get; private set; }

			public List<int> SourceIndex { get; set; }

			public int TargetIndex { get; set; }

			public Mapping(XElement element)
				: this(new TagDef(element))
			{
			}

			public Mapping(TagDef tagdef)
			{
				TagDef = tagdef;
				SourceIndex = new List<int> { tagdef.IndexValue };
				TargetIndex = tagdef.IndexValue;
			}
		}
		#endregion

		private readonly Page page;
		private readonly XNamespace ns;
		private readonly List<Mapping> map;


		/// <summary>
		/// Initializes a new mapper for the given target page
		/// </summary>
		/// <param name="targetPage">The target page into which a merge will occur</param>
		public TagMapper(Page targetPage)
		{
			page = targetPage;
			ns = page.Root.GetNamespaceOfPrefix(OneNote.Prefix);
			map = BuildMap(page);
		}


		private List<Mapping> BuildMap(Page page)
		{
			return page.Root
				.Elements(ns + "TagDef")
				.Select(e => new Mapping(e)).ToList();
		}


		/// <summary>
		/// Merges the TagDefs from a source page with the TagDefs on the current page,
		/// adjusting index values to avoid collisions with pre-existing definitions
		/// </summary>
		/// <param name="sourcePage">
		/// The page from which to copy TagDefs into this page. The value of the index
		/// attribute of the TagDefs are updated for each definition
		/// </param>
		public void MergeTagDefsFrom(Page sourcePage)
		{
			// next available index for new tagdefs
			var index = map.Any() ? map.Max(t => t.TargetIndex) + 1 : 0;

			var sourcedefs = GetTagDefs(sourcePage);

			// resolve source tagdefs with target tagdefs
			foreach (var source in sourcedefs)
			{
				var mapping = map.Find(m => m.TagDef.Equals(source));
				if (mapping is null)
				{
					// no match so add it and set index to maxIndex+1
					// O(n) is OK here; there should only be a few
					var sourceIndex = source.IndexValue;
					source.IndexValue = index++;

					mapping = new Mapping(source)
					{
						SourceIndex = new List<int> { sourceIndex }
					};

					map.Add(mapping);
					page.AddTagDef(source);
				}
				else
				{
					// OneNote allows multiple TagDefs for the same symbol but having different
					// index values, so we need to accumulate them in a list so we can then
					// resolve each later in RemapTags...

					if (!mapping.SourceIndex.Contains(source.IndexValue))
					{
						mapping.SourceIndex.Add(source.IndexValue);
					}
				}
			}
		}


		/// <summary>
		/// Apply the current map to all Tags within the given content branch
		/// </summary>
		/// <param name="content"></param>
		public void RemapTags(XElement content)
		{
			// reverse sort the indexes so logic doesn't overwrite subsequent index references
			foreach (var mapping in map.OrderByDescending(s => s.TargetIndex))
			{
				var targetIndex = mapping.TargetIndex.ToString();

				// lookup Tag index value in map, for any Mapping that has this index value
				// in its list of index values...

				var tags = content.Descendants(ns + "Tag")
					.Where(e => e.Attribute("index") is XAttribute a &&
						int.Parse(a.Value) is int v &&
						mapping.SourceIndex.Contains(v));

				if (tags.Any())
				{
					// apply new index to child outline elements
					foreach (var tag in tags)
					{
						tag.Attribute("index").Value = targetIndex;
					}
				}
			}
		}


		/// <summary>
		/// Get a list of TagDefs on the given page.
		/// </summary>
		/// <param name="page"></param>
		/// <returns></returns>
		public static List<TagDef> GetTagDefs(Page page)
		{
			return page.Root
				.Elements(page.Namespace + "TagDef")
				.Select(e => new TagDef(e)).ToList();
		}
	}
}
