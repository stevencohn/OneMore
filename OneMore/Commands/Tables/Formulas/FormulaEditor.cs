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
	/// A single-line formula editor that colors functions, cell references, variables, numbers,
	/// countif criteria and punctuation using Colorizer theme colors, and draws a red border when invalid.
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


		// Colorizer theme colors (OnetasticFunction, OnetasticVariable, Type/Teal for variables,
		// Number, String, Delimeter)
		private Color GetColor(FormulaTokenKind kind, Color plain)
		{
			var dark = manager.DarkMode;
			return kind switch
			{
				FormulaTokenKind.Function => ColorTranslator.FromHtml(dark ? "#569CD6" : "#2E75B5"),
				FormulaTokenKind.Cell => ColorTranslator.FromHtml(dark ? "#FF8A3D" : "#FF5800"),
				FormulaTokenKind.Variable => ColorTranslator.FromHtml(dark ? "#4EC9B0" : "#008080"),
				FormulaTokenKind.Number => ColorTranslator.FromHtml(dark ? "#B39DDB" : "#8064A2"),
				FormulaTokenKind.Criteria => ColorTranslator.FromHtml(dark ? "#CE9178" : "#B22222"),
				FormulaTokenKind.Punctuation => ColorTranslator.FromHtml("#808080"),
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
