
namespace OneMoreCalendar
{
	using System.Drawing;


	internal static class RectangleExtensions
	{
		public static Rectangle Shift(this Rectangle r, int x, int y)
		{
			r.Offset(x, y);
			return r;
		}
	}
}
