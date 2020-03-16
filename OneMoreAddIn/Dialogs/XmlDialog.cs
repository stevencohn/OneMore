//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  Yada yada...
//************************************************************************************************

#pragma warning disable CS3001 // Type is not CLS-compliant

namespace River.OneMoreAddIn
{
	using System;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Microsoft.Office.Interop.OneNote;


	/// <summary>
	/// View page and hierarchy XML. Update page XML if desired.
	/// </summary>
	/// <remarks>
	/// Disposables: Local ApplicationManager disposed in OnClosed.
	/// </remarks>

	internal partial class XmlDialog : Form, IOneMoreWindow
	{

		private ApplicationManager manager;
		private ILogger logger;


		public XmlDialog ()
		{
			InitializeComponent();

			Logger.DesignMode = DesignMode;
			logger = Logger.Current;

			this.Width = (int)(Screen.PrimaryScreen.WorkingArea.Width * 0.8);
			this.Height = (int)(Screen.PrimaryScreen.WorkingArea.Height * 0.8);
			if (Width > 2500) Width = 2500;
			if (Height > 1500) Height = 1500;
		}

		#region Lifecycle

		private void MainForm_Load (object sender, EventArgs e)
		{
			manager = new ApplicationManager();

			var infoNames = Enum.GetNames(typeof(PageInfo));
			pageInfoBox.Items.AddRange(infoNames);
			pageInfoBox.SelectedIndex = infoNames.ToList().IndexOf("piSelection");
		}

		private void pageInfoBox_SelectedIndexChanged (object sender, EventArgs e)
		{
			if (Enum.TryParse<PageInfo>(pageInfoBox.Text, out var info))
			{
				var page = manager.CurrentPage(info);
				if (page != null)
				{
					var xml = page.ToString(SaveOptions.None);
					var pi = manager.GetCurrentPageInfo();

					pageBox.Text =
						//$"Name=[{pi.Name}]" + Environment.NewLine +
						//$"Path=[{pi.Path}]" + Environment.NewLine +
						//$"Link=[{pi.Link}]" + Environment.NewLine +
						//Environment.NewLine +
						xml;

					logger.WriteLine("XmlDialog loaded page, " + xml.Length + " chars");
				}
			}
		}


		protected override void OnShown (EventArgs e)
		{
			UIHelper.SetForegroundWindow(this);
			findBox.Focus();
		}


		private void XmlDialog_FormClosing (object sender, FormClosingEventArgs e)
		{
			manager?.Dispose();
		}

		private void closeButton_Click (object sender, EventArgs e)
		{
			Close();
		}


		private void tabs_SelectedIndexChanged (object sender, EventArgs e)
		{
			if (tabs.SelectedIndex == 0)
			{
				pageBox.Select(0, 0);
				pageBox.Focus();
				updateButton.Visible = true;
				pageInfoPanel.Visible = true;
				wrapBox.Checked = pageBox.WordWrap;
			}
			else
			{
				if (hierBox.TextLength == 0)
				{
					ShowHierarchy(HierarchyScope.hsNotebooks);
				}

				pageBox.Select(0, 0);
				pageBox.Focus();
				updateButton.Visible = false;
				pageInfoPanel.Visible = false;
				wrapBox.Checked = hierBox.WordWrap;
			}
		}

		private void wrapBox_CheckedChanged (object sender, EventArgs e)
		{
			if (tabs.SelectedIndex == 0)
			{
				pageBox.WordWrap = wrapBox.Checked;
			}
			else
			{
				hierBox.WordWrap = wrapBox.Checked;
			}
		}

		protected override bool ProcessDialogKey (Keys keyData)
		{
			if (keyData == (Keys)(Keys.F | Keys.Control))
			{
				findBox.SelectAll();
				findBox.Focus();
				return true;
			}

			return base.ProcessDialogKey(keyData);
		}

		#endregion Lifecycle

		#region Selection action

		private void selectButton_Click (object sender, EventArgs e)
		{
			if (tabs.SelectedIndex == 0)
			{
				pageBox.SelectAll();
				pageBox.Focus();
			}
			else
			{
				hierBox.SelectAll();
				hierBox.Focus();
			}
		}

		#endregion Selection action

		#region Find actions

		int findIndex = -1;
		private void findButton_Click (object sender, EventArgs e)
		{
			RichTextBox box = tabs.SelectedIndex == 0 ? pageBox : hierBox;
			var index = SearchOne(box, findBox.Text);
			if ((index < 0) && (findIndex > 0))
			{
				findIndex = -1;
				index = SearchOne(box, findBox.Text);
			}
		}

		private int SearchOne (RichTextBox box, string text)
		{
			var index = box.Find(text, findIndex + 1, RichTextBoxFinds.None);
			if (index > findIndex)
			{
				box.Select(index, findBox.Text.Length);
				box.Focus();
				findIndex = index;
			}

			return index;
		}

		private void findBox_TextChanged (object sender, EventArgs e)
		{
			if (findBox.Text.Length == 0)
			{
				findIndex = -1;
				findButton.Enabled = false;
			}
			else
			{
				if (findButton.Enabled == false)
				{
					findIndex = -1;
					findButton.Enabled = true;
				}
			}
		}

		private void findBox_KeyUp (object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				findButton_Click(sender, e);
				e.Handled = true;
			}
		}

		private void pageBox_KeyUp (object sender, KeyEventArgs e)
		{
			if (e.Control && (e.KeyCode == Keys.F))
			{
				findBox.Focus();
			}
		}

		#endregion Find actions

		#region Hierarchy actions

		private void notebooksHierButton_CheckedChanged (object sender, EventArgs e)
		{
			ShowHierarchy(HierarchyScope.hsNotebooks);
		}

		private void sectionsHierButton_CheckedChanged (object sender, EventArgs e)
		{
			ShowHierarchy(HierarchyScope.hsSections);
		}

		private void pagesHierButton_CheckedChanged (object sender, EventArgs e)
		{
			ShowHierarchy(HierarchyScope.hsPages);
		}

		private void currNotebookButton_CheckedChanged (object sender, EventArgs e)
		{
			var element = manager.CurrentNotebook();
			if (element != null)
			{
				var xml = element.ToString(SaveOptions.None);
				hierBox.Text = xml;
			}
			else
			{
				hierBox.Text = "Cannot get current notebook hierarchy";
			}
		}

		private void currSectionButton_CheckedChanged (object sender, EventArgs e)
		{
			var element = manager.CurrentSection();
			if (element != null)
			{
				var xml = element.ToString(SaveOptions.None);
				hierBox.Text = xml;
			}
			else
			{
				hierBox.Text = "Cannot get current section hierarchy";
			}
		}


		private void ShowHierarchy (HierarchyScope scope)
		{
			var element = manager.GetHierarchy(scope);
			if (element != null)
			{
				var xml = element.ToString(SaveOptions.None);
				hierBox.Text = xml;
			}
			else
			{
				hierBox.Text = $"Cannot get hierarchy for {scope.ToString()}";
			}
		}

		#endregion Hierarchy actions


		private void updateButton_Click (object sender, EventArgs e)
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
					var page = XElement.Parse(pageBox.Text);
					manager.UpdatePageContent(page);
					Close();
				}
				catch (Exception exc)
				{
					logger.WriteLine("Error updating page content");
					logger.WriteLine(exc);
				}
			}
		}
	}
}
