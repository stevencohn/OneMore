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

			(float xScaleFactor, float yScaleFactor) = UIHelper.GetScalingFactors();

			ImageScalingSize = new Size(
				(int)(ImageScalingSize.Width * xScaleFactor),
				(int)(ImageScalingSize.Height * yScaleFactor));
		}
	}
}
