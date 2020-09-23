//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  Yada yada...
//************************************************************************************************

#pragma warning disable CS3001  // Type is not CLS-compliant

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
		private readonly ILogger logger;


		public XmlDialog()
		{
			InitializeComponent();

			Logger.DesignMode = DesignMode;
			logger = Logger.Current;

			this.Width = (int)(Screen.PrimaryScreen.WorkingArea.Width * 0.8);
			this.Height = (int)(Screen.PrimaryScreen.WorkingArea.Height * 0.8);
			if (Width > 2000) Width = 2000;
			if (Height > 1500) Height = 1500;
		}

		#region Lifecycle

		private void MainForm_Load(object sender, EventArgs e)
		{
			manager = new ApplicationManager();

			var infoNames = Enum.GetNames(typeof(PageInfo));
			pageInfoBox.Items.AddRange(infoNames);
			pageInfoBox.SelectedIndex = infoNames.ToList().IndexOf("piSelection");
		}

		private void ChangeInfoScope(object sender, EventArgs e)
		{
			if (Enum.TryParse<PageInfo>(pageInfoBox.Text, out var info))
			{
				var page = manager.CurrentPage(info);
				if (page != null)
				{
					var xml = page.ToString(SaveOptions.None);
					//var pi = manager.GetCurrentPageInfo();

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


		protected override void OnShown(EventArgs e)
		{
			//Location = new System.Drawing.Point(30, 30);
			UIHelper.SetForegroundWindow(this);
			findBox.Focus();
		}


		private void XmlDialog_FormClosing(object sender, FormClosingEventArgs e)
		{
			manager?.Dispose();
		}

		private void Close(object sender, EventArgs e)
		{
			Close();
		}


		private void ChangeSelectedTab(object sender, EventArgs e)
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

		private void ChangeWrap(object sender, EventArgs e)
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


		private void HideAttributes(object sender, EventArgs e)
		{
			if (hideBox.Checked)
			{
				var root = XElement.Parse(pageBox.Text);

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

				pageBox.Text = root.ToString(SaveOptions.None);
			}
			else
			{
				ChangeInfoScope(sender, e);
			}
		}


		protected override bool ProcessDialogKey(Keys keyData)
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

		private void SelectAll(object sender, EventArgs e)
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
		private void ClickFind(object sender, EventArgs e)
		{
			RichTextBox box = tabs.SelectedIndex == 0 ? pageBox : hierBox;
			var index = SearchOne(box, findBox.Text);
			if ((index < 0) && (findIndex > 0))
			{
				findIndex = -1;
				SearchOne(box, findBox.Text);
			}
		}

		private int SearchOne(RichTextBox box, string text)
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

		private void ChangeFindText(object sender, EventArgs e)
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

		private void FindBoxKeyUP(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				ClickFind(sender, e);
				e.Handled = true;
			}
		}

		private void PageBoxKeyUp(object sender, KeyEventArgs e)
		{
			if (e.Control && (e.KeyCode == Keys.F))
			{
				findBox.Focus();
			}
		}

		#endregion Find actions

		#region Hierarchy actions

		private void ShowNotebooks(object sender, EventArgs e)
		{
			ShowHierarchy(HierarchyScope.hsNotebooks);
		}

		private void ShowSections(object sender, EventArgs e)
		{
			ShowHierarchy(HierarchyScope.hsSections);
		}

		private void ShowPages(object sender, EventArgs e)
		{
			ShowHierarchy(HierarchyScope.hsPages);
		}

		private void ShowCurrentNotebook(object sender, EventArgs e)
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

		private void ShowCurrentSection(object sender, EventArgs e)
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


		private void ShowHierarchy(HierarchyScope scope)
		{
			var element = manager.GetHierarchy(scope);
			if (element != null)
			{
				var xml = element.ToString(SaveOptions.None);
				hierBox.Text = xml;
			}
			else
			{
				hierBox.Text = $"Cannot get hierarchy for {scope}";
			}
		}

		#endregion Hierarchy actions


		private void Update(object sender, EventArgs e)
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
					logger.WriteLine("Error updating page content", exc);
				}
			}
		}
	}
}
