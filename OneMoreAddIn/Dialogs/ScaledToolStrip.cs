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

			SizeF dpiFactor;
			using (var g = CreateGraphics())
			{
				dpiFactor = new SizeF(g.DpiX / 96f, g.DpiY / 96f);
			}

			//Logger.Current.WriteLine($"factor:{factor} dpiX:{dpiX} dpiY:{dpiY}");
			//Logger.Current.WriteLine($"scaling width:{ImageScalingSize.Width} height:{ImageScalingSize.Height}");

			//if (factor.Width > 1f)
			//{
			ImageScalingSize = new Size(
					(int)(ImageScalingSize.Width * dpiFactor.Width),
					(int)(ImageScalingSize.Height * dpiFactor.Height));

			//Logger.Current.WriteLine($"Rescaled h:{ImageScalingSize.Height} w:{ImageScalingSize.Width}");
			//}

			foreach (var item in Items)
			{
				if (item is ToolStripControlHost)
				{
					var host = item as ToolStripControlHost;
					if (host.Placement == ToolStripItemPlacement.Overflow)
					{
						host.Control.Scale(dpiFactor);
					}
				}
				else if (item is ToolStripItem)
				{
					var host = item as ToolStripItem;
					host.ImageScaling = ToolStripItemImageScaling.None;
				}
			}
		}
	}
}
