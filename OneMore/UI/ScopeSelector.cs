//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Threading.Tasks;
	using System.Windows.Forms;


	public partial class ScopeSelector : UserControl
	{
		[Flags]
		public enum ScopeKind
		{
			Page = 1,
			Section = 2,
			Notebook = 4,
			Notebooks = 8,
			SelectedNotebooks = 16
		}

		private sealed class Book
		{
			public string ID;
			public string Name;

			public override string ToString()
			{
				return Name;
			}
		}


		private ScopeKind scopes;


		public ScopeSelector()
		{
			InitializeComponent();

			Scopes = ScopeKind.Section | ScopeKind.Notebook | ScopeKind.Notebooks;
			sectionButton.Checked = true;

			if (TranslationHelper.NeedsLocalizing())
			{
				TranslationHelper.Localize(this, new string[]
				{
				"pageButton",
				"sectionButton",
				"notebookButton",
				"notebooksButton",
				"selectedButton"
				});
			}
		}


		/// <summary>
		/// Gets the IDs of the selected notebooks
		/// </summary>
		public IEnumerable<string> SelectedNotebooks => GetSelectedNotebooks();


		/// <summary>
		/// Gets the selected scope; if the scope is ScopeKind.SelectedNotebooks then
		/// also get the selected notebooks from the SelectedNotebooks property.
		/// </summary>
		public ScopeKind Scope { get; private set; }



		/// <summary>
		/// Sets the scope choice that will be make available to the user.
		/// </summary>
		[Description("Available scope choices")]
		public ScopeKind Scopes
		{
			get { return scopes; }
			set
			{
				scopes = value;
				EnableScopeButtons();
				if (scopes.HasFlag(ScopeKind.SelectedNotebooks))
				{
					//if (!DesignMode)
					{
						Task.Run(async () => await LoadNotebooks());
					}
				}
			}
		}


		private void EnableScopeButtons()
		{
			var top = pageButton.Top;

			pageButton.Visible = scopes.HasFlag(ScopeKind.Page);
			if (pageButton.Visible)
			{
				pageButton.Top = top;
				top += pageButton.Height + pageButton.Margin.Top + pageButton.Margin.Bottom;
			}

			sectionButton.Visible = scopes.HasFlag(ScopeKind.Section);
			if (sectionButton.Visible)
			{
				sectionButton.Top = top;
				top += sectionButton.Height + sectionButton.Margin.Top + sectionButton.Margin.Bottom;
			}

			notebookButton.Visible = scopes.HasFlag(ScopeKind.Notebook);
			if (notebookButton.Visible)
			{
				notebookButton.Top = top;
				top += notebookButton.Height + notebookButton.Margin.Top + notebookButton.Margin.Bottom;
			}

			notebooksButton.Visible = scopes.HasFlag(ScopeKind.Notebooks);
			if (notebooksButton.Visible)
			{
				notebooksButton.Top = top;
				top += notebooksButton.Height + notebooksButton.Margin.Top + notebooksButton.Margin.Bottom;
			}

			selectedButton.Visible = scopes.HasFlag(ScopeKind.SelectedNotebooks);
			if (selectedButton.Visible)
			{
				selectedButton.Top = top;
				//top += selectedButton.Height + selectedButton.Margin.Top + selectedButton.Margin.Bottom;
			}

			choiceBox.Height = choiceBox.Padding.Top + choiceBox.Padding.Bottom + top;

			if (pageButton.Visible) pageButton.Checked = true;
			else if (sectionButton.Visible) sectionButton.Checked = true;
			else if (notebookButton.Visible) notebookButton.Checked = true;
			else if (notebooksButton.Visible) notebooksButton.Checked = true;
			else selectedButton.Checked = true;
		}


		private async Task LoadNotebooks()
		{
			listBox.Items.Clear();

			using (var one = new OneNote())
			{
				var notebooks = await one.GetNotebooks();
				var ns = one.GetNamespace(notebooks);
				notebooks.Elements(ns + "Notebook").ForEach(n =>
				{
					listBox.Items.Add(new Book
					{
						ID = n.Attribute("ID").Value,
						Name = n.Attribute("name").Value
					});
				});
			}
		}


		private IEnumerable<string> GetSelectedNotebooks()
		{
			foreach (Book book in listBox.CheckedItems)
			{
				yield return book.ID;
			}
		}

		private void ScopeChanged(object sender, EventArgs e)
		{
			if (pageButton.Checked) Scope = ScopeKind.Page;
			else if (sectionButton.Checked) Scope = ScopeKind.Section;
			else if (notebookButton.Checked) Scope = ScopeKind.Notebook;
			else if (notebooksButton.Checked) Scope = ScopeKind.Notebooks;
			else Scope = ScopeKind.SelectedNotebooks;

			selectionPanel.Visible = (Scope == ScopeKind.SelectedNotebooks);
			Height = selectionPanel.Visible
				? choiceBox.Height + selectionPanel.Height
				: choiceBox.Height;
		}
	}
}
