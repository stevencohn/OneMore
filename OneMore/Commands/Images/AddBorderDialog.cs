//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Drawing;
	using System.Globalization;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Asks for border color, line width and whether to add a drop shadow.
	/// </summary>
	internal partial class AddBorderDialog : UI.MoreForm
	{
		private const string CollectionName = "images";

		private Color borderColor;


		public AddBorderDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.AddBorderDialog_Text;

				Localize(new string[]
				{
					"colorLabel",
					"widthLabel",
					"shadowBox",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			borderColor = Color.Black;
			LoadSettings();
			colorBox.BackColor = borderColor;
		}


		/// <summary>
		/// Gets the border color.
		/// </summary>
		public Color BorderColor => borderColor;


		/// <summary>
		/// Gets the border width, in pixels as displayed on the page.
		/// </summary>
		public float BorderWidth => (float)widthBox.Value;


		/// <summary>
		/// Gets whether to add a drop shadow.
		/// </summary>
		public bool DropShadow => shadowBox.Checked;


		private void LoadSettings()
		{
			var collection = new SettingsProvider().GetCollection(CollectionName);

			var html = collection.Get("borderColor", string.Empty);
			if (!string.IsNullOrEmpty(html))
			{
				try
				{
					borderColor = ColorTranslator.FromHtml(html);
				}
				catch
				{
					// keep default
				}
			}

			// stored as an invariant string so decimals survive; older values were integers
			var width = 2m;
			var text = collection.Get("borderWidth", "2");
			if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
			{
				width = parsed;
			}

			widthBox.Value = Math.Max(widthBox.Minimum, Math.Min(widthBox.Maximum, width));

			shadowBox.Checked = collection.Get("borderShadow", false);
		}


		private void SaveSettings()
		{
			var provider = new SettingsProvider();
			var collection = provider.GetCollection(CollectionName);

			collection.Add("borderColor", ColorTranslator.ToHtml(borderColor));
			collection.Add("borderWidth", widthBox.Value.ToString(CultureInfo.InvariantCulture));
			collection.Add("borderShadow", DropShadow);

			provider.SetCollection(collection);
			provider.Save();
		}


		private void ChooseColor(object sender, EventArgs e)
		{
			var location = colorBox.PointToScreen(new Point(0, colorBox.Height));

			using var picker = new UI.MoreColorPicker(
				location.X, location.Y, Resx.AddBorderDialog_colorTitle)
			{
				Color = borderColor,
				AutomaticVisible = false
			};

			if (picker.ShowDialog(this) == DialogResult.OK)
			{
				borderColor = picker.Color;
				colorBox.BackColor = borderColor;
				colorBox.Refresh();
			}
		}


		private void OK(object sender, EventArgs e)
		{
			SaveSettings();
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
