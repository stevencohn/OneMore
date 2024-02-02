//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using River.OneMoreAddIn.Styles;
	using System.Drawing;
	using System.IO;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal partial class ColorizerSheet : SheetBase
	{
		public const string HiddenKey = "hidden";


		public ColorizerSheet(SettingsProvider provider) : base(provider)
		{
			InitializeComponent();

			Name = nameof(ColorizerSheet);
			Title = Resx.ColorizerSheet_Title;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introBox",
					"applyBox",
					"fontLabel",
					"font2Label",
					"fixedBox",
					"enabledLabel",
					"allLink=word_All",
					"noneLink=word_None"
				});
			}

			familyBox.LoadFontFamilies();
			family2Box.LoadFontFamilies();

			if (AddIn.Culture.NumberFormat.NumberDecimalSeparator != ".")
			{
				for (int i = 0; i < sizeBox.Items.Count; i++)
				{
					var text = sizeBox.Items[i].ToString()
						.Replace(".", AddIn.Culture.NumberFormat.NumberDecimalSeparator);

					sizeBox.Items[i] = text;
					size2Box.Items[i] = text;
				}
			}

			var settings = provider.GetCollection(Name);

			applyBox.Checked = settings.Get("apply", false);

			var family = settings.Get("family", StyleBase.DefaultCodeFamily);
			familyBox.SelectedIndex = familyBox.Items.IndexOf(family);

			var size = settings.Get("size", StyleBase.DefaultCodeSize);
			sizeBox.SelectedIndex = sizeBox.Items.IndexOf(size.ToString());

			family = settings.Get("family2", StyleBase.DefaultCodeFamily);
			family2Box.SelectedIndex = family2Box.Items.IndexOf(family);

			size = settings.Get("size2", StyleBase.DefaultCodeSize);
			size2Box.SelectedIndex = size2Box.Items.IndexOf(size.ToString());

			LoadLanguages(settings.Get(HiddenKey, new XElement(HiddenKey)));
		}


		private void LoadLanguages(XElement hidden)
		{
			var languages = Colorizer.Colorizer.LoadLanguageNames();
			var images = new ImageList();
			foreach (var name in languages.Keys)
			{
				var tag = languages[name];
				images.Images.Add(LoadColorizeImage(tag));

				var item = new ListViewItem(name, images.Images.Count - 1)
				{
					Checked = hidden.Element(tag) == null,
					Tag = tag
				};

				langView.Items.Add(item);
			}

			langView.SmallImageList = images;
		}


		private Image LoadColorizeImage(string tag)
		{
			Image image = null;
			try
			{
				var path = Path.Combine(
					Colorizer.Colorizer.GetColorizerDirectory(),
					"Languages",
					$"{tag}.png");

				if (File.Exists(path))
				{
					image = Image.FromFile(path);
				}
			}
			catch
			{
				// no-op
			}

			return image;
		}


		private void FilterFontsOnCheckedChanged(object sender, System.EventArgs e)
		{
			familyBox.Items.Clear();
			familyBox.LoadFontFamilies(fixedBox.Checked);

			family2Box.Items.Clear();
			family2Box.LoadFontFamilies(fixedBox.Checked);
		}


		private void ToggleLanguages(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var enabled = sender == allLink;
			for (var i = 0; i < langView.Items.Count; i++)
			{
				langView.Items[i].Checked = enabled;
			}
		}


		public override bool CollectSettings()
		{
			var settings = provider.GetCollection(Name);
			var count = settings.Count;

			if (applyBox.Checked)
			{
				settings.Add("apply", true);
				settings.Add("family", familyBox.Text);
				settings.Add("size", sizeBox.Text);
				settings.Add("family2", family2Box.Text);
				settings.Add("size2", size2Box.Text);
			}
			else
			{
				settings.Remove("apply");
				settings.Remove("family");
				settings.Remove("size");
				settings.Remove("family2");
				settings.Remove("size2");
			}

			var updated = false;
			var oldset = settings.Get(HiddenKey, new XElement(HiddenKey));
			var newset = new XElement(HiddenKey);

			for (var i = 0; i < langView.Items.Count; i++)
			{
				if (!langView.Items[i].Checked)
				{
					var tag = langView.Items[i].Tag as string;
					var old = oldset.Element(tag);
					if (old == null)
					{
						updated = true;
					}
					else
					{
						old.Remove();
					}

					newset.Add(new XElement(tag));
				}
			}

			if (newset.Elements().Any())
			{
				settings.Add(HiddenKey, newset);
			}
			else
			{
				settings.Remove(HiddenKey);
			}

			updated |= oldset.Elements().Any();

			if (applyBox.Checked || newset.Elements().Any())
			{
				provider.SetCollection(settings);
			}
			else
			{
				provider.RemoveCollection(Name);
				updated = count > 0;
			}

			return updated;
		}
	}
}
