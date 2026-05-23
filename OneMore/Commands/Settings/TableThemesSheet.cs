//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using Microsoft.Office.Core;
	using River.OneMoreAddIn.Commands;
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Data;
	using System.Drawing;
	using System.IO;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Runtime.InteropServices.ComTypes;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class TableThemesSheet : SheetBase
	{
		private IRibbonUI ribbon;


		public TableThemesSheet(SettingsProvider provider, IRibbonUI ribbon)
			: base(provider)
		{
			InitializeComponent();

			Name = nameof(TableThemesSheet);
			Title = Resx.word_Plugins;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
				});
			}

			this.ribbon = ribbon;

			FillThumbnails();
		}


		private void FillThumbnails()
		{
			var provider = new TableThemeProvider();

			// we currently have 32 system tiles with 5x7=35 banded and 1x4 multi
			// so grab the first representative 0, 7, 14, 21, 28, and 35

			WCBox.Image = ImageFromStream(provider, 0);
			WCHBox.Image = ImageFromStream(provider, 7);
			CCBox.Image = ImageFromStream(provider, 14);
			CCHBox.Image = ImageFromStream(provider, 21);
			CCHHBox.Image = ImageFromStream(provider, 28);
			MBox.Image = ImageFromStream(provider, 35);
		}


		private Image ImageFromStream(TableThemeProvider provider, int index)
		{
			var image = new Bitmap(70, 60);

			// inset table so there's a margin
			var bounds = new Rectangle(7, 7, 55, 45);

			var painter = new TableThemePainter(image, bounds, BackColor);
			painter.Paint(provider.GetTheme(index));

			return image;
		}


		private void selectLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			SetChecks(true);
		}


		private void clearLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			SetChecks(false);
		}


		private void SetChecks(bool check)
		{
			showWCBox.Checked = check;
			showWCHBox.Checked = check;
			showCCBox.Checked = check;
			showCCHBox.Checked = check;
			showCCHHBox.Checked = check;
			showMBox.Checked = check;
		}


		public override bool CollectSettings()
		{
			var updated = false;

			var keepList = new List<string>();
			if (showCCBox.Checked) keepList.Add("WC");
			if (showCCBox.Checked) keepList.Add("WCH");
			if (showCCBox.Checked) keepList.Add("CC");
			if (showCCBox.Checked) keepList.Add("CCH");
			if (showCCBox.Checked) keepList.Add("CCHH");
			if (showCCBox.Checked) keepList.Add("M");

			var keepers = keepList.Aggregate((a, b) => $"{a},{b}");

			var settings = provider.GetCollection(Name);
			var categories = settings.Get("categories", string.Empty);

			if (keepers != categories)
			{
				if (keepList.Count == 0)
				{
					settings.Remove("categories");
				}
				else
				{
					settings.Add("categories", keepers);
				}

				updated = true;
			}

			provider.SetCollection(settings);

			if (updated)
			{
				ribbon.InvalidateControl("ribTableThemesButton");
			}

			// restart not required
			return false;
		}
	}
}
