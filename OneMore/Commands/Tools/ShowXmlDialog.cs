//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable CS3001  // Type is not CLS-compliant

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// A dialog to view page and hierarchy XML and update page XML if desired.
	/// </summary>
	internal partial class ShowXmlDialog : LocalizableForm
	{
		private int findIndex = -1;


		public ShowXmlDialog()
		{
			InitializeComponent();

			if (DesignMode)
			{
				AutoScaleDimensions = new SizeF(96f, 96f);
				AutoScaleMode = AutoScaleMode.Dpi;
				Logger.SetDesignMode(DesignMode);
			}

			if (NeedsLocalizing())
			{
				Text = Resx.ShowXmlDialog_Text;

				Localize(new string[]
				{
					"wrapBox",
					"selectButton",
					// pagePanel
					"hideEditedByBox",
					"multilineBox",
					"linefeedBox",
					"editModeBox",
					"saveWindowBox",
					"pageInfoLabel",
					// manualPanel
					"hideEditedByBox2=ShowXmlDialog_hideEditedByBox.Text",
					"multilineBox2=ShowXmlDialog_multilineBox.Text",
					"pidBox",
					"manualLabel",
					"fnLabel=word_Function",
					// tabs
					"pageTab=word_Page",
					"sectionTab=word_Section",
					"notebooksTab",
					"nbSectionsTab",
					"nbPagesTab",
					"manualTab",
					// bottom info/buttons
					"pageNameLabel",
					"pagePathLabel",
					"pageLinkLabel",
					"okButton",
					"cancelButton=word_Close"
				});
			}

			manualPanel.Location = pageOptionsPanel.Location;
			((Control)manualTab).Enabled = false;
		}


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			// build scopeBox with custom order
			var type = typeof(OneNote.PageDetail);
			var names = new List<string>
			{
				Enum.GetName(type, OneNote.PageDetail.All),
				Enum.GetName(type, OneNote.PageDetail.Selection),
				Enum.GetName(type, OneNote.PageDetail.Basic),
				Enum.GetName(type, OneNote.PageDetail.BinaryData),
				Enum.GetName(type, OneNote.PageDetail.BinaryDataSelection),
				Enum.GetName(type, OneNote.PageDetail.BinaryDataFileType),
				Enum.GetName(type, OneNote.PageDetail.FileType),
				Enum.GetName(type, OneNote.PageDetail.SelectionFileType)
			};

			scopeBox.Items.AddRange(names.ToArray());
			scopeBox.SelectedIndex = names.IndexOf("Selection");

			// populate page info...
			using var one = new OneNote();
			var info = one.GetPageInfo(sized: true);
			pageName.Text = $"{info.Name} ({info.Size.ToBytes()})";
			pagePath.Text = info.Path;
			pageLink.Text = info.Link;

			var settings = new SettingsProvider().GetCollection("XmlDialog");
			saveWindowBox.Checked = settings.Count > 0;

			Width = settings.Get("width",
				Math.Min(2000, (int)(Screen.PrimaryScreen.WorkingArea.Width * 0.8)));

			Height = settings.Get("height",
				Math.Min(1500, (int)(Screen.PrimaryScreen.WorkingArea.Height * 0.8)));

			Left = settings.Get("left", (Screen.PrimaryScreen.WorkingArea.Width - Width) / 2);
			Top = settings.Get("top", (Screen.PrimaryScreen.WorkingArea.Height - Height) / 2);

			if (Left < 0 || Left > Screen.PrimaryScreen.WorkingArea.Width) Left = 0;
			if (Top < 0 || Top > Screen.PrimaryScreen.WorkingArea.Height) Top = 0;
		}


		protected override void OnShown(EventArgs e)
		{
			//Location = new System.Drawing.Point(30, 30);
			UIHelper.SetForegroundWindow(this);
			findBox.Focus();
		}


		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (keyData == (Keys.F | Keys.Control))
			{
				// Find
				findBox.SelectAll();
				findBox.Focus();
				return true;
			}
			else if (keyData == Keys.F3)
			{
				// Find next
				FindOnClick(null, null);
			}

			return base.ProcessDialogKey(keyData);
		}


		private void Close(object sender, EventArgs e)
		{
			Close();
		}


		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			var settings = new SettingsProvider();
			if (saveWindowBox.Checked)
			{
				var collection = settings.GetCollection("XmlDialog");
				collection.Add("left", Left);
				collection.Add("top", Top);
				collection.Add("width", Width);
				collection.Add("height", Height);
				settings.SetCollection(collection);
			}
			else
			{
				settings.RemoveCollection("XmlDialog");
			}

			settings.Save();

			base.OnFormClosing(e);
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private void FindOptionsOnTextChanged(object sender, EventArgs e)
		{
			if (findBox.Text.Length == 0)
			{
				findIndex = -1;
				findButton.Enabled = false;
			}
			else if (!findButton.Enabled)
			{
				findIndex = -1;
				findButton.Enabled = true;
			}
		}


		private void FindOnClick(object sender, EventArgs e)
		{
			var box = tabs.TabPages[tabs.SelectedIndex].Controls[0] as RichTextBox;
			var index = FindNext(box, findBox.Text);

			if (index > 0)
			{
				box.Select(index, findBox.Text.Length);
				box.Focus();
				findIndex = index;
			}
		}


		private int FindNext(RichTextBox box, string text)
		{
			var index = box.Find(text, findIndex + 1, RichTextBoxFinds.None);

			if ((index < 0) && (findIndex > 0))
			{
				// wrap back to top and try again
				findIndex = -1;
				index = box.Find(text, 0, RichTextBoxFinds.None);
			}

			return index;
		}


		private void FindOnKeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				FindOnClick(sender, e);
				e.Handled = true;
			}
		}


		private void XmlBoxKeyHandlerOnKeyUp(object sender, KeyEventArgs e)
		{
			if (e.Control && (e.KeyCode == Keys.F))
			{
				findBox.Focus();
			}
			else if (sender == pageBox && e.KeyCode == Keys.F3)
			{
				// Find next
				FindOnClick(sender, e);
			}
		}


		private void SelectAll(object sender, EventArgs e)
		{
			var box = tabs.TabPages[tabs.SelectedIndex].Controls[0] as RichTextBox;
			box.SelectAll();
			box.Focus();
		}


		private void ToggleWrap(object sender, EventArgs e)
		{
			var box = tabs.TabPages[tabs.SelectedIndex].Controls[0] as RichTextBox;
			box.WordWrap = wrapBox.Checked;
			box.Focus();
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Page Editor

		private void RefreshPage(object sender, EventArgs e)
		{
			if (!Enum.TryParse<OneNote.PageDetail>(scopeBox.Text, out var scope))
			{
				// should never happen!
				return;
			}

			using var one = new OneNote();
			var page = one.GetPage(scope);
			if (page == null)
			{
				// should never happen!
				return;
			}

			var xml = Format(
				page.Root.ToString(SaveOptions.None),
				hideEditedByBox.Checked, multilineBox.Checked, linefeedBox.Checked);

			pageBox.Clear();
			pageBox.Text = xml;

			Colorize(pageBox, !hideEditedByBox.Checked);

			logger.WriteLine($"XmlDialog loaded page, scope {scope}, {xml.Length} chars");
		}


		private void RefreshOnClick(object sender, EventArgs e)
		{
			// editedByBox clicked
			// rely on Clicked event instead of Checked changed because editedByBox is
			// also controlled by editedByBox2

			if (sender == hideEditedByBox)
				hideEditedByBox2.Checked = hideEditedByBox.Checked;
			else if (sender == hideEditedByBox2)
				hideEditedByBox.Checked = hideEditedByBox2.Checked;
			else if (sender == multilineBox)
				multilineBox2.Checked = multilineBox.Checked;
			else if (sender == multilineBox2)
				multilineBox.Checked = multilineBox2.Checked;

			if (tabs.SelectedIndex == 0)
			{
				RefreshPage(sender, e);
				MarkHierarchyTabs(-1);
			}
			else
			{
				((RichTextBox)tabs.TabPages[tabs.SelectedIndex].Controls[0]).Clear();
				RefreshHierarchy(sender, e);
				RefreshPage(sender, e);
			}
		}


		private string Format(string xml, bool hideEditedBy, bool multiline, bool hideLinefeeds)
		{
			var root = XElement.Parse(xml);

			if (hideEditedBy)
			{
				// EditedByAttributes and others
				root.Descendants().Attributes().Where(a =>
					a.Name.LocalName == "author"
					|| a.Name.LocalName == "authorInitials"
					|| a.Name.LocalName == "authorResolutionID"
					|| a.Name.LocalName == "lastModifiedBy"
					|| a.Name.LocalName == "lastModifiedByInitials"
					|| a.Name.LocalName == "lastModifiedByResolutionID"
					|| a.Name.LocalName == "creationTime"
					|| a.Name.LocalName == "lastModifiedTime"
					|| a.Name.LocalName == "objectID"
					|| a.Name.LocalName == "ID")
					.Remove();
			}
			else
			{
				// reshuffle attributes...

				root.Descendants()
					.Where(e => e.Attributes("objectID").Any() || e.Attributes("ID").Any())
					.ForEach(e =>
					{
						var attributes = e.Attributes().ToList();

						// move objectID, lastModifiedTime, and creationTime to beginning of list
						// ... this is reverse-order
						var moved = PromoteAttribute(attributes, "creationTime");
						moved = PromoteAttribute(attributes, "lastModifiedTime") || moved;
						moved = PromoteAttribute(attributes, "objectID") || moved;
						moved = PromoteAttribute(attributes, "ID") || moved;
						moved = PromoteAttribute(attributes, "nickname") || moved;
						moved = PromoteAttribute(attributes, "name") || moved;

						if (moved)
						{
							e.ReplaceAttributes(attributes);
						}

						e.ReplaceAttributes(attributes);
					});
			}

			if (hideLinefeeds)
			{
				var nodes = root.DescendantNodes().OfType<XCData>();
				if (!nodes.IsNullOrEmpty())
				{
					foreach (var cdata in nodes)
					{
						cdata.Value = cdata.Value
							.Replace("\nstyle", " style")
							.Replace("\nhref", " href")
							.Replace(";\nfont-size:", ";font-size:")
							.Replace(";\ncolor:", ";color:")
							.Replace(":\n", ": ");
					}
				}
			}

			return multiline
				? root.PrettyPrint()
				: root.ToString(SaveOptions.None);
		}


		private bool PromoteAttribute(List<XAttribute> attributes, string name)
		{
			var att = attributes.FirstOrDefault(a => a.Name == name);
			if (att != null)
			{
				attributes.Remove(att);
				attributes.Insert(0, att);
				return true;
			}

			return false;
		}


		private void Colorize(RichTextBox box, bool editedBy)
		{
			var matches = Regex.Matches(box.Text,
				@"<((?:[a-zA-Z][a-zA-Z0-9]*?:)?[a-zA-Z][a-zA-Z0-9]*?)[^>]+selected=""all""[^\1]*?\1>");

			foreach (Match match in matches)
			{
				box.SelectionStart = match.Index;
				box.SelectionLength = match.Length;
				box.SelectionBackColor = Color.Yellow;
			}

			if (editedBy)
			{
				// author attributes
				matches = Regex.Matches(box.Text,
					"(?:author|authorInitials|authorResolutionID|lastModifiedBy|" +
					"lastModifiedByInitials|lastModifiedByResolutionID|creationTime|" +
					"lastModifiedTime|dateTime)=\"[^\"]*\""
					);

				foreach (Match m in matches)
				{
					box.SelectionStart = m.Index;
					box.SelectionLength = m.Length;
					box.SelectionColor = Color.Silver;
				}

				// objectID
				matches = Regex.Matches(box.Text, "(?:objectID|ID)=\"[^\"]*\"");

				foreach (Match m in matches)
				{
					box.SelectionStart = m.Index;
					box.SelectionLength = m.Length;
					box.SelectionColor = Color.CornflowerBlue;
				}
			}
		}


		private void ToggleEditMode(object sender, EventArgs e)
		{
			if (editModeBox.Checked)
			{
				pageBox.BackColor = Color.White;
				pageBox.ReadOnly = false;
				okButton.Enabled = true;
				hideEditedByBox.Checked = false;
				hideEditedByBox.Enabled = false;
				multilineBox.Enabled = false;
				linefeedBox.Enabled = false;
				scopeBox.Enabled = false;
				wrapBox.Enabled = false;

				RefreshPage(sender, e);
			}
			else
			{
				pageBox.BackColor = SystemColors.Control;
				pageBox.ReadOnly = true;
				okButton.Enabled = false;
				hideEditedByBox.Enabled = true;
				multilineBox.Enabled = true;
				linefeedBox.Enabled = true;
				scopeBox.Enabled = true;
				wrapBox.Enabled = true;
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Hierarchy Viewers

		private void TabGuard(object sender, TabControlCancelEventArgs e)
		{
			// prevents moving to the Manual tab until its contents are enabled
			if (!((Control)e.TabPage).Enabled)
			{
				e.Cancel = true;
			}
		}

		private void TogglePid(object sender, EventArgs e)
		{
			((RichTextBox)tabs.TabPages[tabs.SelectedIndex].Controls[0]).Clear();
			RefreshHierarchy(sender, e);
		}


		private async void RefreshHierarchy(object sender, EventArgs e)
		{
			var enabled = true;

			switch (tabs.SelectedIndex)
			{
				case 0: // Page
					pageBox.Select(0, 0);
					pageBox.Focus();
					okButton.Visible = true;
					pageOptionsPanel.Visible = true;
					manualPanel.Visible = false;
					wrapBox.Checked = pageBox.WordWrap;
					return;

				case 1: // Section+Pages
					await ShowHierarchy(sectionBox, "one.GetSection()",
						async (one) => { await Task.Yield(); return one.GetSection(); });
					break;

				case 2: // Notebooks
					await ShowHierarchy(notebookBox, "one.GetNotebooks()",
						async (one) => { return await one.GetNotebooks(); });
					break;

				case 3: // Notebook+Sections
					await ShowHierarchy(nbSectionBox, "one.GetNotebook()",
						async (one) => { return await one.GetNotebook(); });
					break;

				case 4: // Notebook+Sections+Pages
					await ShowHierarchy(nbPagesBox, "one.GetNotebook(OneNote.Scope.Pages)",
						async (one) => { return await one.GetNotebook(OneNote.Scope.Pages); });
					break;

				case 5:
					enabled = false;
					break;
			}

			hideEditedByBox2.Enabled = enabled;
			multilineBox2.Enabled = enabled;
			pidBox.Enabled = enabled;

			if (sender is not TabControl)
			{
				// changed filtering so mark other tabs as dirty
				MarkHierarchyTabs(tabs.SelectedIndex);
			}

			if (okButton.Visible)
			{
				okButton.Visible = false;
				pageOptionsPanel.Visible = false;
				manualPanel.Visible = true;
			}
		}


		private void MarkHierarchyTabs(int selected)
		{
			for (int i = 1; i <= 4; i++)
			{
				if (i != selected)
				{
					((RichTextBox)tabs.TabPages[i].Controls[0]).Clear();
				}
			}
		}


		private async Task ShowHierarchy(
			RichTextBox box, string comment, Func<OneNote, Task<XElement>> action)
		{
			if (box.TextLength == 0)
			{
				using var one = new OneNote();
				var root = await action(one);

				if (root != null)
				{
					if (pidBox.Checked)
					{
						Sanitize(root);
					}

					logger.WriteLine($"box1={hideEditedByBox2.Checked} box2={multilineBox2.Checked}");

					var xml = Format(
						root.ToString(SaveOptions.None),
						hideEditedByBox2.Checked, multilineBox2.Checked, linefeedBox.Checked);

					box.Clear();
					box.WordWrap = wrapBox.Checked;
					box.SelectionColor = Color.Black;
					box.Text = $"<!-- {comment} -->\n{xml}";
					box.Select(0, comment.Length + 9);
					box.SelectionColor = Color.Green;

					Colorize(box, !hideEditedByBox2.Checked);

					logger.WriteLine($"XmlDialog loaded hierarchy, {comment}, {xml.Length} chars");
				}
				else
				{
					box.Text = Resx.ShowXmlDialog_noHierarchy;
				}
			}

			box.Select(0, 0);
			box.Focus();
		}


		private void Sanitize(XElement root)
		{
			var paths = root.DescendantsAndSelf()
				.Where(e => e.Attribute("path") != null)
				.Select(e => e.Attribute("path"));

			if (paths.Any())
			{
				var regex = new Regex(@"(http[s]?://.+live\.net/)([0-9a-f]+)(/.+)");
				foreach (var path in paths)
				{
					path.Value = regex.Replace(path.Value, "$1xxxxx$3");
				}
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Manual Query

		private void ManualInputChanged(object sender, EventArgs e)
		{
			// validate the input...

			queryButton.Enabled =
				objectIdBox.Text.Trim().Length > 0 &&
				functionBox.SelectedIndex >= 0;
		}


		private async void RunManual(object sender, EventArgs e)
		{
			try
			{
				XElement content = null;

				using var one = new OneNote();

				switch (functionBox.SelectedIndex)
				{
					case 0:
						content = await one.GetNotebook(objectIdBox.Text, OneNote.Scope.Pages);
						break;

					case 1:
						content = one.GetSection(objectIdBox.Text);
						break;

					case 2:
						content = one.GetPage(objectIdBox.Text, OneNote.PageDetail.BinaryData).Root;
						break;
				}

				if (content == null)
				{
					UIHelper.ShowMessage("Cannot find object ID");
					return;
				}

				((Control)manualTab).Enabled = true;
				tabs.SelectTab(5);

				// supress rendering
				Native.SendMessage(manualBox.Handle, Native.WM_SETREDRAW, 0, 0);
				var eventMask = Native.SendMessage(manualBox.Handle, Native.EM_GETEVENTMASK, 0, 0);

				manualBox.Clear();
				manualBox.SelectionColor = Color.Black;

				await ShowHierarchy(manualBox,
					$"{(string)functionBox.SelectedItem}(\"{objectIdBox.Text}\")",
					async (one) => { await Task.Yield(); return content; });

				// resume rendering
				Native.SendMessage(manualBox.Handle, Native.EM_SETEVENTMASK, 0, eventMask);
				Native.SendMessage(manualBox.Handle, Native.WM_SETREDRAW, 1, 0);
				manualBox.Invalidate();

			}
			catch (Exception exc)
			{
				if (exc is COMException cex)
				{
					if ((uint)cex.ErrorCode == 0x80042014)
					{
						UIHelper.ShowMessage("Invalid ObjectID");
						return;
					}
				}

				manualBox.SelectionColor = Color.Black;
				manualBox.Text = exc.FormatDetails();
				manualBox.SelectAll();
				manualBox.SelectionColor = Color.Red;

				logger.WriteLine(exc);
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Update Page

		private async void UpdatePage(object sender, EventArgs e)
		{
			var result = UIHelper.ShowQuestion(Resx.ShowXmlDialog_WARNING);
			if (result == DialogResult.OK)
			{
				try
				{
					using var one = new OneNote();
					await one.Update(new Models.Page(XElement.Parse(pageBox.Text)));

					Close();

					// doesn't account for binary data, but close enough
					logger.WriteLine($"updating {pageBox.Text.Length} bytes");
				}
				catch (Exception exc)
				{
					MoreMessageBox.ShowErrorWithLogLink(Owner,
						$"Error updating page content: {exc.Message}\n\nSee log for details");

					logger.WriteLine("error updating page content", exc);
				}
			}
		}
	}
}
