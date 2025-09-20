//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;
	using System.Xml.Linq;

	internal partial class SearchDialogTextControl : MoreUserControl
	{
		private readonly ILogger logger;


		public SearchDialogTextControl()
		{
			InitializeComponent();
			logger = Logger.Current;
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


		private async void Search(object sender, EventArgs e)
		{
			await using var one = new OneNote(out var page, out var ns);

			var paragraphs = page.BodyOutlines
				.Descendants(ns + "OE")
				.Where(e => e.Elements(ns + "T").Any());

			if (paragraphs.Any())
			{
				// pattern to remove SPAN|A elements and &#nn; escaped characters
				var cleaner = new Regex(
					@"(?:<\s*(?:span|a)[^>]*?>)|(?:</(?:span|a)>)|(?:&#\d+;)",
					RegexOptions.Compiled);

				var builder = new TextMatchBuilder(false, false);
				var finder = builder.BuildRegex(findBox.Text);
				logger.WriteLine(finder.ToString());

				foreach (var paragraph in paragraphs)
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

					text = cleaner.Replace(text.Trim(), string.Empty);
					if (text.Length > 0)
					{
						if (finder.IsMatch(text))
						{
							logger.WriteLine($"hit [{text}]");
						}
					}
				}
			}
		}
	}
}
