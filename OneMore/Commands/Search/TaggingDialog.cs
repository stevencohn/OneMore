//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Web;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class TaggingDialog : UI.LocalizableForm
	{

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

		private const int PoolSize = 20;


		public TaggingDialog()
		{
			InitializeComponent();

			VerticalOffset = 5;
			tagBox.PressedEnter += AcceptInput;

			if (NeedsLocalizing())
			{
				Text = Resx.TaggingDialog_Text;

				Localize(new string[]
				{
					"tagLabel",
					"introLabel",
					"addButton=word_Add",
					"clearLabel",
					"suggestionsLabel=word_Suggestions",
					"recentGroup",
					"commonGroup",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}
		}


		private async void DialogLoad(object sender, EventArgs e)
		{
			await FetchRecentTags();
			FetchCommonWords();
			SuggestionsResize(sender, e);
		}


		private void SuggestionsResize(object sender, EventArgs e)
		{
			var width = suggestionsFlow.Width -
				suggestionsFlow.Padding.Left - suggestionsFlow.Padding.Right -
				suggestionPanel.Padding.Left - suggestionPanel.Padding.Right -
				suggestionPanel.Margin.Left - suggestionPanel.Margin.Right -
				SystemInformation.VerticalScrollBarWidth;

			recentGroup.Width = width;
			var size = new Size(width, CalculateFlowHeight(recentFlow));
			recentGroup.Size = size;
			recentGroup.MaximumSize = size;
			recentGroup.MinimumSize = size;

			commonGroup.Width = width;
			size = new Size(width, CalculateFlowHeight(commonFlow));
			commonGroup.Size = size;
			commonGroup.MaximumSize = size;
			commonGroup.MinimumSize = size;
		}


		private int CalculateFlowHeight(FlowLayoutPanel layout)
		{
			var width = 0;
			var height = 80;
			foreach (var control in layout.Controls)
			{
				var tag = (Control)control;
				var buffer = tag.Margin.Left + tag.Margin.Right + layout.Padding.Left + layout.Padding.Right;

				width += tag.Width + buffer;
				if (width > layout.Width)
				{
					height += tag.Height + tag.Margin.Top + tag.Margin.Bottom;
					width = 0;
				}
			}

			return height;
		}


		//private void RestrictSplitterMoving(object sender, SplitterCancelEventArgs e)
		//{
		//	if (splitter.SplitterDistance < 100)
		//	{
		//		splitter.SplitterDistance = 100;
		//	}
		//	else if (splitter.SplitterDistance > splitter.Height / 2)
		//	{
		//		splitter.SplitterDistance = splitter.Height / 2;
		//	}
		//}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		public List<string> Tags
		{
			get
			{
				return tagsFlow.Controls.OfType<TagControl>().ToList().ConvertAll(t => t.Label);
			}

			set
			{
				foreach (var tag in value)
				{
					AddTag(tag);
				}
			}
		}


		private async Task FetchRecentTags()
		{
			var tags = await TagHelpers.FetchRecentTags(OneNote.Scope.Notebooks, PoolSize);

			if (tags.Count > 0)
			{
				// keep order of most recent first
				foreach (var value in tags.Values)
				{
					recentFlow.Controls.Add(MakeLabel(value, value));
				}
			}
		}


		private void FetchCommonWords()
		{
			using (var one = new OneNote(out var page, out var ns, OneNote.PageDetail.Basic))
			{
				var builder = new StringBuilder();

				// collect all visible text into one StringBuilder

				var runs = page.Root.Elements(ns + "Outline")
					.Where(e => !e.Elements(ns + "Meta")
								.Any(m => m.Attribute("name").Value == MetaNames.TaggingBank))
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
							builder.Append(HttpUtility.HtmlDecode(text));
						}
					}
					else
					{
						var text = cdata.Value.Trim();
						if (text.Length > 0)
						{
							builder.Append(" ");
							builder.Append(HttpUtility.HtmlDecode(text));
						}
					}
				}

				// collect OCR text, e.g. <one:OCRText><![CDATA[...

				var data = page.Root.Elements(ns + "Outline").Descendants(ns + "OCRText")
					.Select(e => e.GetCData());

				foreach (var cdata in data)
				{
					builder.Append(" ");
					builder.Append(cdata.Value);
				}


				// split text into individual words, discarding all non-word chars and numbers

				var alltext = builder.Replace("\n", string.Empty).ToString();

				var words = Regex.Split(alltext, @"\W")
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
					.Take(PoolSize);

				foreach (var word in words)
				{
					commonFlow.Controls.Add(MakeLabel(word.Word, $"{word.Word} ({word.Count})"));
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
				// some languages use ';' as a list separator, most others use ',' (So use both!)
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
				var tag = new TagControl(text)
				{
					Name = text
				};

				tag.Deleting += (sender, e) =>
				{
					tagsFlow.Controls.Remove(sender as TagControl);
					clearLabel.Enabled = tagsFlow.Controls.Count > 0;
				};

				tagsFlow.Controls.Add(tag);

				clearLabel.Enabled = true;
			}
		}


		private void RemoveAllTags(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var result = MessageBox.Show(this,
				"Remove all tags from the current page?",
				"Confirm",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

			if (result == DialogResult.Yes)
			{
				tagsFlow.Controls.Clear();
				clearLabel.Enabled = false;
			}
		}
	}
}
