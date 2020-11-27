//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Dialogs
{
	using System;
	using System.ComponentModel;
	using System.Linq;
	using System.Reflection;
	using System.Threading;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;



	internal class LocalizableForm : Form, IOneMoreWindow
	{

		public event EventHandler ModelessClosed;


		/// <summary>
		/// Determines if the main OneNote thread culture differs from our default design-time
		/// language, English. If true, then the Localize method should be called.
		/// </summary>
		/// <returns></returns>
		protected static bool NeedsLocalizing()
		{
			return AddIn.Culture.TwoLetterISOLanguageName != "en";
		}


		/// <summary>
		/// Set the Text property, or specified property, for each named control.
		/// </summary>
		/// <param name="keys">
		/// A string array; each item is the name of the control to set the Text property, or can
		/// specified the controlName.propertyName to set a named property rather than Text
		/// </param>
		protected void Localize(string[] keys)
		{
			foreach (var key in keys)
			{
				// named control and default Text property
				var k = key;
				var p = "Text";

				var dot = key.IndexOf('.');
				if (dot > 0)
				{
					// specific named property for this control
					k = key.Substring(0, dot);
					p = key.Substring(dot + 1);
				}

				// Name will be the dialog class name, k is the control name, p is the property
				var resid = $"{Name}_{k}.{p}";
				string text;

				try
				{
					text = Resx.ResourceManager.GetString(resid, AddIn.Culture);
					if (string.IsNullOrEmpty(text))
					{
						Logger.Current.WriteLine($"resource not found {resid}");
						return;
					}
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine($"error loading resource {resid}", exc);
					return;
				}

				var control = Controls.Find(k, true).FirstOrDefault();
				if (control != null)
				{
					var prop = control.GetType().GetProperty(p);
					if (prop != null)
						prop.SetValue(control, text, null);
					else
						Logger.Current.WriteLine($"cannot find control property {k}.{p}");
				}
				else
				{
					var bindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
					var component = GetType().GetField(k, bindings)?.GetValue(this);

					if (component is Component comp)
					{
						var prop = comp.GetType().GetProperty(p, bindings);
						if (prop != null)
							prop.SetValue(comp, text, null);
						else
							Logger.Current.WriteLine($"cannot find Component property {k}.{p}");
					}
					else if (component is TreeNode node)
					{
						var prop = node.GetType().GetProperty(p, bindings);
						if (prop != null)
							prop.SetValue(node, text, null);
						else
							Logger.Current.WriteLine($"cannot find TreeNode property {k}.{p}");
					}
					else
					{
						Logger.Current.WriteLine($"cannot translate {k}, name not found");
					}
				}
			}
		}


		/// <summary>
		/// In order for a dialog to interact with OneNote, it must run modeless so it doesn't
		/// blocks the OneNote main UI thread. This method runs the current form as a modeless
		/// window and invokes the specified callbacks upon OK and Cancel.
		/// </summary>
		/// <param name="closedAction">
		/// An event handler to run when the modeless dialog is closed
		/// </param>
		public void RunModeless(EventHandler closedAction = null, int topDelta = 0)
		{
			StartPosition = FormStartPosition.Manual;
			TopMost = true;

			var rect = new Native.Rectangle();
			using (var one = new OneNote())
			{
				Native.GetWindowRect(one.WindowHandle, ref rect);
			}

			Location = new System.Drawing.Point(
				(rect.Left + (rect.Right - rect.Left) / 2) - (Width / 2),
				(rect.Top + (rect.Bottom - rect.Top) / 2) - ((Height / 2) + (Height * (topDelta / 100)))
				);

			Shown += ForceForeground;

			if (closedAction != null)
			{
				ModelessClosed += (sender, e) => { closedAction(sender, e); };
			}

			var thread = new Thread(() =>
			{
				Application.Run(this);
			});

			thread.Start();
		}


		private void ForceForeground(object sender, EventArgs e)
		{
			// modeless dialogs would appear behind the OneNote window by default so this
			// forces the dialog to the foreground
			UIHelper.SetForegroundWindow(this);
		}


		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			base.OnFormClosed(e);
			ModelessClosed?.Invoke(this, e);
		}
	}
}
