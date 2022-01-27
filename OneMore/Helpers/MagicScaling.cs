﻿//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Drawing;


	internal class MagicScaling
	{
		private readonly float scalingX;
		private readonly float scalingY;


		/// <summary>
		/// Initialize a new instance from the given resolutions which are typically
		/// from Image properties
		/// </summary>
		/// <param name="horizontalResolution">The Image.HorizontalResolution</param>
		/// <param name="verticalResolution">The Image.VerticalResolution</param>
		public MagicScaling(float horizontalResolution, float verticalResolution)
		{
			// set scaling factors
			var (dpiX, dpiY) = UIHelper.GetDpiValues();
			scalingX = dpiX / horizontalResolution;
			scalingY = dpiY / verticalResolution;

			//var (sx, sy) = UIHelper.GetScalingFactors();

			//Logger.Current.WriteLine(
			//	$"dpiX={dpiX} dpiY={dpiY} scalingX={scalingX} scalingY={scalingY} sx={sx} sy={sy}");
		}


		public float ScalingX => scalingX;


		public float ScalingY => scalingY;


		/// <summary>
		/// Gets the magic ratio, the larger of the horizontal or vertical ratio of the image
		/// </summary>
		/// <param name="image">The image to measure</param>
		/// <param name="boundingWidth">The bounding width in which to paint the image</param>
		/// <param name="boundingHeight">The bounding height in which to paint the image</param>
		/// <param name="margin">The margin to leave around the image</param>
		/// <returns></returns>
		public double GetRatio(Image image, int boundingWidth, int boundingHeight, int margin)
		{
			// return the larger ratio, horizontal or vertical of the image
			return Math.Max(
				// min of scaled image width or pictureBox width without margins
				image.Width / (Math.Min(Math.Round(image.Width * scalingX), boundingWidth - margin * 2)),
				// min of scaled image height or pictureBox height without margins
				image.Height / (Math.Min(Math.Round(image.Height * scalingY), boundingHeight - margin * 2))
				);
		}
	}
}
