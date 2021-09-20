//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Styles
{
	using System;
	using System.Globalization;
	using Drawing = System.Drawing;


	/// <summary>
	/// Used by style drop-down menu item in ribbon bar and Style dialog to visualize styles.
	/// </summary>
	internal class GraphicStyle : StyleBase, IDisposable
	{

		/// <summary>
		/// Copy given style to initialize a new instance.
		/// </summary>
		/// <param name="style">A Style from which property values are to be copied.</param>
		public GraphicStyle(Style style, bool scaling = true) : base(style)
		{
			float scaleFactor = 1f;
			if (scaling)
			{
				using (var bitmap = new Drawing.Bitmap(1, 1))
				using (var graphics = Drawing.Graphics.FromImage(bitmap))
				{
					scaleFactor = 96 / graphics.DpiY;
				}
			}

			if (string.IsNullOrEmpty(Name))
			{
				// TODO: is this needed here? should do in styledialog?
				Name = "Style-" + new Random().Next(1000, 9999).ToString();
			}

			var fontStyle = Drawing.FontStyle.Regular;
			if (style.IsBold) fontStyle |= Drawing.FontStyle.Bold;
			if (style.IsItalic) fontStyle |= Drawing.FontStyle.Italic;
			if (style.IsUnderline) fontStyle |= Drawing.FontStyle.Underline;
			if (style.IsStrikethrough) fontStyle |= Drawing.FontStyle.Strikeout;

			try
			{
				Font = new Drawing.Font(
					FontFamily, (float)fontSize * scaleFactor, fontStyle);
			}
			catch (Exception exc)
			{
				logger.WriteLine(
					$"error creating font({FontFamily}, {fontSize}, {fontStyle})", exc);

				Font = new Drawing.Font(
					DefaultFontFamily, (float)DefaultFontSize * scaleFactor, fontStyle);
			}

			try
			{
				Foreground = Color.Equals(Automatic) 
					? Drawing.Color.Black
					: Drawing.ColorTranslator.FromHtml(Color);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error translating color {Color}", exc);
				Foreground = Drawing.Color.Black;
			}

			try
			{
				Background = Highlight.Equals(Automatic)
					? Drawing.Color.Transparent
					: Drawing.ColorTranslator.FromHtml(Highlight);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error translating highlight {Highlight}", exc);
				Background = Drawing.Color.Transparent;
			}
		}


		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}


		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				Font?.Dispose();
				Font = null;
			}
		}


		/// <summary>
		/// Gets or sets the font used to display the text.
		/// </summary>
		public Drawing.Font Font { get; set; }


		/// <summary>
		/// Gets or sets the foreground (highlight) color of text
		/// </summary>
		public Drawing.Color Foreground { get; set; }


		/// <summary>
		/// Gets or sets the background (highlight) color of text
		/// </summary>
		public Drawing.Color Background { get; set; }


		/// <summary>
		/// Extracts a Style instance from this GraphicStyle.
		/// </summary>
		/// <returns></returns>
		public Style GetStyle()
		{
			return new Style(this)
			{
				FontFamily = Font.FontFamily.Name,
				FontSize = Font.Size.ToString("#0.0", CultureInfo.InvariantCulture),
				IsBold = Font.Bold,
				IsItalic = Font.Italic,
				IsUnderline = Font.Underline,
				IsStrikethrough = Font.Strikeout,
				Color = Drawing.ColorTranslator.ToHtml(Foreground),
				Highlight = Drawing.ColorTranslator.ToHtml(Background)
			};
		}


		/// <summary>
		/// Must override for StyleDialog name binding
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return Name; // $"{Name} ({FontFamily} {FontSize})";
		}
	}
}
