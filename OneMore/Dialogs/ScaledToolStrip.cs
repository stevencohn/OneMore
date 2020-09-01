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
			Logger.Current.WriteLine($"ScaleControl factor={factor.Width}x{factor.Height}");
			base.ScaleControl(factor, specified);

			(float xScaleFactor, float yScaleFactor) = UIHelper.GetScalingFactors();
			Logger.Current.WriteLine($"ScaleControl scaleFactor={xScaleFactor}x{yScaleFactor}");

			ImageScalingSize = new Size(
				(int)(ImageScalingSize.Width * xScaleFactor),
				(int)(ImageScalingSize.Height * yScaleFactor));

			//var scale = new SizeF(xScaleFactor, yScaleFactor);
			/*
			var scale = new SizeF(1, 1);
			using (var bitmap = new Bitmap(1, 1))
			using (var graphics = Graphics.FromImage(bitmap))
			{
				Logger.Current.WriteLine($"ScaleControl graphics={graphics.DpiX}x{graphics.DpiY}");
				if (graphics.DpiX > 96)
				{
					scale.Width = graphics.DpiX / 96;
				}

				if (graphics.DpiY > 96)
				{
					scale.Height = graphics.DpiY / 96;
				}
			}

			Logger.Current.WriteLine($"ScaleControl scale={scale.Width}x{scale.Height}");

			foreach (var item in Items)
			{
				if (item is ToolStripControlHost)
				{
					var host = item as ToolStripControlHost;
					if (host.Placement == ToolStripItemPlacement.Overflow)
					{
						host.Control.Scale(factor);
					}
				}
				else if (item is ToolStripItem)
				{
					var host = item as ToolStripItem;
					host.ImageScaling = ToolStripItemImageScaling.SizeToFit;
				}
			}
			*/
		}
	}
}
