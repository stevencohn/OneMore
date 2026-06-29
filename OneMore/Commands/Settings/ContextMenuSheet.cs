//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions

namespace River.OneMoreAddIn.Settings
{
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Helpers.Office;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Reflection;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal partial class ContextMenuSheet : SheetBase
	{
		private const string ProofingResID = "ribProofingMenu_Label";
		private const string ColorizeResID = "ribColorizeMenu_Label";
		private const string StylesResID = "ctxStyleGallery_Label";

		#region Private classes
		private sealed class CtxMenu
		{
			public string Name { get; set; }
			public string ResID { get; set; }
			public IEnumerable<CtxMenuItem> Commands { get; set; }
			public override string ToString() => Name;
		}


		private sealed class CtxMenuItem
		{
			public string Name { get; set; }
			public string ResID { get; set; }
			public override string ToString() => Name;
		}


		#endregion Private classes


		// extra left padding used to visually nest a command row under its category
		private const int ChildIndent = 24;


		// cache of discovered command categories/commands; this is derived purely from
		// reflection over AddIn plus resource strings, both fixed for the process lifetime
		// (a UI language change requires an addin restart), so it only needs to be built once
		private static List<CtxMenu> discoveredMenus;

		private readonly MoreCheckList menuList;
		private readonly MoreListView orderList;
		private MoreToolStripButton sortButton;
		private MoreToolStripButton upButton;
		private MoreToolStripButton downButton;


		public ContextMenuSheet(SettingsProvider provider) : base(provider)
		{
			InitializeComponent();

			Name = "ContextMenuSheet";
			Title = Resx.ContextMenuSheet_Title;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introBox"
				});
			}

			menuList = new MoreCheckList
			{
				Dock = DockStyle.Fill,
				HeaderStyle = ColumnHeaderStyle.None,
				MultiSelect = false,
				GetCellStyle = GetRowStyle,
				// a subtle selection fill, rather than the strong system "Highlight" color,
				// keeps each row's checkbox glyph and text clearly legible when clicked
				SelectedBackColorKey = "LinkHighlight",
				SelectedForeColorKey = "ControlText"
			};

			menuList.Columns.Add(string.Empty);
			menuList.SetColumnProportions(1f);
			menuList.CheckChanged += OnMenuCheckChanged;

			// right-side ordered list ─────────────────────────────────────────────────────────

			orderList = new MoreListView
			{
				Dock = DockStyle.Fill,
				HeaderStyle = ColumnHeaderStyle.None,
				MultiSelect = false,
				SelectedBackColorKey = "LinkHighlight",
				SelectedForeColorKey = "ControlText"
			};

			orderList.Columns.Add(string.Empty);
			orderList.SetColumnProportions(1f);
			orderList.SelectedIndexChanged += OnOrderSelectionChanged;

			sortButton = new MoreToolStripButton
			{
				DisplayStyle = ToolStripItemDisplayStyle.Image,
				ImageTransparentColor = Color.Magenta,
				Text = Resx.ribSortButton_Label,
				Image = Resx.m_Sort
			};
			sortButton.Click += OnSortClick;

			upButton = new MoreToolStripButton
			{
				DisplayStyle = ToolStripItemDisplayStyle.Image,
				ImageTransparentColor = Color.Magenta,
				Text = Resx.NavigatorWindow_menuMoveUp,
				Image = Resx.m_MoveUp,
				Enabled = false
			};
			upButton.Click += OnUpClick;

			downButton = new MoreToolStripButton
			{
				DisplayStyle = ToolStripItemDisplayStyle.Image,
				ImageTransparentColor = Color.Magenta,
				Text = Resx.NavigatorWindow_menuMoveDown,
				Image = Resx.m_MoveDown,
				Enabled = false
			};
			downButton.Click += OnDownClick;

			var orderToolbar = new MoreToolStrip();
			orderToolbar.Items.AddRange(new ToolStripItem[] { sortButton, upButton, downButton });
			orderToolbar.Dock = DockStyle.Top;

			// right panel: toolbar pinned to top, list fills the rest
			var orderPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 0, 0, 0) };
			orderPanel.Controls.Add(orderList);
			orderPanel.Controls.Add(orderToolbar);

			// split the contentPanel into two columns
			var splitLayout = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 2,
				RowCount = 1,
				Margin = Padding.Empty,
				Padding = Padding.Empty,
				CellBorderStyle = TableLayoutPanelCellBorderStyle.None
			};
			splitLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55f));
			splitLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));
			splitLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			splitLayout.Controls.Add(menuList, 0, 0);
			splitLayout.Controls.Add(orderPanel, 1, 0);

			contentPanel.Controls.Add(splitLayout);
		}


		protected override void OnLoad(EventArgs e)
		{
			PopulateRows(CollectCommandMenus());
			base.OnLoad(e);
		}


		private void PopulateRows(IEnumerable<CtxMenu> menus)
		{
			var settings = provider.GetCollection(Name);
			var items = settings.Get<XElement>("items");

			menuList.BeginUpdate();
			menuList.Items.Clear();

			foreach (var menu in menus)
			{
				menuList.Items.Add(MakeRow(menu.Name, menu.ResID, false, settings, items));

				if (menu.ResID != ColorizeResID)
				{
					foreach (var command in menu.Commands)
					{
						menuList.Items.Add(MakeRow(command.Name, command.ResID, true, settings, items));
					}
				}
			}

			menuList.EndUpdate();

			// populate orderList from saved items in saved order
			orderList.BeginUpdate();
			orderList.Items.Clear();

			if (items != null)
			{
				foreach (var item in items.Elements())
				{
					var key = item.Value;
					var matchingRow = menuList.Items.Cast<ListViewItem>()
						.FirstOrDefault(r => ((string)r.Tag).Replace("_Label", string.Empty) == key);

					if (matchingRow != null)
					{
						orderList.Items.Add(new ListViewItem(matchingRow.Text) { Tag = matchingRow.Tag });
					}
				}
			}

			// backward compat: styles was stored as a boolean, not in the items list
			if (settings.Get("styles", false))
			{
				var stylesKey = StylesResID.Replace("_Label", string.Empty);
				var alreadyListed = orderList.Items.Cast<ListViewItem>()
					.Any(r => ((string)r.Tag).Replace("_Label", string.Empty) == stylesKey);

				if (!alreadyListed)
				{
					var stylesRow = menuList.Items.Cast<ListViewItem>()
						.FirstOrDefault(r => (string)r.Tag == StylesResID);

					if (stylesRow != null)
					{
						orderList.Items.Add(new ListViewItem(stylesRow.Text) { Tag = stylesRow.Tag });
					}
				}
			}

			orderList.EndUpdate();
		}


		private static ListViewItem MakeRow(
			string name, string resID, bool indented, SettingsCollection settings, XElement items)
		{
			var row = new ListViewItem(name)
			{
				Tag = resID,
				IndentCount = indented ? 1 : 0
			};

			var key = resID.Replace("_Label", string.Empty);
			// support both new format (key in items list) and old format (styles boolean)
			row.Checked = (items != null && items.Elements().Any(e => e.Value == key))
				|| (resID == StylesResID && settings.Get("styles", false));

			return row;
		}


		private void OnMenuCheckChanged(object sender, MoreCheckList.CheckChangedEventArgs e)
		{
			SyncOrderList(e.Item);
		}


		private void SyncOrderList(ListViewItem menuItem)
		{
			var key = ((string)menuItem.Tag).Replace("_Label", string.Empty);

			if (menuItem.Checked)
			{
				var alreadyListed = orderList.Items.Cast<ListViewItem>()
					.Any(r => ((string)r.Tag).Replace("_Label", string.Empty) == key);

				if (!alreadyListed)
				{
					orderList.Items.Add(new ListViewItem(menuItem.Text) { Tag = menuItem.Tag });
				}
			}
			else
			{
				var orderItem = orderList.Items.Cast<ListViewItem>()
					.FirstOrDefault(r => ((string)r.Tag).Replace("_Label", string.Empty) == key);

				if (orderItem != null)
				{
					orderList.Items.Remove(orderItem);
				}
			}
		}


		private void OnOrderSelectionChanged(object sender, EventArgs e)
		{
			if (orderList.SelectedItems.Count == 0)
			{
				upButton.Enabled = false;
				downButton.Enabled = false;
				return;
			}

			var index = orderList.SelectedIndices[0];
			upButton.Enabled = index > 0;
			downButton.Enabled = index < orderList.Items.Count - 1;
		}


		private void OnSortClick(object sender, EventArgs e)
		{
			var sorted = orderList.Items.Cast<ListViewItem>()
				.OrderBy(r => r.Text)
				.ToList();

			orderList.BeginUpdate();
			orderList.Items.Clear();
			foreach (var item in sorted)
			{
				orderList.Items.Add(new ListViewItem(item.Text) { Tag = item.Tag });
			}
			orderList.EndUpdate();
		}


		private void OnUpClick(object sender, EventArgs e)
		{
			if (orderList.SelectedItems.Count == 0)
			{
				return;
			}

			var selected = orderList.SelectedItems[0];
			var index = selected.Index;
			if (index <= 0)
			{
				return;
			}

			var text = selected.Text;
			var tag = selected.Tag;

			orderList.BeginUpdate();
			orderList.Items.RemoveAt(index);
			var moved = new ListViewItem(text) { Tag = tag };
			orderList.Items.Insert(index - 1, moved);
			moved.Selected = true;
			orderList.EndUpdate();

			moved.EnsureVisible();
			upButton.Enabled = index - 1 > 0;
			downButton.Enabled = true;
		}


		private void OnDownClick(object sender, EventArgs e)
		{
			if (orderList.SelectedItems.Count == 0)
			{
				return;
			}

			var selected = orderList.SelectedItems[0];
			var index = selected.Index;
			if (index >= orderList.Items.Count - 1)
			{
				return;
			}

			var text = selected.Text;
			var tag = selected.Tag;

			orderList.BeginUpdate();
			orderList.Items.RemoveAt(index);
			var moved = new ListViewItem(text) { Tag = tag };
			orderList.Items.Insert(index + 1, moved);
			moved.Selected = true;
			orderList.EndUpdate();

			moved.EnsureVisible();
			upButton.Enabled = true;
			downButton.Enabled = index + 1 < orderList.Items.Count - 1;
		}


		private static MoreListView.CellStyle GetRowStyle(ListViewItem item, int column)
		{
			return new MoreListView.CellStyle(indent: item.IndentCount > 0 ? ChildIndent : 0, muted: false);
		}


		private IEnumerable<CtxMenu> CollectCommandMenus()
		{
			if (discoveredMenus == null)
			{
				var atype = typeof(CommandAttribute);

				discoveredMenus = typeof(AddIn).GetMethods()
					.Select(m => new
					{
						Method = m,
						Attribute = (CommandAttribute)m.GetCustomAttribute(atype)
					})
					.Where(o =>
						o.Method.Name.EndsWith("Cmd") &&
						o.Attribute != null &&
						!string.IsNullOrWhiteSpace(o.Attribute.Category)
					)
					.Select(o => new
					{
						o.Attribute.ResID,
						CategoryResID = $"{o.Attribute.Category}_Label",
						Name = Resx.ResourceManager.GetString(o.Attribute.ResID),
						Category = Resx.ResourceManager.GetString($"{o.Attribute.Category}_Label")
					})
					.GroupBy(c => new { Name = c.Category, ResID = c.CategoryResID })
					.Select(g => new CtxMenu
					{
						Name = string.Format(Resx.ContextMenuSheet_menu, g.Key.Name),
						ResID = g.Key.ResID,
						Commands = g.Select(c => new CtxMenuItem { Name = c.Name, ResID = c.ResID })
							.OrderBy(c => c.Name)
							.ToList()
					})
					.OrderBy(m => m.Name)
					.ToList();
			}

			var list = new List<CtxMenu>(discoveredMenus);

			// add Proofing Language menu
			var codes = Office.GetEditingLanguages();
			if (codes != null && codes.Length > 1)
			{
				list.Add(new CtxMenu
				{
					Name = Resx.ResourceManager.GetString(ProofingResID),
					ResID = ProofingResID,
					Commands = new List<CtxMenuItem>()
				});
			}

			// add Styles menu; this is not a discovered command since the Styles
			// gallery is built directly into the context menu rather than cloned
			// from a ribbon button or menu, so it must be injected here artificially

			list.Add(new CtxMenu
			{
				Name = Resx.ResourceManager.GetString(StylesResID),
				ResID = StylesResID,
				Commands = new List<CtxMenuItem>()
			});

			return list.OrderBy(m => m.Name);
		}


		public override bool CollectSettings()
		{
			var settings = provider.GetCollection(Name);
			var existingItems = settings.Get<XElement>("items");

			var newItems = new XElement("items");
			foreach (ListViewItem orderItem in orderList.Items)
			{
				var key = ((string)orderItem.Tag).Replace("_Label", string.Empty);
				newItems.Add(new XElement("item", key));
			}

			var existingKeys = existingItems?.Elements().Select(e => e.Value).ToList()
				?? new List<string>();
			var newKeys = newItems.Elements().Select(e => e.Value).ToList();

			var updated = !existingKeys.SequenceEqual(newKeys);

			// migrate old-style styles boolean into the items list
			if (settings.Contains("styles"))
			{
				settings.Remove("styles");
				updated = true;
			}

			if (updated)
			{
				settings.Add(newItems);

				if (settings.Count > 0)
				{
					provider.SetCollection(settings);
				}
				else
				{
					provider.RemoveCollection(Name);
				}
			}

			return updated;
		}


		public static void UpgradeSettings(SettingsProvider provider)
		{
			/*
			 * TODO: Temporary
			 * Reader-makes-right to convert old button names to new names
			 * Released with 5.1.0
			 */

			var exchange = new Dictionary<string, string>
			{
				{ "ribDrawPlantUmlButton", "ribPlantUmlButton" },
				{ "ribInsertBoxButton", "ribInsertTextBoxButton" },
				{ "ribInsertCodeBlockButton", "ribInsertCodeBoxButton" },
				{ "ribInsertInfoBlockButton", "ribInsertInfoBoxButton" }
			};

			var collection = provider.GetCollection(nameof(ContextMenuSheet));
			var updated = false;

			exchange.ForEach(item =>
			{
				if (collection.Contains(item.Key))
				{
					collection.Remove(item.Key);
					collection.Add(item.Value, true);
					updated = true;
				}
			});

			/*
			 * TODO: Temporary
			 * Reader-makes-right to convert from indvidual elements like
			 * Released with 5.8.3
			 *
			 *		<ribFooButton_Label>true</ribFooButton_Label>
			 *
			 * to a more XML-conforming list of items like
			 *
			 *		<items>
			 *		  <item>ribFooButton_Label</item>
			 *		  <item>ribBarButton_Label</item>
			 *		</items>
			 */

			if (!collection.Contains("items"))
			{
				var items = new XElement("items");
				collection.Keys.ToList().ForEach(key =>
				{
					items.Add(new XElement("item", key));
					collection.Remove(key);
				});

				collection.Add(items);
				updated = true;
			}

			if (updated)
			{
				provider.SetCollection(collection);
				provider.Save();
			}
		}
	}
}
