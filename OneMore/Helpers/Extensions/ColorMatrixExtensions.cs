//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Drawing.Imaging;


	internal static class ColorMatrixExtensions
	{
		private const int MAX = 5;


		public static ColorMatrix Multiply(this ColorMatrix matrix, ColorMatrix other)
		{
			var m1 = GetMatrix(matrix);
			var m2 = GetMatrix(other);

			var result = new float[MAX][];
			for (var r = 0; r < MAX; r++)
				result[r] = new float[MAX];

			var column = new float[MAX];

			for (var c = 0; c < MAX; c++)
			{
				for (var i = 0; i < MAX; i++)
				{
					column[i] = m1[i][c];
				}

				for (var r = 0; r < MAX; r++)
				{
					var row = m2[r];
					var s = 0f;
					for (var k = 0; k < MAX; k++)
					{
						s += row[k] * column[k];
					}

					result[r][c] = s;
				}
			}

			return new ColorMatrix(result);
		}


		private static float[][] GetMatrix(ColorMatrix cm)
		{
			var m = new float[MAX][];
			for (var d = 0; d < MAX; d++)
			{
				m[d] = new float[MAX];
			}

			for (var r = 0; r < MAX; r++)
				for (var c = 0; c < MAX; c++)
					m[r][c] = cm[r, c];

			return m;
		}
	}
}
