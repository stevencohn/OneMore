//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Drawing;
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


		/// <summary>
		/// Gets or sets whether the location has been set by the caller and should NOT be
		/// overriden by the OnLoad method below...
		/// </summary>
		public bool ManualLocation { get; set; } = false;


		/// <summary>
		/// Sets the absolute vertical offset in pixels from "centered" that you want to
		/// position this window upon load. This can be either a positive or negative value.
		/// </summary>
		public int VerticalOffset { private get; set; }


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

			using var one = new OneNote();
			Native.GetWindowRect(one.WindowHandle, ref rect);

			var yoffset = (int)(Height * topDelta / 100.0);

			Location = new Point(
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


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			// RunModeless has already set location so don't repeat that here and only set
			// location if inheritor hasn't declined by setting it to zero. Also, we're doing
			// this in OnLoad so it doesn't visually "jump" as it would if done in OnShown
			if (DesignMode || modeless)
			{
				return;
			}

			if (!ManualLocation && StartPosition == FormStartPosition.Manual)
			{
				// find the center point of the active OneNote window
				using var one = new OneNote();
				var bounds = one.GetCurrentMainWindowBounds();
				var center = new Point(
					bounds.Left + (bounds.Right - bounds.Left) / 2,
					bounds.Top + (bounds.Bottom - bounds.Top) / 2);

				Location = new Point(center.X - (Width / 2), center.Y - (Height / 2));
			}

			if (VerticalOffset != 0)
			{
				StartPosition = FormStartPosition.Manual;
				var x = Location.X < 0 ? 0 : Location.X;
				var y = Location.Y + VerticalOffset;

				Location = new Point(x, y < 0 ? 0 : y);
			}
		}

		protected override void OnShown(EventArgs e)
		{
			// modeless dialogs would appear behind the OneNote window by default
			// so this forces the dialog to the foreground
			UIHelper.SetForegroundWindow(this);
		}

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new(typeof(LocalizableForm));
			this.SuspendLayout();
			// 
			// LocalizableForm
			// 
			this.ClientSize = new Size(278, 244);
			this.Icon = ((Icon)(resources.GetObject("$this.Icon")));
			this.Name = "LocalizableForm";
			this.ResumeLayout(false);

		}
	}
}
