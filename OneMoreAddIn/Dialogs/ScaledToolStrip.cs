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

		protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
		{
			base.ScaleControl(factor, specified);

			var scale = new SizeF(1, 1);
            using (var bitmap = new Bitmap(1, 1))
            using (var graphics = Graphics.FromImage(bitmap))
            {
				if (graphics.DpiX > 96)
				{
					scale.Width = graphics.DpiX / 96;
				}

				if (graphics.DpiY > 96)
                {
                    scale.Height = graphics.DpiY / 96;
                }
            }

            foreach (var item in Items)
			{
				if (item is ToolStripControlHost)
				{
					var host = item as ToolStripControlHost;
					if (host.Placement == ToolStripItemPlacement.Overflow)
					{
						host.Control.Scale(scale);
					}
				}
				else if (item is ToolStripItem)
				{
					var host = item as ToolStripItem;
                    //host.Image = HighDpiHelper.ScaleImage(host.Image, scale.Height);
					host.ImageScaling = ToolStripItemImageScaling.SizeToFit;
				}
			}
		}
	}
}
