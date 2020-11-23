//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Dialogs;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;
	using System.Xml.Linq;

	internal partial class TaggingDialog : LocalizableForm
	{

		#region TagsTExtBox
		/// <summary>
		/// The default TextBox.KeyPressed/Up/Down handlers don't really prevent the accept
		/// button from being bubbled up to the dialog so this class provides an alternate method
		/// of intercepting the Enter key with its own event handler.
		/// </summary>
		private class TagsTextBox : TextBox
		{
			private const int WM_KEYDOWN = 256;

			public event EventHandler TagsChanged;

			protected override bool ProcessCmdKey(ref Message m, Keys k)
			{
				if (m.Msg == WM_KEYDOWN && k == Keys.Enter)
				{
					if (TagsChanged != null)
						TagsChanged(this, new EventArgs());

					// stop further interpretation
					return true;
				}
				// else default handlers...
				return base.ProcessCmdKey(ref m, k);
			}
		}
		#endregion TagsTExtBox


		private static readonly string[] Blacklist = new[]
		{
			#region Blacklist
			"about",
			"after",
			"again",
			"air",
			"all",
			"along",
			"also",
			"an",
			"and",
			"another",
			"any",
			"are",
			"around",
			"as",
			"at",
			"away",
			"back",
			"be",
			"because",
			"been",
			"before",
			"below",
			"between",
			"both",
			"but",
			"by",
			"came",
			"can",
			"come",
			"could",
			"day",
			"did",
			"different",
			"do",
			"does",
			"don't",
			"down",
			"each",
			"end",
			"even",
			"every",
			"few",
			"find",
			"first",
			"for",
			"found",
			"from",
			"get",
			"give",
			"go",
			"good",
			"great",
			"had",
			"has",
			"have",
			"he",
			"help",
			"her",
			"here",
			"him",
			"his",
			"home",
			"house",
			"how",
			"if",
			"in",
			"into",
			"is",
			"it",
			"its",
			"just",
			"know",
			"large",
			"last",
			"left",
			"like",
			"line",
			"little",
			"long",
			"look",
			"made",
			"make",
			"man",
			"many",
			"may",
			"me",
			"men",
			"might",
			"more",
			"most",
			"Mr.",
			"must",
			"my",
			"name",
			"never",
			"new",
			"next",
			"no",
			"not",
			"now",
			"number",
			"of",
			"off",
			"old",
			"on",
			"one",
			"only",
			"or",
			"other",
			"our",
			"out",
			"over",
			"own",
			"part",
			"people",
			"place",
			"put",
			"read",
			"right",
			"said",
			"same",
			"saw",
			"say",
			"see",
			"she",
			"should",
			"show",
			"small",
			"so",
			"some",
			"something",
			"sound",
			"still",
			"such",
			"take",
			"tell",
			"than",
			"that",
			"the",
			"them",
			"then",
			"there",
			"these",
			"they",
			"thing",
			"think",
			"this",
			"those",
			"thought",
			"three",
			"through",
			"time",
			"to",
			"together",
			"too",
			"two",
			"under",
			"up",
			"us",
			"use",
			"very",
			"want",
			"water",
			"way",
			"we",
			"well",
			"went",
			"were",
			"what",
			"when",
			"where",
			"which",
			"while",
			"who",
			"why",
			"will",
			"with",
			"word",
			"work",
			"world",
			"would",
			"write",
			"year",
			"you",
			"your",
			"was",
			"able",
			"above",
			"across",
			"add",
			"against",
			"ago",
			"almost",
			"among",
			"animal",
			"answer",
			"became",
			"become",
			"began",
			"behind",
			"being",
			"better",
			"black",
			"best",
			"body",
			"book",
			"boy",
			"brought",
			"call",
			"cannot",
			"car",
			"certain",
			"change",
			"children",
			"city",
			"close",
			"cold",
			"country",
			"course",
			"cut",
			"didn't",
			"dog",
			"done",
			"door",
			"draw",
			"during",
			"early",
			"earth",
			"eat",
			"enough",
			"ever",
			"example",
			"eye",
			"face",
			"family",
			"far",
			"father",
			"feel",
			"feet",
			"fire",
			"fish",
			"five",
			"food",
			"form",
			"four",
			"front",
			"gave",
			"given",
			"got",
			"green",
			"ground",
			"group",
			"grow",
			"half",
			"hand",
			"hard",
			"heard",
			"high",
			"himself",
			"however",
			"I'll",
			"I'm",
			"idea",
			"important",
			"inside",
			"John",
			"keep",
			"kind",
			"knew",
			"known",
			"land",
			"later",
			"learn",
			"let",
			"letter",
			"life",
			"light",
			"live",
			"living",
			"making",
			"mean",
			"means",
			"money",
			"morning",
			"mother",
			"move",
			"Mrs.",
			"near",
			"night",
			"nothing",
			"once",
			"open",
			"order",
			"page",
			"paper",
			"parts",
			"perhaps",
			"picture",
			"play",
			"point",
			"ready",
			"red",
			"remember",
			"rest",
			"room",
			"run",
			"school",
			"sea",
			"second",
			"seen",
			"sentence",
			"several",
			"short",
			"shown",
			"since",
			"six",
			"slide",
			"sometime",
			"soon",
			"space",
			"States",
			"story",
			"sun",
			"sure",
			"table",
			"though",
			"today",
			"told",
			"took",
			"top",
			"toward",
			"tree",
			"try",
			"turn",
			"United",
			"until",
			"upon",
			"using",
			"usually",
			"white",
			"whole",
			"wind",
			"without",
			"yes",
			"yet",
			"young"
			#endregion Blacklist
		};


		public TaggingDialog()
		{
			InitializeComponent();
			tagBox.TagsChanged += AcceptInput;
			FetchRecentTags();
			FetchPageWords();
		}


		protected override void OnShown(EventArgs e)
		{
			Location = new Point(Location.X, Location.Y - (Height / 5));
			UIHelper.SetForegroundWindow(this);
		}

		public List<string> Tags
		{
			get
			{
				return tagsFlow.Controls.OfType<TagLabel>().ToList().ConvertAll(t => t.Label);
			}

			set
			{
				foreach (var tag in value)
				{
					AddTag(tag);
				}
			}
		}


		private void FetchRecentTags()
		{
			using (var one = new OneNote())
			{
				var root = one.SearchMeta(string.Empty, Page.TaggingMetaName);
				var ns = root.GetNamespaceOfPrefix("one");
				var pages = root.Descendants(ns + "Page")
					.OrderByDescending(e => e.Attribute("lastModifiedTime").Value);

				var tags = new Dictionary<string, string>();

				var count = 0;
				foreach (var page in pages)
				{
					var meta = page.Element(ns + "Meta");
					if (meta != null)
					{
						var parts = meta.Attribute("content").Value
							.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

						foreach (var part in parts)
						{
							var p = part.Trim();
							var key = p.ToLower();
							if (!tags.ContainsKey(key))
							{
								tags.Add(key, p);

								count++;
								if (count > 19) break;
							}
						}
					}

					if (count > 19) break;
				}

				if (tags.Count > 0)
				{
					var sorted = tags.OrderBy(k => k.Key.StartsWith("#") ? k.Key.Substring(1) : k.Key);

					foreach (var s in sorted)
					{
						recentFlow.Controls.Add(MakeLabel(s.Value, s.Value));
					}
				}
			}
		}

		private void FetchPageWords()
		{
			using (var one = new OneNote(out var page, out var ns, OneNote.PageDetail.Basic))
			{
				var builder = new StringBuilder();

				var runs = page.Root.Elements(ns + "Outline")
					.Where(e => !e.Elements(ns + "Meta")
								.Any(m => m.Attribute("name").Value == Page.TagBankMetaName))
					.Descendants(ns + "T");

				foreach (var run in runs)
				{
					var cdata = run.GetCData();
					if (cdata.Value.Contains("<"))
					{
						var wrapper = cdata.GetWrapper();
						var text = wrapper.Value.Trim();
						if (text.Length > 0)
						{
							builder.Append(" ");
							builder.Append(text);
						}
					}
					else
					{
						var text = cdata.Value.Trim();
						if (text.Length > 0)
						{
							builder.Append(" ");
							builder.Append(text);
						}
					}
				}

				var words = Regex.Split(builder.ToString(), @"\W")
					.Select(w=> w.Trim().ToLower()).Where(w =>
						w.Length > 1 && 
						!Blacklist.Contains(w) && 
						!Regex.Match(w, @"^\s*\d+\s*$").Success)
					.GroupBy(w => w)
					.Select(g => new
					{
						Word = g.Key,
						Count = g.Count()
					})
					.OrderByDescending(g => g.Count)
					.Take(20);

				foreach (var word in words)
				{
					wordsFlow.Controls.Add(MakeLabel(word.Word, $"{word.Word} ({word.Count})"));
				}
			}
		}


		private Label MakeLabel(string value, string text)
		{
			var label = new Label
			{
				Name = value,
				Tag = value,
				AutoSize = true,
				Margin = new Padding(4),
				Padding = new Padding(4, 5, 4, 5),
				Text = text,
				BackColor = SystemColors.ControlLight,
				Cursor = Cursors.Hand
			};

			label.Click += (sender, e) =>
			{
				AddTag(((Label)sender).Tag as string);
			};

			label.MouseEnter += (sender, e) =>
			{
				((Label)sender).BackColor = Color.FromKnownColor(KnownColor.LightSkyBlue);
			};

			label.MouseLeave += (sender, e) =>
			{
				((Label)sender).BackColor = SystemColors.ControlLight;
			};

			return label;
		}


		private void AcceptInput(object sender, EventArgs e)
		{
			var text = tagBox.Text.Trim();
			if (text.Length > 0)
			{
				// some languages use ';' as a list separator, most others use ','
				var parts = text.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (var part in parts)
				{
					AddTag(part);
				}

				tagBox.Text = string.Empty;
			}
		}

		private void AddTag(string text)
		{
			if (!tagsFlow.Controls.ContainsKey(text))
			{
				var tag = new TagLabel(text)
				{
					Name = text
				};

				tag.Deleting += DeleteTag;
				tagsFlow.Controls.Add(tag);
			}
		}

		private void DeleteTag(object sender, EventArgs e)
		{
			var tag = sender as TagLabel;
			tagsFlow.Controls.Remove(tag);

		}
	}
}
