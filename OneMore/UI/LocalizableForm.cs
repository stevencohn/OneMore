//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.ComponentModel;
	using System.Linq;
	using System.Reflection;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;



	internal class LocalizableForm : Form, IOneMoreWindow
	{
		public event EventHandler ModelessClosed;

		protected ILogger logger = Logger.Current;
		private bool modeless = false;


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
			return AddIn.Culture.TwoLetterISOLanguageName != "en";
		}


		/// <summary>
		/// Set the Text property, or specified property, for each named control.
		/// </summary>
		/// <param name="keys">
		/// A string array, each item can be one of these formats:
		///   - "control"            sets the Text property of the named control
		///   - "control.prop"       sets the prop property of the named control
		///   - "control=resid"      sets the Text property of the named control to resid
		///   - "control.prop=resid" sets the prop property of the named control to resid
		/// resid override can be used to target common word_ phrases
		/// </param>
		protected void Localize(string[] keys)
		{
			foreach (var key in keys)
			{
				var controlName = key;
				var propName = "Text";

				string resid = null;
				var marker = controlName.IndexOf('=');
				if (marker > 0)
				{
					// override with explicit resid
					resid = controlName.Substring(marker + 1);
					controlName = controlName.Substring(0, marker);
				}

				marker = controlName.IndexOf('.');
				if (marker > 0)
				{
					// override property name
					propName = controlName.Substring(marker + 1);
					controlName = controlName.Substring(0, marker);
				}

				if (resid == null)
				{
					// default resid form if not explicitly overriden
					resid = $"{Name}_{controlName}.{propName}";
				}

				string text;
				try
				{
					text = Resx.ResourceManager.GetString(resid, AddIn.Culture);
					if (string.IsNullOrEmpty(text))
					{
						logger.WriteLine($"resource not found {resid}");
						continue;
					}
				}
				catch (Exception exc)
				{
					logger.WriteLine($"error loading resource {resid}", exc);
					continue;
				}

				var control = Controls.Find(controlName, true).FirstOrDefault();
				if (control != null)
				{
					if (control is ComboBox box)
					{
						box.Items.Clear();
						box.Items.AddRange(text.Split(
							new string[] { Environment.NewLine },
							StringSplitOptions.RemoveEmptyEntries));
					}
					else
					{
						var prop = control.GetType().GetProperty(propName);
						if (prop != null)
						{
							//logger.WriteLine($"resx {controlName}.{propName} = {resid} = {text}");
							prop.SetValue(control, text, null);
						}
						else
							logger.WriteLine($"cannot find control property {controlName}.{propName}");
					}
				}
				else
				{
					var bindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
					var component = GetType().GetField(controlName, bindings)?.GetValue(this);

					if (component is Component comp)
					{
						var prop = comp.GetType().GetProperty(propName, bindings);
						if (prop != null)
							prop.SetValue(comp, text, null);
						else
							logger.WriteLine($"cannot find Component property {controlName}.{propName}");
					}
					else if (component is TreeNode node)
					{
						var prop = node.GetType().GetProperty(propName, bindings);
						if (prop != null)
							prop.SetValue(node, text, null);
						else
							logger.WriteLine($"cannot find TreeNode property {controlName}.{propName}");
					}
					else
					{
						logger.WriteLine($"cannot translate {controlName}, name not found");
					}
				}
			}
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
