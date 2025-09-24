﻿//************************************************************************************************
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
		private readonly IntPtr hcursor;
		private Color back;
		private Color fore;
		private bool selected;


		public MoreLinkLabel()
		{
			Cursor = Cursors.Hand;
			hcursor = Native.LoadCursor(IntPtr.Zero, Native.IDC_HAND);
			fore = Color.Empty;

			ActiveLinkColor = Color.MediumOrchid;
			LinkColor = Color.MediumOrchid;
			VisitedLinkColor = Color.MediumOrchid;
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


		public bool StrictColors { get; set; }


		public string ThemedBack { get; set; }


		public string ThemedFore { get; set; }


		void ILoadControl.OnLoad()
		{
			if (!StrictColors)
			{
				var manager = ThemeManager.Instance;

				var foreColor = !string.IsNullOrWhiteSpace(ThemedFore)
					? manager.GetColor(ThemedFore)
					: manager.GetColor("LinkColor");

				LinkColor = foreColor;
				ActiveLinkColor = foreColor;
				HoverColor = manager.GetColor("HoverColor");

				BackColor = back = !string.IsNullOrWhiteSpace(ThemedBack)
					? manager.GetColor(ThemedBack)
					: Parent.BackColor;
			}
		}


		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);

			if (fore == Color.Empty)
			{
				fore = LinkColor;
			}

			LinkColor = VisitedLinkColor = HoverColor;
		}


		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			LinkColor = VisitedLinkColor = fore;
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
