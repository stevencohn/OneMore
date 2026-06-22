//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Windows.Forms;


	/// <summary>
	/// Extension of LinkLabel to force system Hand cursor instead of default Forms Hand cursor.
	/// </summary>
	internal class MoreLinkLabel : LinkLabel, ILoadControl
	{
		private const int BarWidth = 3;

		private readonly IntPtr hcursor;
		private Color back;
		private Color themedLinkColor;
		private bool selected;
		private bool active;
		private bool navMode;
		private LinkLabelLinkClickedEventHandler linkClicked;


		public MoreLinkLabel()
		{
			Cursor = Cursors.Hand;
			hcursor = Native.LoadCursor(IntPtr.Zero, Native.IDC_HAND);

			ActiveLinkColor = Color.MediumOrchid;
			LinkColor = Color.MediumOrchid;
			VisitedLinkColor = Color.MediumOrchid;
		}


		/// <summary>
		/// Properly dispose the unmanaged resources
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (linkClicked is not null)
				{
					base.LinkClicked -= linkClicked;
					linkClicked = null;
				}

				if (hcursor != IntPtr.Zero)
				{
					Native.DestroyCursor(hcursor);
				}
			}

			base.Dispose(disposing);
		}


		[Description("Determines the color of the hyperlink when mouse is over it")]
		public Color HoverColor
		{
			get;
			set;
		} = Color.Orchid;


		public bool Selected
		{
			get => selected;
			set
			{
				selected = value;
				if (selected)
				{
					back = BackColor;
					BackColor = ThemeManager.Instance.GetColor("LinkHighlight");
				}
				else
				{
					BackColor = back;
				}
			}
		}


		/// <summary>
		/// Gets or sets whether this control uses its secondary "nav-style" mode, in which
		/// the <see cref="Active"/> property controls the fore color and accent bar instead
		/// of the normal hyperlink colors. This is off by default, so a plain MoreLinkLabel
		/// always shows the normal themed hyperlink/active-link colors regardless of Active.
		/// Enable this for links that act as items in a vertical nav-style list.
		/// </summary>
		public bool NavMode
		{
			get => navMode;
			set
			{
				if (navMode != value)
				{
					navMode = value;
					ApplyRestingColor();
					Invalidate();
				}
			}
		}


		/// <summary>
		/// Gets or sets whether this link is the current "active" item in a vertical list
		/// of nav-style links, drawn with an accent bar along its left edge instead of the
		/// full-row highlight that <see cref="Selected"/> uses. Only takes effect while
		/// <see cref="NavMode"/> is enabled: the active item keeps the normal themed
		/// hyperlink color while inactive items render in plain ControlText, so only the
		/// active item reads as a link. Reserve left padding (e.g. Padding.Left = 8) on the
		/// control so the bar doesn't overlap the text. Has no effect while NavMode is false.
		/// </summary>
		public bool Active
		{
			get => active;
			set
			{
				if (active != value)
				{
					active = value;
					ApplyRestingColor();
					Invalidate();
				}
			}
		}


		public bool StrictColors { get; set; }


		public string ThemedBack { get; set; }


		public string ThemedFore { get; set; }


		/// <summary>
		/// Overrides the base LinkClicked event so we can keep a reference to the handler
		/// in order to unhook it later in our Dispose override. This assumes that we only
		/// bind to a single handler.
		/// </summary>
		public new event LinkLabelLinkClickedEventHandler LinkClicked
		{
			add
			{
				base.LinkClicked += value;
				linkClicked = value;
			}

			remove
			{
				base.LinkClicked -= value;
				if (linkClicked == value)
				{
					linkClicked = null;
				}
			}
		}


		void ILoadControl.OnLoad()
		{
			if (!StrictColors)
			{
				var manager = ThemeManager.Instance;

				themedLinkColor = !string.IsNullOrWhiteSpace(ThemedFore)
					? manager.GetColor(ThemedFore)
					: manager.GetColor("LinkColor");

				HoverColor = manager.GetColor("HoverColor");
				ApplyRestingColor();

				BackColor = back = !string.IsNullOrWhiteSpace(ThemedBack)
					? manager.GetColor(ThemedBack)
					: Parent.BackColor;
			}
		}


		/// <summary>
		/// Applies the not-hovered text color. While NavMode is enabled, this is the normal
		/// themed hyperlink color when this is the active item (see Active), otherwise plain
		/// ControlText. While NavMode is disabled, the normal themed hyperlink color always
		/// applies. Hovering always shows HoverColor regardless of mode.
		/// </summary>
		private void ApplyRestingColor()
		{
			var color = navMode && !active
				? ThemeManager.Instance.GetColor("ControlText")
				: themedLinkColor;

			LinkColor = VisitedLinkColor = ActiveLinkColor = color;
		}


		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			LinkColor = VisitedLinkColor = HoverColor;
		}


		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			ApplyRestingColor();
		}


		/// <summary>
		/// Suppresses the dotted keyboard-focus rectangle while this link is the active item
		/// in a nav-style list (NavMode enabled and Active true); the accent bar drawn in
		/// OnPaint already conveys that state.
		/// </summary>
		protected override bool ShowFocusCues => (navMode && active) ? false : base.ShowFocusCues;


		/// <summary>
		/// Claims Up/Down as input keys while NavMode is enabled so they're delivered to this
		/// control as ordinary KeyDown events instead of being swallowed by dialog-key
		/// navigation, letting a consumer move the active item in a vertical nav-style list.
		/// </summary>
		protected override bool IsInputKey(Keys keyData)
		{
			if (navMode && (keyData == Keys.Up || keyData == Keys.Down))
			{
				return true;
			}

			return base.IsInputKey(keyData);
		}


		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			if (navMode && active)
			{
				using var brush = new SolidBrush(ThemeManager.Instance.GetColor("Highlight"));
				e.Graphics.FillRectangle(brush, 0, 0, BarWidth, Height);
			}
		}


		protected override void WndProc(ref Message msg)
		{
			if (msg.Msg == Native.WM_SETCURSOR && hcursor != IntPtr.Zero)
			{
				Native.SetCursor(hcursor);
				msg.Result = IntPtr.Zero; // indicate handled
				return;
			}

			base.WndProc(ref msg);
		}
	}
}
