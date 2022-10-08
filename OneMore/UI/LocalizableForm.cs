//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Threading.Tasks;
	using System.Windows.Forms;



	internal class LocalizableForm : Form, IOneMoreWindow
	{
		public event EventHandler ModelessClosed;

		protected ILogger logger = Logger.Current;
		private bool modeless = false;


		public LocalizableForm()
		{
			Properties.Resources.Culture = AddIn.Culture;
		}


		public int VerticalOffset
		{
			private get;
			set;
		} = 2;


		/// <summary>
		/// Determines if the main OneNote thread culture differs from our default design-time
		/// language, English. If true, then the Localize method should be called.
		/// </summary>
		/// <returns></returns>
		protected static bool NeedsLocalizing()
		{
			return TranslationHelper.NeedsLocalizing();
		}


		/// <summary>
		/// Traslate the text of specified controls on this form
		/// </summary>
		/// <param name="keys">
		/// A list of control identifiers as described by TranslationHelper
		/// </param>
		/// <seealso cref="River.OneMoreAddIn.UI.TranslationHelper"/>
		protected void Localize(string[] keys)
		{
			TranslationHelper.Localize(this, keys);
		}


		/// <summary>
		/// In order for a dialog to interact with OneNote, it must run modeless so it doesn't
		/// block the OneNote main UI thread. This method runs the current form as a modeless
		/// window and invokes the specified callbacks upon OK and Cancel.
		/// </summary>
		/// <param name="closedAction">
		/// An event handler to run when the modeless dialog is closed
		/// </param>
		/// <param name="topDelta">
		/// Optionally percentage of the dialog height to subtract from the top coordinate, 0-100
		/// </param>
		public async Task RunModeless(EventHandler closedAction = null, int topDelta = 0)
		{
			StartPosition = FormStartPosition.Manual;
			TopMost = true;
			modeless = true;

			var rect = new Native.Rectangle();
			using (var one = new OneNote())
			{
				Native.GetWindowRect(one.WindowHandle, ref rect);
			}

			var yoffset = (int)(Height * topDelta / 100.0);

			Location = new System.Drawing.Point(
				(rect.Left + ((rect.Right - rect.Left) / 2)) - (Width / 2),
				(rect.Top + ((rect.Bottom - rect.Top) / 2)) - (Height / 2) - yoffset
				);

			if (closedAction != null)
			{
				ModelessClosed += (sender, e) => { closedAction(sender, e); };
			}

			await Task.Factory.StartNew(() =>
			{
				Application.Run(this);
			});
		}


		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			base.OnFormClosed(e);
			ModelessClosed?.Invoke(this, e);
		}


		protected override void OnShown(EventArgs e)
		{
			// modeless has already set location so don't repeat that here
			// and only set location if inheritor hasn't declined by setting it to zero
			if (!DesignMode)
			{
				if (!modeless && VerticalOffset > 0)
				{
					var x = Location.X < 0 ? 0 : Location.X;
					var y = Location.Y - (Height / VerticalOffset);

					Location = new System.Drawing.Point(x, y < 0 ? 0 : y);
				}
			}

			// modeless dialogs would appear behind the OneNote window by default
			// so this forces the dialog to the foreground
			UIHelper.SetForegroundWindow(this);
		}
	}
}
