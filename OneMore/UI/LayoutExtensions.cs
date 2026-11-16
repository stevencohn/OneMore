//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Windows.Forms;


	internal static class LayoutExtensions
	{
		/// <summary>
		/// Keeps a control positioned to the right of one or more auto-sized controls, e.g.
		/// MoreCheckBoxes whose width changes with the UI language, so that it is never overlapped.
		/// </summary>
		/// <param name="follower">The control to reposition</param>
		/// <param name="gap">
		/// The desired gap, in 96-dpi pixels, between the rightmost anchor and the follower
		/// </param>
		/// <param name="alignWith">
		/// Optional sibling whose Left is the minimum for the follower, to keep it aligned with
		/// other controls in the same column when the anchors are narrow; otherwise null
		/// </param>
		/// <param name="anchors">
		/// The controls to stay clear of; these should share the follower's parent
		/// </param>
		public static void FollowRight(
			this Control follower, int gap, Control alignWith, params Control[] anchors)
		{
			void Reposition(object sender, EventArgs e)
			{
				var right = int.MinValue;
				foreach (var anchor in anchors)
				{
					right = Math.Max(right, anchor.Right);
				}

				var scale = Scaling.GetScalingFactors(follower).Item1;
				var left = right + (int)Math.Round(gap * scale);
				if (alignWith is not null)
				{
					left = Math.Max(left, alignWith.Left);
				}

				if (follower.Left != left)
				{
					follower.Left = left;
				}
			}

			foreach (var anchor in anchors)
			{
				anchor.SizeChanged += Reposition;
				anchor.LocationChanged += Reposition;
			}

			follower.HandleCreated += Reposition;
		}
	}
}
