//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable IDE0060 // Remove unused parameter

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Drawing;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Implements an auto-complete drop-down control that can be attached to a TextBox.
	/// 
	/// Can run in either command mode or free mode. In command mode, the available commands
	/// take priority over the text box; entering a partial command and pressing Enter will
	/// presume the first matched command. In free mode, the text box takes priority so pressing
	/// Enter will accept the raw text; the available commands are used to quickly edit the text.
	/// </summary>
	/// <remarks>
	/// The ProvideProperty tells VS to show a AutoCompleteList property for the visual designer
	/// but we don't really use it here because the ListView is shown in the designer making it
	/// harder to actually layout the UI. So consumers do this programmatically in OneMore.
	/// </remarks>
	[ProvideProperty("AutoCompleteList", typeof(Control))]
	internal class MoreAutoCompleteList : ListView, IExtenderProvider
	{
		// The ListView itself changes its own items dynamically based on input from Owner
		// The 'commands' field maintains the original list of available Cmds
		// The 'matches' field maintains the current matched Cmds based on Owner input

		private ToolStripDropDown popup;        // invisible host control of this ListtView	
		private readonly Font highFont;         // font of matched substring
		private readonly List<Cmd> commands;    // original list of commands
		private readonly List<Cmd> matches;     // dynamic list of matched commands
		private readonly ThemeManager manager;	// color manager
		private string boxtext;                 // the current/previous text in the Owner TextBox

		// each command name is described by a Cmd entry
		private sealed class Cmd
		{
			// incoming descriptor is of the form [category:]name[|keys]

			public string Category;				// category part
			public string Name;                 // name part
			public string Keys;					// key sequence part
			public bool Recent;                 // true if in "recently used" category
		}


		/// <summary>
		/// Initialize a new instance. The instance should be bound to a TextBox using
		/// the SetAutoCompleteList method.
		/// </summary>
		public MoreAutoCompleteList()
		{
			OwnerDraw = true;
			manager = ThemeManager.Instance;

			BackColor = manager.GetColor("ListView");
			ForeColor = manager.GetColor("ControlText");

			// detail view with default headless column so all drawing is done by DrawSubItem
			View = View.Details;
			Columns.Add("cmd");
			HeaderStyle = ColumnHeaderStyle.None;

			FullRowSelect = true;
			MultiSelect = false;
			MinimumSize = new Size(300, 300);
			SetStyle(ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true);

			Font = new Font("Segoe UI", 9);
			highFont = new Font(Font, Font.Style | FontStyle.Bold);
			commands = new List<Cmd>();
			matches = new List<Cmd>();

			RecentKicker = Resx.AutoComplete_recentlyUsed;
			OtherKicker = Resx.AutoComplete_otherCommands;
		}


		/// <summary>
		/// Gets or sets whether the text field takes priority, allowing free text beyond the
		/// list of known strings in the drop-down. If false, then only strings in the drop-down
		/// are accepted.
		/// </summary>
		public bool FreeText { get; set; }


		/// <summary>
		/// Gets or sets a character used to delimit a command's category string from its name.
		/// </summary>
		/// <remarks>
		/// command.Name format is [category:]name[|keys]
		/// </remarks>
		public char CategoryDivider { get; set; } = ':';


		/// <summary>
		/// Gets or sets a character used to delimit a command's name from its key sequence.
		/// </summary>
		/// <remarks>
		/// command.Name format is [category:]name[|keys]
		/// </remarks>
		public char KeyDivider { get; set; } = '|';


		/// <summary>
		/// Gets a value indicating whether there are currently any matched items.
		/// </summary>
		public bool HasMatches =>
			string.IsNullOrWhiteSpace(Owner.Text.Trim()) ||
			commands.Count == 0 ||
			commands.Exists(c => c.Name.ContainsICIC(Owner.Text.Trim()));


		/// <summary>
		/// Gets a value indicating whether the hosted popup is visible
		/// </summary>
		public bool IsPopupVisible => popup?.Visible == true;


		/// <summary>
		/// Sets the subtitle shown for all non-recent commands.
		/// The default is similar to "other commands"
		/// </summary>
		public string OtherKicker { private get; set; }


		/// <summary>
		/// Gets the TextBox control to which this auto-complete list is bound.
		/// </summary>
		public TextBox Owner { get; private set; }


		/// <summary>
		/// Sets the subtitle shown for the "recently used" commands
		/// The default is similar to "recently used"
		/// </summary>
		public string RecentKicker { private get; set; }


		/// <summary>
		/// Gets or sets whether the list is shown immediately along with its TextBox.
		/// The default is to only show the list on the first keypress.
		/// </summary>
		public bool ShowPopupOnStartup { get; set; }


		/// <summary>
		/// Extra chars to include as Word characters when scanning for words in text
		/// </summary>
		public char[] WordChars { private get; set; }


		#region IExtenderProvider implementation
		bool IExtenderProvider.CanExtend(object extendee)
		{
			return (extendee is TextBox);
		}


		/// <summary>
		/// Required implementation for ProvideProperty.
		/// </summary>
		public Control GetAutoCompleteList(Control control)
		{
			return Owner;
		}
		#endregion IExtenderProvider implementation


		#region public SetAutoCompleteList and LoadCommands
		/// <summary>
		/// Binds this control to a specified TextBox.
		/// This is required implementation for ProvideProperty.
		/// </summary>
		/// <param name="control">The TextBox to bind with the list</param>
		/// <remarks>
		/// This call can be auto-generated by the VS UI designer by setting the
		/// AutoCompleteList property of a TextBox.
		/// </remarks>
		public void SetAutoCompleteList(Control control)
		{
			// currently, only allow TextBox as the owner control
			if (control is TextBox box)
			{
				Owner = box;
				Width = Math.Max(box.Width, 300);
				box.KeyDown += DoKeydown;
				box.PreviewKeyDown += DoPreviewKeyDown;
				box.TextChanged += DoTextChanged;

				// For a free text control like the hashtag search dialog, we want to let the
				// popup disappear when focus is elsewhere. But for the Command Palette, we
				// want to keep the popup open unless the Esc key is pressed
				//if (FreeText)
				//{
				//	box.LostFocus += HidePopup;
				//}

				boxtext = box.Text.Trim();

				if (ShowPopupOnStartup)
				{
					// delays the popup until it can be seen
					// TODO: do we want to disconnect this handler once initialized?
					box.GotFocus += ShowPopup;
				}
			}
			else
			{
				// this should be covered by CanExtend but just in case...
				throw new ArgumentException("SetAutoCompleteList(control) must be a TextBox");
			}
		}


		/// <summary>
		/// Populate the typeahead buffer with a list of names.
		/// </summary>
		/// <param name="names">List of command names</param>
		/// <param name="recentNames">Optional list of recently used command names</param>
		public void LoadCommands(IEnumerable<string> names, IEnumerable<string> recentNames = null)
		{
			Items.Clear();
			commands.Clear();
			matches.Clear();

			// descriptors are of the form [category:]name[|keyseq]
			var pattern = new Regex(
				@$"(?:(?<cat>[^{CategoryDivider}]+){CategoryDivider})?" +
				@$"(?:(?<nam>[^\{KeyDivider}]+))" +
				$@"(?:\{KeyDivider}(?<seq>.*))?",
				RegexOptions.IgnoreCase | RegexOptions.Compiled);

			foreach (var name in names)
			{
				var match = pattern.Match(name);
				if (match.Success)
				{
					var groups = match.Groups;

					var cmd = new Cmd
					{
						Category = groups["cat"].Success ? groups["cat"].Value : null,
						Name = groups["nam"].Value,
						Keys = groups["seq"].Success ? groups["seq"].Value : null
					};

					Items.Add(name);
					commands.Add(cmd);
				}
			}

			if (recentNames?.Any() == true)
			{
				// inject recent names at top of list
				foreach (var name in recentNames.Reverse())
				{
					var match = pattern.Match(name);
					if (match.Success)
					{
						var groups = match.Groups;

						var cmd = new Cmd
						{
							Category = groups["cat"].Success ? groups["cat"].Value : null,
							Name = groups["nam"].Value,
							Keys = groups["seq"].Success ? groups["seq"].Value : null,
							Recent = true
						};

						Items.Insert(0, name);
						commands.Insert(0, cmd);
					}
				}
			}

			// preselect the first item
			if (Items.Count > 0 && !FreeText)
			{
				Items[0].Selected = true;
			}
		}
		#endregion public SetAutoCompleteList and LoadCommands


		#region private HidePopup and ShowPopUp
		private void HidePopup(object sender, EventArgs e)
		{
			if (popup?.Visible == true) // && !popup.Focused)
			{
				popup.Close();
			}
		}


		private void ShowPopup(object sender, EventArgs e)
		{
			if (Items.Count == 0 || popup?.Visible == true)
			{
				return;
			}

			if (sender is TextBox box && !box.Visible)
			{
				popup?.Close();
				return;
			}

			if (popup == null)
			{
				popup = new ToolStripDropDown
				{
					Margin = Padding.Empty,
					Padding = Padding.Empty,
					AutoClose = false
				};
				popup.Items.Add(new ToolStripControlHost(this)
				{
					Margin = Padding.Empty,
					Padding = Padding.Empty
				});

				Owner.FindForm().Move += HidePopup;

				manager.InitializeTheme(popup);
			}

			if (!popup.Visible)
			{
				var itemHeight = GetItemRect(0).Height;
				Height = (itemHeight + 1) * 15;

				popup.Show(Owner, new Point(0, Owner.Height));
			}
		}
		#endregion private HidePopup and ShowPopUp


		#region Overrides including OnDrawSubItem
		protected override void OnMouseClick(MouseEventArgs e)
		{
			base.OnMouseClick(e);
			var info = HitTest(e.Location);

			Logger.Current.WriteLine("mouseclick");
			if (info?.Item is ListViewItem item)
			{
				item.Selected = true;
				DoPreviewKeyDown(this, new PreviewKeyDownEventArgs(Keys.Enter));
				SendKeys.Send("{Enter}");
			}
		}


		protected override void OnClientSizeChanged(EventArgs e)
		{
			base.OnClientSizeChanged(e);
			if (Columns.Count > 0)
			{
				Columns[0].Width = ClientSize.Width;
			}
		}


		protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
		{
			Brush back;
			Brush fore;
			Brush high;
			
			if (e.Item.Selected)
			{
				back = new SolidBrush(manager.GetColor("Highlight"));
				fore = new SolidBrush(manager.GetColor("HighlightText"));
				high = new SolidBrush(manager.GetColor("GradientInactiveCaption"));
			}
			else
			{
				back = new SolidBrush(manager.GetColor("ListView"));
				fore = new SolidBrush(manager.GetColor("ControlText"));
				high = new SolidBrush(manager.GetColor("Highlight"));
			}

			e.Graphics.FillRectangle(back,
				e.Bounds.X, e.Bounds.Y + 1,
				e.Bounds.Width, e.Bounds.Height - 2);

			var source = matches.Any() ? matches : commands;
			var command = source[e.Item.Index];

			var drawn = false;
			float x;

			if (!string.IsNullOrWhiteSpace(Owner.Text))
			{
				// draw name in three parts: unmatched start, matched middle, unmatched end...

				var text = command.Name;

				// Owner is the bound TextBox
				var index = text.IndexOf(Owner.Text, StringComparison.InvariantCultureIgnoreCase);
				if (index >= 0)
				{
					string phrase;
					SizeF size;

					// track x-offset of each phrase
					x = e.Bounds.X;

					// phrase is in middle so draw prior phrase
					if (index > 0)
					{
						phrase = text.Substring(0, index);

						// when phrase is substring of word, GenericTypographic doesn't measure
						// trailing space and when it is prefaced by a space, GenericDefault
						// removes that space. So choose appropriate format carefully here
						var format = index < text.Length - 1 && text[index - 1] == ' '
							? StringFormat.GenericDefault
							: StringFormat.GenericTypographic;

						e.Graphics.DrawString(phrase, Font, fore, x, e.Bounds.Y, format);
						size = e.Graphics.MeasureString(phrase, Font, new PointF(x, e.Bounds.Y), format);
						x += size.Width;
					}

					// draw matched phrase
					phrase = text.Substring(index, Owner.Text.Length);
					e.Graphics.DrawString(phrase, highFont, high,
						x, e.Bounds.Y, StringFormat.GenericTypographic);

					size = e.Graphics.MeasureString(
						phrase, highFont, new PointF(x, e.Bounds.Y), StringFormat.GenericTypographic);

					x += size.Width;

					// draw remaining phrase
					index += Owner.Text.Length;
					if (index < text.Length)
					{
						phrase = text.Substring(index);
						e.Graphics.DrawString(phrase, Font, fore,
							x, e.Bounds.Y, StringFormat.GenericTypographic);
					}

					drawn = true;
				}
			}

			if (!drawn)
			{
				// if no matched part then draw entire name normally...

				e.Graphics.DrawString(command.Name, e.Item.Font, fore, e.Bounds);
			}

			// leave margin from right side to end of key sequence text
			x = e.Bounds.Width - 5;

			// did we match any Recent items at all?
			if (source[0].Recent)
			{
				if (e.ItemIndex == 0)
				{
					var annotation = RecentKicker;
					var size = e.Graphics.MeasureString(annotation, Font);
					// push key sequence positioning over to the left
					x -= size.Width;
					e.Graphics.DrawString(annotation, e.Item.Font, high, x, e.Bounds.Y);
				}

				// index of first common command found after all recent commands
				var common = 1;
				while (common < source.Count && source[common].Recent)
				{
					common++;
				}

				// divider line
				if (common < source.Count && e.ItemIndex == common - 1)
				{
					e.Graphics.DrawLine(Pens.Silver, // yes, this pen is hard-coded
						e.Bounds.X, e.Bounds.Y + e.Bounds.Height - 1,
						e.Bounds.Width, e.Bounds.Y + e.Bounds.Height - 1);
				}
				else if (common == e.ItemIndex)
				{
					var annotation = OtherKicker;
					var size = e.Graphics.MeasureString(annotation, Font);
					// push key sequence positioning over to the left
					x -= size.Width;
					e.Graphics.DrawString(annotation, e.Item.Font, high, x, e.Bounds.Y);
				}
			}

			// prefer category
			if (!string.IsNullOrWhiteSpace(command.Category))
			{
				if (e.Item.Index == 0 ||
					(e.Item.Index > 0 && command.Category != source[e.Item.Index - 1].Category))
				{
					if (e.Item.Index > 0)
					{
						// divider line
						e.Graphics.DrawLine(Pens.Silver, // yes, this pen is hard-coded
							e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Y);
					}

					var size = TextRenderer.MeasureText(command.Category, e.Item.Font);
					x -= size.Width;
					e.Graphics.DrawString(command.Category, e.Item.Font, high, x, e.Bounds.Y);
				}
			}
			// key sequence
			else if (!string.IsNullOrWhiteSpace(command.Keys))
			{
				var size = e.Graphics.MeasureString(command.Keys, Font);
				x -= size.Width + 5;

				var cap = e.Item.Selected
					? new SolidBrush(manager.GetColor("GradientInactiveCaption"))
					: new SolidBrush(manager.GetColor("ActiveCaption"));

				e.Graphics.DrawString(command.Keys, e.Item.Font, cap, x, e.Bounds.Y);
			}
		}


		protected override void OnItemSelectionChanged(ListViewItemSelectionChangedEventArgs e)
		{
			base.OnItemSelectionChanged(e);
			Owner.Focus();
		}
		#endregion Overrides including OnDrawSubItem


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Text changed handlers

		private void DoTextChanged(object sender, EventArgs e)
		{
			// as the TextBox.Text changes, this finds matches in the command list and
			// highlights the first one; the TextBox value is not modified here...

			if (Items.Count == 0)
			{
				return;
			}

			var text = FreeText ? ClosestWord(out _) : Owner.Text.Trim();

			// make sure we're not duplicate effort here
			if (text == boxtext)
			{
				return;
			}

			var hadMatches = matches.Any();
			matches.Clear();

			commands.ForEach(cmd =>
			{
				if (cmd.Name.ContainsICIC(text))
				{
					matches.Add(cmd);
				}
			});

			if (matches.Any())
			{
				Items.Clear();
				matches.ForEach(m => Items.Add(m.Name));
				Items[0].Selected = !FreeText;
			}
			else if (hadMatches)
			{
				Items.Clear();
				commands.ForEach(c => Items.Add(c.Name));
				Items[0].Selected = !FreeText;
			}

			boxtext = text;
		}


		private string ClosestWord(out int start)
		{
			if (Owner.SelectionLength > 0)
			{
				start = Owner.SelectionStart;
				return Owner.SelectedText.Trim();
			}

			if (Owner.Text.Length == 0)
			{
				start = 0;
				return string.Empty;
			}

			start = Owner.SelectionStart;
			while (start > 0 && Owner.Text[start - 1].IsWordCharacter(WordChars))
			{
				start--;
			}

			var end = Owner.SelectionStart;
			while (end < Owner.Text.Length && Owner.Text[end].IsWordCharacter(WordChars))
			{
				end++;
			}

			// for inner words, trim off the trailing space
			if (end < Owner.Text.Length - 1 && !Owner.Text[end].IsWordCharacter(WordChars))
			{
				end--;
			}

			var len = end - start + 1;
			if (start + len > Owner.Text.Length - 1)
			{
				return Owner.Text.Substring(start, end - start);
			}

			var word = Owner.Text.Substring(start, end - start + 1);
			return word;
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Keyboard handlers

		private void DoPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			if (Items.Count == 0)
			{
				return;
			}

			// Catch the Enter key here in the preview handler so it's not superseded by
			// the higher level Form.AcceptButton handler
			if (e.KeyCode == Keys.Enter)
			{
				if (FreeText)
				{
					if (IsPopupVisible)
					{
						if (SelectedItems.Count > 0)
						{
							EditQuery();
						}

						HidePopup(sender, e);
					}
				}
				else if (SelectedItems.Count > 0)
				{
					// command mode so extract name, stripping keys
					var text = SelectedItems[0].Text;
					var index = text.IndexOf(KeyDivider);
					Owner.Text = index < 0 ? text : text.Substring(0, index);
					HidePopup(sender, e);
				}
			}
		}


		private void EditQuery()
		{
			var phrase = SelectedItems[0].Text; // presume no |keys
			var text = Owner.Text;

			var word = ClosestWord(out var start);
			if (word == string.Empty)
			{
				if (Owner.SelectionLength > 0)
				{
					text = text.Remove(start, Owner.SelectionLength);
				}
			}
			else
			{
				text = text.Remove(start, word.Length);
			}

			Owner.Text = text.Insert(start, phrase);
			Owner.SelectionLength = 0;
			// move caret after inserted phrase
			Owner.SelectionStart = start + phrase.Length;

			SelectedItems.Clear();
			matches.Clear();
		}


		private void DoKeydown(object sender, KeyEventArgs e)
		{
			if (Items.Count == 0)
			{
				return;
			}

			if (e.KeyCode == Keys.Enter && FreeText)
			{
				if (IsPopupVisible)
				{
					HidePopup(sender, e);
					e.Handled = true;
				}
				return;
			}

			if (e.KeyCode == Keys.Escape)
			{
				HidePopup(sender, e);
				e.Handled = true;
				return;
			}

			if (e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.ControlKey)
			{
				e.Handled = true;
				return;
			}

			ShowPopup(null, EventArgs.Empty);

			if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up ||
				e.KeyCode == Keys.PageDown || e.KeyCode == Keys.PageUp)
			{
				SelectItem(e.KeyCode);
				Owner.Focus();
				e.Handled = true;
			}

			void SelectItem(Keys keycode)
			{
				if (Items.Count == 0)
				{
					return;
				}

				if (SelectedItems.Count == 0)
				{
					Items[0].Selected = true;
					EnsureVisible(SelectedIndices.Count > 0 ? SelectedIndices[0] : 0);
				}
				else if (keycode == Keys.Down && SelectedItems[0].Index < Items.Count - 1)
				{
					Items[SelectedItems[0].Index + 1].Selected = true;
					EnsureVisible(SelectedIndices.Count > 0 ? SelectedIndices[0] : 0);
				}
				else if (keycode == Keys.Up && SelectedItems[0].Index > 0)
				{
					Items[SelectedItems[0].Index - 1].Selected = true;
					EnsureVisible(SelectedIndices.Count > 0 ? SelectedIndices[0] : 0);
				}
				else if (
					(keycode == Keys.PageDown && SelectedItems[0].IndentCount < Items.Count - 1) ||
					(keycode == Keys.PageUp && SelectedItems[0].Index > 0))
				{
					var max = Items.Count - 1;
					var top = TopItem.Index;
					var itemHeight = GetItemRect(top).Height;
					var visible = Height / (itemHeight + 1);
					var bottom = Math.Min(top + visible - 1, max);
					var selected = SelectedIndices[0];
					if (keycode == Keys.PageDown)
					{
						selected = selected < bottom ? bottom : Math.Min(selected + visible - 1, max);
					}
					else
					{
						selected = selected > top ? top : Math.Max(selected - visible + 1, 0);
					}

					Items[selected].Selected = true;
					EnsureVisible(SelectedIndices.Count > 0 ? SelectedIndices[0] : 0);
				}
			}
		}
	}
}
