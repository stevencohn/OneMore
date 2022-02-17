//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable CS3001  // Type is not CLS-compliant

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	/// <summary>
	/// A dialog to view page and hierarchy XML and update page XML if desired.
	/// </summary>
	internal partial class ShowXmlDialog : UI.LocalizableForm
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
					"pageInfoLabel",
					"hideBox",
					"hideLFBox",
					// manualPanel
					"manualLabel",
					"hidePidBox",
					// tabs
					"pageTab",
					"sectionTab",
					"notebooksTab",
					"nbSectionsTab",
					"nbPagesTab",
					"manualTab",
					// bottom info/buttons
					"pageNameLabel",
					"pagePathLabel",
					"pageLinkLabel",
					"okButton",
					"cancelButton=word_Cancel"
				});
			}

			Width = Math.Min(2000, (int)(Screen.PrimaryScreen.WorkingArea.Width * 0.8));
			Height = Math.Min(1500, (int)(Screen.PrimaryScreen.WorkingArea.Height * 0.8));

			manualPanel.Location = pagePanel.Location;
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
			using (var one = new OneNote())
			{
				var info = one.GetPageInfo();
				pageName.Text = info.Name;
				pagePath.Text = info.Path;
				pageLink.Text = info.Link;
			}
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
				FindClicked(null, null);
			}

			return base.ProcessDialogKey(keyData);
		}


		private void Close(object sender, EventArgs e)
		{
			Close();
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private void FindTextChanged(object sender, EventArgs e)
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


		private void FindClicked(object sender, EventArgs e)
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


		private void FindBoxKeyUP(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				FindClicked(sender, e);
				e.Handled = true;
			}
		}


		private void XmlBoxKeyUp(object sender, KeyEventArgs e)
		{
			if (e.Control && (e.KeyCode == Keys.F))
			{
				findBox.Focus();
			}
		}


		private void ChangeWrap(object sender, EventArgs e)
		{
			var box = tabs.TabPages[tabs.SelectedIndex].Controls[0] as RichTextBox;
			box.WordWrap = wrapBox.Checked;
			box.Focus();
		}


		private void SelectAllClicked(object sender, EventArgs e)
		{
			var box = tabs.TabPages[tabs.SelectedIndex].Controls[0] as RichTextBox;
			box.SelectAll();
			box.Focus();
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private void ScopeSelectedValueChanged(object sender, EventArgs e)
		{
			if (Enum.TryParse<OneNote.PageDetail>(scopeBox.Text, out var info))
			{
				using (var one = new OneNote())
				{
					var page = one.GetPage(info);
					if (page != null)
					{
						var xml = page.Root.ToString(SaveOptions.None);
						pageBox.WordWrap = wrapBox.Checked;
						pageBox.Text = xml;

						ApplyHideOptions();

						logger.WriteLine($"XmlDialog loaded page, scope {info}, {xml.Length} chars");
					}
				}
			}
		}


		private void HideCheckedChanged(object sender, EventArgs e)
		{
			ScopeSelectedValueChanged(sender, e);
			okButton.Enabled = !hideBox.Checked;
		}


		private void ApplyHideOptions()
		{
			var root = XElement.Parse(pageBox.Text);

			if (hideBox.Checked)
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
					|| a.Name.LocalName == "objectID")
					.Remove();
			}

			if (hideLFBox.Checked)
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

			pageBox.Text = root.ToString(SaveOptions.None);

			Highlights();
		}


		private void Highlights()
		{
			var matches = Regex.Matches(pageBox.Text,
				@"<((?:[a-zA-Z][a-zA-Z0-9]*?:)?[a-zA-Z][a-zA-Z0-9]*?)[^>]+selected=""all""[^\1]*?\1>");

			foreach (Match match in matches)
			{
				pageBox.SelectionStart = match.Index;
				pageBox.SelectionLength = match.Length;
				pageBox.SelectionBackColor = Color.Yellow;
			}

			if (!hideBox.Checked)
			{
				// author attributes
				matches = Regex.Matches(pageBox.Text,
					"(?:author|authorInitials|authorResolutionID|lastModifiedBy|" +
					"lastModifiedByInitials|lastModifiedByResolutionID|creationTime|" +
					"lastModifiedTime)=\"[^\"]*\""
					);

				foreach (Match m in matches)
				{
					pageBox.SelectionStart = m.Index;
					pageBox.SelectionLength = m.Length;
					pageBox.SelectionColor = Color.Silver;
				}

				// objectID
				matches = Regex.Matches(pageBox.Text, "(?:objectID)=\"[^\"]*\"");

				foreach (Match m in matches)
				{
					pageBox.SelectionStart = m.Index;
					pageBox.SelectionLength = m.Length;
					pageBox.SelectionColor = Color.CornflowerBlue;
				}
			}
		}


		// async event handlers should be be declared 'async void'
		private async void Update(object sender, EventArgs e)
		{
			var result = MessageBox.Show(
				"Are you sure? This may corrupt the current page.",
				"Feelin lucky punk?",
				MessageBoxButtons.OKCancel,
				MessageBoxIcon.Warning);

			if (result == DialogResult.OK)
			{
				try
				{
					using (var one = new OneNote())
					{
						await one.Update(XElement.Parse(pageBox.Text));
					}

					Close();

					// doesn't account for binary data, but close enough
					logger.WriteLine($"updating {pageBox.Text.Length} bytes");
				}
				catch (Exception exc)
				{
					UIHelper.ShowError(
						$"Error updating page content: {exc.Message}\n\nSee log for details");

					logger.WriteLine("error updating page content", exc);
				}
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private void TabsSelecting(object sender, TabControlCancelEventArgs e)
		{
			if (!((Control)e.TabPage).Enabled)
			{
				e.Cancel = true;
			}
		}


		private async void TabsSelectedIndexChanged(object sender, EventArgs e)
		{
			if (tabs.SelectedIndex == 0)
			{
				pageBox.Select(0, 0);
				pageBox.Focus();
				okButton.Visible = true;
				pagePanel.Visible = true;
				manualPanel.Visible = false;
				wrapBox.Checked = pageBox.WordWrap;
			}
			else
			{
				switch (tabs.SelectedIndex)
				{
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
				}

				wrapBox.Checked = ((RichTextBox)tabs.SelectedTab.Controls[0]).WordWrap;
				hidePidBox.Checked = tabs.SelectedTab.Tag == null || (bool)tabs.SelectedTab.Tag;

				okButton.Visible = false;
				pagePanel.Visible = false;
				manualPanel.Visible = true;
			}
		}


		private async Task ShowHierarchy(
			RichTextBox box, string comment, Func<OneNote, Task<XElement>> action)
		{
			if (box.TextLength == 0)
			{
				using (var one = new OneNote())
				{
					var root = await action(one);

					if (root != null)
					{
						if (hidePidBox.Checked)
						{
							Sanitize(root);
						}

						box.Clear();
						box.WordWrap = wrapBox.Checked;
						box.SelectionColor = Color.Black;
						box.Text = $"<!-- {comment} -->\n{root.ToString(SaveOptions.None)}";
						box.Select(0, comment.Length + 9);
						box.SelectionColor = Color.Green;
					}
					else
					{
						box.Text = "no hierarchy";
					}
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


		private void HidePidCheckedChanged(object sender, EventArgs e)
		{
			var box = tabs.TabPages[tabs.SelectedIndex].Controls[0] as RichTextBox;
			box.Clear();

			tabs.TabPages[tabs.SelectedIndex].Tag = hidePidBox.Checked;

			TabsSelectedIndexChanged(sender, e);
		}


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

				using (var one = new OneNote())
				{
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
				}

				if (content == null)
				{
					UIHelper.ShowMessage("Cannot find object ID");
					return;
				}

				((Control)manualTab).Enabled = true;
				tabs.SelectTab(5);

				manualBox.Clear();
				manualBox.SelectionColor = Color.Black;

				await ShowHierarchy(manualBox,
					$"{(string)functionBox.SelectedItem}(\"{objectIdBox.Text}\")",
					async (one) => { await Task.Yield(); return content; });
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
	}
}
