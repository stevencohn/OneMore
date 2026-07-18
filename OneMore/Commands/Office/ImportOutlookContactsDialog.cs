//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Microsoft.Office.Interop.Outlook;
	using River.OneMoreAddIn.Helpers.Office;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// A 4-step wizard (Folders -> Categories -> Contacts -> Options) that lets the user
	/// narrow down which Outlook contacts to import into OneNote and choose import
	/// options.
	/// </summary>
	internal partial class ImportOutlookContactsDialog : UI.MoreForm
	{
		// synthetic category representing contacts with no assigned category at all;
		// always shown first in the category list, ahead of Outlook's real categories
		private static string UncategorizedName => Resx.ImportOutlookContactsDialog_uncategorized;

		// Outlook's 25 preset category colors, by the name left after stripping the
		// "olCategoryColor" prefix (see Outlook.GetCategories). Most match a real .NET
		// KnownColor by name (resolved directly in ResolveCategoryColor via
		// Color.FromName); this table only needs an entry for the ones that don't,
		// e.g. "Steel" (not "SteelBlue") or "Lavendar" (Outlook's own spelling of
		// Lavender). Values are close visual approximations, not exact Outlook swatches.
		private static readonly Dictionary<string, Color> CategoryColorFallbacks =
			new(StringComparer.OrdinalIgnoreCase)
			{
				["Peach"] = Color.FromArgb(0xFF, 0xCC, 0x99),
				["LightTeal"] = Color.FromArgb(0x99, 0xCC, 0xCC),
				["Lavendar"] = Color.FromArgb(0xCC, 0x99, 0xFF),
				["Steel"] = Color.FromArgb(0x8C, 0x9E, 0xB5),
				["WarmGray"] = Color.FromArgb(0xA6, 0x99, 0x88),
				["NavyBlue"] = Color.FromArgb(0x00, 0x00, 0x80),
				["DarkPurple"] = Color.FromArgb(0x4B, 0x00, 0x82),
				["DarkPing"] = Color.FromArgb(0x99, 0x00, 0x4D),
			};

		private readonly List<OutlookFolder> allFolders;
		private readonly List<OutlookCategory> allCategories;
		private readonly List<OutlookContact> allContacts;

		// per-contact folder/category lookups mean a COM round-trip per contact, so they're
		// resolved lazily (see EnsureContactLookupsResolved) rather than in the constructor,
		// keeping the dialog's own first appearance fast
		private Dictionary<OutlookContact, string> contactFolderIds;
		private Dictionary<OutlookContact, string[]> contactCategoryNames;

		private readonly Dictionary<(bool Checked, string ColorName), Image> categoryGlyphCache = new();

		private readonly UI.MoreCheckListPanel folderChecklist;
		private readonly UI.MoreCheckListPanel categoryChecklist;
		private readonly UI.MoreCheckListPanel contactChecklist;
		private readonly UI.MoreMultilineLabel contactIntroLabel;

		private int currentStep = 1;


		/// <summary>
		/// Gets the checklist panel for whichever step is currently showing - the running
		/// contact count is displayed in that panel's own caption area (below its list)
		/// rather than in the footer, so it has the full width to itself.
		/// </summary>
		private UI.MoreCheckListPanel ActiveChecklist => currentStep switch
		{
			1 => folderChecklist,
			2 => categoryChecklist,
			_ => contactChecklist
		};


		/// <summary>
		/// Initializes a new wizard over the given folders, categories, and contacts.
		/// The contacts list is expected to already span every contact in every folder
		/// returned by Outlook.GetContactFolders() - the wizard only filters/selects
		/// among them locally and never re-queries Outlook while stepping through it.
		/// </summary>
		public ImportOutlookContactsDialog(
			List<OutlookFolder> folders, List<OutlookCategory> categories, List<OutlookContact> contacts)
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.ImportOutlookContactsDialog_Text;

				Localize(new[]
				{
					"backButton",
					"cancelButton=word_Cancel",
					"templateGroupBox",
					"personalRadio",
					"businessRadio",
					"bothRadio",
					"sortGroupBox",
					"lastNameRadio",
					"firstNameRadio",
					"companyRadio"
				});
			}

			allFolders = folders;
			allCategories = categories;
			allContacts = contacts;

			Disposed += (s, e) =>
			{
				foreach (var image in categoryGlyphCache.Values)
				{
					image.Dispose();
				}

				// allContacts/allFolders are read throughout this dialog's lifetime and,
				// for the caller's selected contacts, after it closes too (page generation,
				// list report) - this only runs once the dialog itself is being disposed,
				// which is after all of that has completed
				foreach (var contact in allContacts)
				{
					contact.Dispose();
				}

				foreach (var folder in allFolders)
				{
					folder.Dispose();
				}
			};

			(_, folderChecklist) = BuildStep(folderPanel, Resx.ImportOutlookContactsDialog_folderIntro);

			(_, categoryChecklist) = BuildStep(categoryPanel, Resx.ImportOutlookContactsDialog_categoryIntro);

			(contactIntroLabel, contactChecklist) = BuildStep(contactPanel, string.Empty);

			ConfigureFolderList();
			ConfigureCategoryList();
			ConfigureContactList();

			PopulateFolders();

			GoToStep(1);
		}


		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			// let the frame paint before the per-contact resolution blocks this thread
			System.Windows.Forms.Application.DoEvents();

			EnsureContactLookupsResolved();

			// deferred until here (rather than the constructor) because deciding which
			// categories to show requires knowing which are actually used by a contact
			PopulateCategories();

			UpdateSelectionCount();
		}


		/// <summary>
		/// Gets the contacts the user checked on the final step.
		/// </summary>
		public IEnumerable<OutlookContact> SelectedContacts =>
			contactChecklist.List.Items.Cast<ListViewItem>()
				.Where(i => i.Checked)
				.Select(i => (OutlookContact)i.Tag);


		/// <summary>
		/// Gets the template the user chose on the Options step.
		/// </summary>
		public ContactTemplateOption Template =>
			true switch
			{
				_ when personalRadio.Checked => ContactTemplateOption.Personal,
				_ when businessRadio.Checked => ContactTemplateOption.Business,
				_ => ContactTemplateOption.Both
			};


		/// <summary>
		/// Gets the sort field the user chose on the Options step.
		/// </summary>
		public ContactSortByOption SortBy =>
			true switch
			{
				_ when firstNameRadio.Checked => ContactSortByOption.FirstName,
				_ when companyRadio.Checked => ContactSortByOption.Company,
				_ => ContactSortByOption.LastName
			};


		private static (UI.MoreMultilineLabel Intro, UI.MoreCheckListPanel Checklist) BuildStep(
			Panel host, string introText)
		{
			var checklist = new UI.MoreCheckListPanel { Dock = DockStyle.Fill };
			host.Controls.Add(checklist);

			var intro = new UI.MoreMultilineLabel
			{
				Dock = DockStyle.Top,
				Height = 40,
				Padding = new Padding(0, 0, 0, 8),
				Text = introText
			};
			host.Controls.Add(intro);

			return (intro, checklist);
		}


		private void ConfigureFolderList()
		{
			var list = folderChecklist.List;
			list.Columns.Add(string.Empty);
			list.Columns.Add(string.Empty);
			list.SetColumnProportions(0.7f, 0.3f);
			list.SelectedBackColorKey = "LinkHighlight";
			list.SelectedForeColorKey = "ControlText";
			folderChecklist.SelectionChanged += (s, e) => UpdateSelectionCount();
		}


		private void ConfigureCategoryList()
		{
			var list = categoryChecklist.List;
			list.Columns.Add(string.Empty);
			list.Columns.Add(string.Empty);
			list.SetColumnProportions(0.7f, 0.3f);
			list.SelectedBackColorKey = "LinkHighlight";
			list.SelectedForeColorKey = "ControlText";
			categoryChecklist.SelectionChanged += (s, e) => UpdateSelectionCount();

			var baseGlyph = list.GetCellImage;
			list.GetCellImage = (item, column) =>
			{
				if (column != 0)
				{
					return null;
				}

				var colorName = (item.Tag as OutlookCategory)?.ColorName ?? string.Empty;
				var key = (item.Checked, colorName);
				if (!categoryGlyphCache.TryGetValue(key, out var glyph))
				{
					glyph = ComposeSwatchGlyph(baseGlyph?.Invoke(item, column), ResolveCategoryColor(colorName));
					categoryGlyphCache[key] = glyph;
				}

				return glyph;
			};
		}


		private void ConfigureContactList()
		{
			var list = contactChecklist.List;
			list.Columns.Add(string.Empty);
			list.Columns.Add(string.Empty);
			list.SetColumnProportions(0.4f, 0.6f);
			list.SelectedBackColorKey = "LinkHighlight";
			list.SelectedForeColorKey = "ControlText";
			contactChecklist.SelectionChanged += (s, e) => UpdateSelectionCount();
		}


		private void PopulateFolders()
		{
			var list = folderChecklist.List;

			list.BeginUpdate();
			foreach (var folder in allFolders.OrderBy(f => f.Name, NaturalStringComparer.Instance))
			{
				// Folder.Items.Count is a single cheap COM call per folder (backed by
				// Outlook's own item-count metadata); it may include a handful of
				// non-contact items (e.g. distribution lists) but that's an acceptable
				// trade-off to avoid resolving every contact's folder up front just to
				// show this screen.
				var items = folder.Folder.Items;
				var count = items.Count;
				Marshal.ReleaseComObject(items);

				list.Items.Add(new ListViewItem(
					new[] { folder.Name, string.Format(Resx.ImportOutlookContactsDialog_contactsCount, count) })
				{
					Tag = folder,
					Checked = true
				});
			}
			list.EndUpdate();

			UpdatePrimaryButtonState();
		}


		/// <summary>
		/// Resolves each contact's folder id and category names the first time they're
		/// needed - normally right after the dialog is first shown (see OnShown), but
		/// this also guards RecomputeCategoryCounts/RebuildContactList directly in case
		/// either runs before OnShown has had a chance to. Each resolution is a COM
		/// round-trip per contact, so it only ever runs once (cached thereafter).
		/// </summary>
		private void EnsureContactLookupsResolved()
		{
			if (contactFolderIds != null)
			{
				return;
			}

			ActiveChecklist.Caption = Resx.ImportOutlookContactsDialog_calculating;
			ActiveChecklist.Refresh();

			Cursor = Cursors.WaitCursor;
			try
			{
				contactFolderIds = allContacts.ToDictionary(c => c, GetContactFolderId);
				contactCategoryNames = allContacts.ToDictionary(c => c, GetContactCategoryNames);
			}
			finally
			{
				Cursor = Cursors.Default;
			}
		}


		/// <summary>
		/// Populates the category list, skipping any real Outlook category that isn't
		/// actually assigned to any contact - Outlook installations commonly have unused
		/// predefined categories that would otherwise just be noise here. The synthetic
		/// "Uncategorized" row is always shown, first, regardless of that check.
		/// Requires contactCategoryNames, so this can't run until after
		/// EnsureContactLookupsResolved.
		/// </summary>
		private void PopulateCategories()
		{
			var list = categoryChecklist.List;

			list.BeginUpdate();

			// always first, regardless of how allCategories happens to be ordered
			list.Items.Add(new ListViewItem(new[] { UncategorizedName, string.Empty })
			{
				Tag = new OutlookCategory { Name = UncategorizedName, ColorName = "None" },
				Checked = true
			});

			foreach (var category in allCategories.OrderBy(c => c.Name, NaturalStringComparer.Instance))
			{
				var inUse = allContacts.Any(c => MatchesCategory(contactCategoryNames[c], category));
				if (!inUse)
				{
					continue;
				}

				list.Items.Add(new ListViewItem(new[] { category.Name, string.Empty })
				{
					Tag = category,
					Checked = true
				});
			}
			list.EndUpdate();
		}


		/// <summary>
		/// Updates the running contact count shown in the active step's own caption area
		/// (below its list). On step 3 this reflects the actual per-contact checkboxes;
		/// on steps 1-2 (before that list exists) it reflects how many contacts currently
		/// match the folder/category filters chosen so far.
		/// </summary>
		private void UpdateSelectionCount()
		{
			if (contactFolderIds != null)
			{
				string text;

				if (currentStep == 3)
				{
					var items = contactChecklist.List.Items.Cast<ListViewItem>().ToList();
					var checkedCount = items.Count(i => i.Checked);
					text = string.Format(
						Resx.ImportOutlookContactsDialog_contactsSelected, checkedCount, items.Count);
				}
				else
				{
					var checkedFolderIds = new HashSet<string>(folderChecklist.List.Items.Cast<ListViewItem>()
						.Where(i => i.Checked)
						.Select(i => GetFolderEntryId((OutlookFolder)i.Tag)));

					var checkedCategories = currentStep >= 2
						? categoryChecklist.List.Items.Cast<ListViewItem>()
							.Where(i => i.Checked)
							.Select(i => (OutlookCategory)i.Tag)
							.ToList()
						: new List<OutlookCategory>();

					// categories aren't relevant yet while still on step 1 - the count there
					// reflects folders only, regardless of the category list's state
					var count = allContacts.Count(c =>
						checkedFolderIds.Contains(contactFolderIds[c]) &&
						(currentStep < 2 ||
							checkedCategories.Any(cat => MatchesCategory(contactCategoryNames[c], cat))));

					text = string.Format(Resx.ImportOutlookContactsDialog_matchesSelection, count);
				}

				ActiveChecklist.Caption = text;
			}

			UpdatePrimaryButtonState();
		}


		/// <summary>
		/// Recomputes each category row's contextual count against the folders currently
		/// checked on step 1. Called every time step 2 is entered.
		/// </summary>
		private void RecomputeCategoryCounts()
		{
			EnsureContactLookupsResolved();

			var checkedFolderIds = new HashSet<string>(folderChecklist.List.Items.Cast<ListViewItem>()
				.Where(i => i.Checked)
				.Select(i => GetFolderEntryId((OutlookFolder)i.Tag)));

			foreach (ListViewItem item in categoryChecklist.List.Items)
			{
				var category = (OutlookCategory)item.Tag;
				var count = allContacts.Count(c =>
					checkedFolderIds.Contains(contactFolderIds[c]) &&
					MatchesCategory(contactCategoryNames[c], category));

				item.SubItems[1].Text = string.Format(Resx.ImportOutlookContactsDialog_contactsCount, count);
			}

			categoryChecklist.List.Invalidate();
		}


		/// <summary>
		/// Rebuilds the contact list from folders checked on step 1 intersected with
		/// categories checked on step 2. At least one category must be checked to reach
		/// this step (see UpdatePrimaryButtonState), so there's no "none checked" case
		/// to special-case here. Called every time step 3 is entered.
		/// </summary>
		private void RebuildContactList()
		{
			EnsureContactLookupsResolved();

			var checkedFolderIds = new HashSet<string>(folderChecklist.List.Items.Cast<ListViewItem>()
				.Where(i => i.Checked)
				.Select(i => GetFolderEntryId((OutlookFolder)i.Tag)));

			var checkedCategories = categoryChecklist.List.Items.Cast<ListViewItem>()
				.Where(i => i.Checked)
				.Select(i => (OutlookCategory)i.Tag)
				.ToList();

			var filtered = allContacts.Where(c =>
				checkedFolderIds.Contains(contactFolderIds[c]) &&
				checkedCategories.Any(cat => MatchesCategory(contactCategoryNames[c], cat)))
				.OrderBy(GetContactDisplayName, NaturalStringComparer.Instance)
				.ToList();

			var list = contactChecklist.List;
			list.BeginUpdate();
			list.Items.Clear();
			foreach (var contact in filtered)
			{
				list.Items.Add(new ListViewItem(
					new[] { GetContactDisplayName(contact), contact.Email1Address ?? string.Empty })
				{
					Tag = contact,
					Checked = true
				});
			}
			list.EndUpdate();

			contactIntroLabel.Text = string.Format(Resx.ImportOutlookContactsDialog_contactIntro, filtered.Count);
		}


		private void GoToStep(int step)
		{
			currentStep = step;

			folderPanel.Visible = step == 1;
			categoryPanel.Visible = step == 2;
			contactPanel.Visible = step == 3;
			optionsPanel.Visible = step == 4;

			stepIndicator.CurrentStep = step;
			backButton.Enabled = step > 1;
			nextButton.Text = step == 4
				? Resx.ImportOutlookContactsDialog_importButton
				: Resx.ImportOutlookContactsDialog_nextButton;

			if (step == 2)
			{
				RecomputeCategoryCounts();
			}
			else if (step == 3)
			{
				RebuildContactList();
			}

			// step 4 has no checklist to report a running count for
			if (step != 4)
			{
				UpdateSelectionCount();
			}

			UpdatePrimaryButtonState();
		}


		private void UpdatePrimaryButtonState()
		{
			nextButton.Enabled = currentStep switch
			{
				1 => folderChecklist.List.Items.Cast<ListViewItem>().Any(i => i.Checked),
				2 => categoryChecklist.List.Items.Cast<ListViewItem>().Any(i => i.Checked),
				3 => contactChecklist.List.Items.Cast<ListViewItem>().Any(i => i.Checked),
				4 => true,
				_ => true
			};
		}


		private void BackButton_Click(object sender, EventArgs e)
		{
			if (currentStep > 1)
			{
				GoToStep(currentStep - 1);
			}
		}


		private void NextButton_Click(object sender, EventArgs e)
		{
			if (currentStep < 4)
			{
				GoToStep(currentStep + 1);
				return;
			}

			DialogResult = DialogResult.OK;
			Close();
		}


		private static string GetFolderEntryId(OutlookFolder folder) => folder.Folder.EntryID;


		private static string GetContactFolderId(OutlookContact contact)
		{
			if (contact.Contact.Parent is not Folder parent)
			{
				return null;
			}

			var id = parent.EntryID;
			Marshal.ReleaseComObject(parent);
			return id;
		}


		private static string[] GetContactCategoryNames(OutlookContact contact) =>
			(contact.Contact.Categories ?? string.Empty)
				.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(c => c.Trim())
				.ToArray();


		/// <summary>
		/// Tests whether a contact's category names satisfy the given category row - the
		/// synthetic "Uncategorized" row matches contacts with zero assigned categories
		/// rather than being matched by name.
		/// </summary>
		private static bool MatchesCategory(string[] contactCategoryNames, OutlookCategory category) =>
			category.Name == UncategorizedName
				? contactCategoryNames.Length == 0
				: contactCategoryNames.Contains(category.Name, StringComparer.OrdinalIgnoreCase);


		internal static string GetContactDisplayName(OutlookContact contact)
		{
			var name = $"{contact.FirstName} {contact.LastName}".Trim();
			if (!string.IsNullOrWhiteSpace(name))
			{
				return name;
			}

			return !string.IsNullOrWhiteSpace(contact.CompanyName)
				? contact.CompanyName
				: contact.Email1Address ?? string.Empty;
		}


		private static Color ResolveCategoryColor(string colorName)
		{
			if (string.IsNullOrEmpty(colorName) || colorName == "None")
			{
				return UI.ThemeManager.Instance.GetColor("GrayText");
			}

			var named = Color.FromName(colorName);
			if (named.IsKnownColor)
			{
				return named;
			}

			return CategoryColorFallbacks.TryGetValue(colorName, out var color)
				? color
				: UI.ThemeManager.Instance.GetColor("GrayText");
		}


		private static Image ComposeSwatchGlyph(Image checkGlyph, Color swatchColor)
		{
			const int SwatchSize = 10;
			const int Gap = 6;

			var width = (checkGlyph?.Width ?? 0) + Gap + SwatchSize;
			var height = Math.Max(checkGlyph?.Height ?? 0, SwatchSize);

			var bitmap = new Bitmap(width, height);
			using var g = Graphics.FromImage(bitmap);
			g.SmoothingMode = SmoothingMode.AntiAlias;

			var x = 0;
			if (checkGlyph != null)
			{
				g.DrawImage(checkGlyph, 0, (height - checkGlyph.Height) / 2);
				x = checkGlyph.Width + Gap;
			}

			using var brush = new SolidBrush(swatchColor);
			g.FillRoundedRectangle(brush, new Rectangle(x, (height - SwatchSize) / 2, SwatchSize, SwatchSize), 2);

			return bitmap;
		}
	}
}
