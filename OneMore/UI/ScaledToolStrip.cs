//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  Yada yada...
//************************************************************************************************

namespace River.OneMoreAddIn.UI
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

			var items = Items.GetEnumerator();
			while (items.MoveNext())
			{
				if (items.Current is ToolStripControlHost host)
				{
					if (host.Placement == ToolStripItemPlacement.Overflow)
						host.Control.Scale(factor);
				}
			}
		}



		/// <summary>
		/// Call this after InitializeComponent for the Form using the ScaledToolStrip
		/// </summary>
		public void Rescale()
		{
			(float scaleX, float scaleY) = UIHelper.GetScalingFactors();
			ImageScalingSize = new Size((int)(16 * scaleX), (int)(16 * scaleY));
			Height = (int)(24 * scaleY);
		}
	}
}
