//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Drawing;


	internal class CustomStyle : IDisposable
	{
		public static readonly string DefaultFontFamily = "Calibri";
		public const float DefaultFontSize = 11;


		// Lifecycle

		public CustomStyle()
		{
			Name = "Normal";
			StyleType = StyleType.Paragraph;
			Font = new Font(DefaultFontFamily, DefaultFontSize);
			Color = Color.Black;
			Background = Color.Transparent;
			ApplyColors = true;
			SpaceAfter = 0;
			SpaceBefore = 0;
		}


		private bool disposedValue = false;
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					Font?.Dispose();
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}


		// Properties

		public string Name { get; set; }

		public StyleType StyleType { get; set; }

		public Font Font { get; set; }

		public Color Color { get; set; }

		public Color Background { get; set; }

		public bool ApplyColors { get; set; }

		public int SpaceBefore { get; set; }

		public int SpaceAfter { get; set; }


		/// <summary>
		/// Required because this model is data bound to the style dialog box Name dropdown
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return Name;
		}
	}
}
