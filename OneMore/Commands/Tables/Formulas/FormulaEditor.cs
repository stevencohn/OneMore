//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.Formulas
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Drawing;
	using System.Windows.Forms;


	/// <summary>
	/// A single-line formula editor that colors functions, cell references, numbers,
	/// countif criteria and punctuation using theme colors, and draws a red border when invalid.
	/// </summary>
	internal class FormulaEditor : MoreRichTextBox
	{
		private const int WM_SETREDRAW = 0x000B;
		private const int WM_KEYDOWN = 0x0100;

		private readonly ThemeManager manager;
		private bool coloring;
		private bool invalid;


		public FormulaEditor()
		{
			manager = ThemeManager.Instance;

			Multiline = false;
			WordWrap = false;
			DetectUrls = false;
			AcceptsTab = false;
			BorderStyle = BorderStyle.FixedSingle;
			Font = new Font("Consolas", 11F);
		}


		/// <summary>
		/// Gets or sets whether the formula is invalid; invalid shows a red foreground ring.
		/// </summary>
		public bool Invalid
		{
			get => invalid;
			set
			{
				if (invalid != value)
				{
					invalid = value;
					Parent?.Invalidate(true);
				}
			}
		}


		public void InsertAtCursor(string text)
		{
			SelectedText = text;
			Focus();
		}


		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			Colorize();
		}


		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			Colorize();
		}


		protected override bool ProcessCmdKey(ref Message m, Keys keyData)
		{
			// leave Enter to the form's AcceptButton
			if (m.Msg == WM_KEYDOWN && keyData == Keys.Enter)
			{
				return false;
			}

			return base.ProcessCmdKey(ref m, keyData);
		}


		private void Colorize()
		{
			if (coloring || !IsHandleCreated)
			{
				return;
			}

			coloring = true;
			var start = SelectionStart;
			var length = SelectionLength;

			Native.SendMessage(Handle, WM_SETREDRAW, 0, 0);
			try
			{
				var plain = manager.GetColor("ControlText");
				BackColor = manager.GetColor("Window");

				SelectAll();
				SelectionColor = plain;

				foreach (var token in FormulaTokenizer.Tokenize(Text))
				{
					if (token.Kind == FormulaTokenKind.Plain)
					{
						continue;
					}

					Select(token.Start, token.Length);
					SelectionColor = GetColor(token.Kind, plain);
				}
			}
			finally
			{
				Select(start, length);
				SelectionColor = manager.GetColor("ControlText");
				Native.SendMessage(Handle, WM_SETREDRAW, 1, 0);
				Invalidate();
				coloring = false;
			}
		}


		private Color GetColor(FormulaTokenKind kind, Color plain)
		{
			return kind switch
			{
				FormulaTokenKind.Function => manager.GetColor("HotTrack"),
				FormulaTokenKind.Cell => manager.GetColor("MenuHighlight"),
				FormulaTokenKind.Number => manager.GetColor("ActiveCaption"),
				FormulaTokenKind.Criteria => manager.GetColor("InfoText"),
				FormulaTokenKind.Punctuation => manager.GetColor("GrayText"),
				_ => plain
			};
		}


		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);
			if (Parent is not null)
			{
				Parent.Paint += PaintBorder;
			}
		}


		private void PaintBorder(object sender, PaintEventArgs e)
		{
			var color = invalid
				? manager.GetColor("ErrorText")
				: Focused ? manager.GetColor("HotTrack") : manager.GetColor("ActiveBorder");

			using var pen = new Pen(color, 1);
			var r = Bounds;
			r.Inflate(1, 1);
			e.Graphics.DrawRectangle(pen, r);
		}


		protected override void OnEnter(EventArgs e)
		{
			base.OnEnter(e);
			Parent?.Invalidate(true);
		}


		protected override void OnLeave(EventArgs e)
		{
			base.OnLeave(e);
			Parent?.Invalidate(true);
		}
	}
}
