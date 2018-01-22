//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  Yada yada...
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Drawing;
	using System.Windows.Forms;


	/// <summary>
	/// Extension of ToolStrip to enable proper DPI scaling, primarily by
	/// applying the given factor to ImageScalingSize so button icons are
	/// drawn to correct size for the device.
	/// </summary>

	internal class ScaledToolStrip : ToolStrip
	{
		protected override void ScaleControl (SizeF factor, BoundsSpecified specified)
		{
			base.ScaleControl(factor, specified);

			float dpiX;
			float dpiY;

			using (var g = CreateGraphics())
			{
				dpiX = g.DpiX;
				dpiY = g.DpiY;
			}

			//Logger.Current.WriteLine($"factor:{factor} dpiX:{dpiX} dpiY:{dpiY}");

			if (factor.Width > 1f)
			{
				ImageScalingSize = new Size(
					(int)(ImageScalingSize.Width * factor.Width),
					(int)(ImageScalingSize.Height * factor.Height));

				//Logger.Current.WriteLine($"Rescaled h:{ImageScalingSize.Height} w:{ImageScalingSize.Width}");
			}

			foreach (var item in Items)
			{
				var host = item as ToolStripControlHost;
				if (host != null)
				{
					if (host.Placement == ToolStripItemPlacement.Overflow)
					{
						host.Control.Scale(factor);
					}
				}
			}
		}
	}
}
