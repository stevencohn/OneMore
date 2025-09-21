//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Drawing;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;
	using System.Xml.Linq;

	internal partial class SearchDialogTextControl : MoreUserControl
	{
		private readonly ILogger logger;
		private readonly Regex cleaner;


		public SearchDialogTextControl()
		{
			InitializeComponent();
			logger = Logger.Current;

			// pattern to remove SPAN|A elements and &#nn; escaped characters
			cleaner = new Regex(
				@"(?:<\s*(?:span|a)[^>]*?>)|(?:</(?:span|a)>)|(?:&#\d+;)",
				RegexOptions.Compiled);
		}


		public event EventHandler<SearchCloseEventArgs> SearchClosing;


		private void Nevermind(object sender, EventArgs e)
		{
			SearchClosing?.Invoke(this, new(DialogResult.Cancel));
		}


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			resultsView.BackColor = manager.GetColor("ListView");

			var rowWidth = Width - SystemInformation.VerticalScrollBarWidth * 2;
			resultsView.Columns[0].Width = rowWidth;

			findBox.Focus();
		}


		private void ChangedText(object sender, EventArgs e)
		{
			var text = findBox.Text.Trim();
			searchButton.Enabled = text.Length > 0;
		}


		private void SearchOnKeydown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter &&
				findBox.Text.Trim().Length > 0)
			{
				Search(sender, e);
			}
		}

		private sealed class SearchHit
		{
			public string Hyperlink { get; set; }
			public string PageID { get; set; }
			public string ObjectID { get; set; }
		}

		private async void Search(object sender, EventArgs e)
		{
			await using var one = new OneNote(out var page, out var ns);

			var paragraphs = page.BodyOutlines
				.Descendants(ns + "OE")
				.Where(e => e.Elements(ns + "T").Any());

			if (paragraphs.Any())
			{
				resultsView.SuspendLayout();
				resultsView.Items.Clear();

				var builder = new TextMatchBuilder(false, false);
				var finder = builder.BuildRegex(findBox.Text);
				//logger.WriteLine(finder.ToString());

				foreach (var paragraph in paragraphs)
				{
					var text = GetRawText(paragraph, ns);
					if (text.Length > 0)
					{
						if (finder.IsMatch(text))
						{
							var paragraphID = paragraph.Attribute("objectID").Value;
							AddResult(one, page, paragraphID, text);
						}
					}
				}

				resultsView.ResumeLayout(true);
			}
		}


		private string GetRawText(XElement paragraph, XNamespace ns)
		{
			var text = string.Empty;
			paragraph.Elements(ns + "T").ForEach(e =>
			{
				var line = e.TextValue(true).Trim();
				if (line.Length > 0)
				{
					text = $"{text}{line} ";
				}
			});

			return cleaner.Replace(text.Trim(), string.Empty);
		}


		private void AddResult(OneNote one, Page page, string paragraphID, string text)
		{
			//logger.WriteLine($"hit [{text}]");

			var hit = new SearchHit
			{
				PageID = page.PageId,
				ObjectID = paragraphID,
				Hyperlink = one.GetHyperlink(page.PageId, paragraphID)
			};

			var link = new MoreLinkLabel
			{
				Text = text,
				Font = new("Segoe UI", 8.5f, FontStyle.Regular, GraphicsUnit.Point),
				Padding = new(0),
				Margin = new(0, 0, 0, 0),
				Width = resultsView.Width
			};

			link.Links.Add(new(0, 0, hit));
			link.LinkClicked += new LinkLabelLinkClickedEventHandler(async (s, e) =>
			{
				if (s is MoreLinkLabel label &&
					label.Links[0].LinkData is SearchHit hit)
				{
					await using var one = new OneNote();
					await one.NavigateTo(hit.Hyperlink);
				}
			});

			resultsView.AddHostedItem(link);
		}
	}
}
