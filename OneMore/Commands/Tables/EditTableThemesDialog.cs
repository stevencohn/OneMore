//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Drawing;


	internal partial class EditTableThemesDialog : UI.LocalizableForm
	{
		private const int margin = 15;

		private readonly Rectangle bounds;
		private readonly TableThemePainter painter;


		public EditTableThemesDialog()
		{
			InitializeComponent();

			previewBox.Image = new Bitmap(previewBox.Width, previewBox.Height);

			bounds = new Rectangle(
				margin, margin,
				previewBox.Width - (margin * 2), previewBox.Height - (margin * 2));

			var theme = new TableTheme
			{
				WholeTable = Color.Yellow,
				FirstColumnStripe = Color.Yellow,
				SecondColumnStripe = Color.Orange,
				//FirstRowStripe = Color.Aquamarine,
				//SecondRowStripe = Color.Salmon,
				FirstColumn = TableTheme.Rainbow,
				//LastColumn = Color.LightBlue,
				HeaderRow = Color.Red,
				TotalRow = Color.Green,
				HeaderFirstCell = Color.White,
				//HeaderLastCell = Color.Black,
				//TotalFirstCell = Color.Yellow,
				//TotalLastCell = Color.Purple
			};

			painter = new TableThemePainter(previewBox.Image, bounds);
			painter.Paint(theme);
		}
	}
}
