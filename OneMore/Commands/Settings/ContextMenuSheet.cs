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
	using System.Drawing.Drawing2D;
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


		// pixel sizes for the owner-drawn checkbox glyph and the extra left padding used
		// to visually nest a command row under its category
		private const int GlyphSize = 14;
		private const int ChildIndent = 24;


		// cache of discovered command categories/commands; this is derived purely from
		// reflection over AddIn plus resource strings, both fixed for the process lifetime
		// (a UI language change requires an addin restart), so it only needs to be built once
		private static List<CtxMenu> discoveredMenus;

		private readonly MoreListView menuList;
		private Image checkedGlyph;
		private Image uncheckedGlyph;


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

			menuList = new MoreListView
			{
				Dock = DockStyle.Fill,
				HeaderStyle = ColumnHeaderStyle.None,
				MultiSelect = false,
				GetCellStyle = GetRowStyle,
				GetCellImage = GetRowGlyph,
				// a subtle selection fill, rather than the strong system "Highlight" color,
				// keeps each row's checkbox glyph and text clearly legible when clicked
				SelectedBackColorKey = "LinkHighlight",
				SelectedForeColorKey = "ControlText"
			};

			menuList.Columns.Add(string.Empty);
			menuList.SetColumnProportions(1f);
			menuList.MouseClick += OnRowClick;
			menuList.KeyDown += OnRowKeyDown;

			contentPanel.Controls.Add(menuList);

			Disposed += (s, e) =>
			{
				checkedGlyph?.Dispose();
				uncheckedGlyph?.Dispose();
			};
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
		}


		private static ListViewItem MakeRow(
			string name, string resID, bool indented, SettingsCollection settings, XElement items)
		{
			var row = new ListViewItem(name)
			{
				Tag = resID,
				IndentCount = indented ? 1 : 0
			};

			if (resID == StylesResID)
			{
				row.Checked = settings.Get("styles", false);
			}
			else
			{
				var key = resID.Replace("_Label", string.Empty);
				row.Checked = items != null && items.Elements().Any(e => e.Value == key);
			}

			return row;
		}


		private void OnRowClick(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left)
			{
				return;
			}

			var hit = menuList.HitTest(e.Location);
			if (hit.Item != null)
			{
				hit.Item.Checked = !hit.Item.Checked;
				menuList.Invalidate(hit.Item.Bounds);
			}
		}


		private void OnRowKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Space && menuList.SelectedItems.Count > 0)
			{
				var item = menuList.SelectedItems[0];
				item.Checked = !item.Checked;
				menuList.Invalidate(item.Bounds);
				e.Handled = true;
			}
		}


		private static MoreListView.CellStyle GetRowStyle(ListViewItem item, int column)
		{
			return new MoreListView.CellStyle(indent: item.IndentCount > 0 ? ChildIndent : 0, muted: false);
		}


		private Image GetRowGlyph(ListViewItem item, int column)
		{
			return item.Checked
				? checkedGlyph ??= BuildGlyph(true)
				: uncheckedGlyph ??= BuildGlyph(false);
		}


		private Image BuildGlyph(bool isChecked)
		{
			var bitmap = new Bitmap(GlyphSize, GlyphSize);
			using var g = Graphics.FromImage(bitmap);
			g.SmoothingMode = SmoothingMode.AntiAlias;

			var boxColor = manager.GetColor("Highlight");
			using var pen = new Pen(boxColor);
			g.DrawRoundedRectangle(pen, new Rectangle(0, 0, GlyphSize - 1, GlyphSize - 1), 2);

			if (isChecked)
			{
				using var fillBrush = new SolidBrush(boxColor);
				g.FillRoundedRectangle(fillBrush, new Rectangle(2, 2, GlyphSize - 4, GlyphSize - 4), 2);
			}

			return bitmap;
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
			var updated = false;

			var settings = provider.GetCollection(Name);
			var items = settings.Get<XElement>("items");
			items ??= new XElement("items");

			foreach (ListViewItem control in menuList.Items)
			{
				var tag = (string)control.Tag;

				if (tag == StylesResID)
				{
					updated = control.Checked
						? settings.Add("styles", true) || updated
						: settings.Remove("styles") || updated;

					continue;
				}

				var key = tag.Replace("_Label", string.Empty);

				if (control.Checked)
				{
					if (!items.Elements().Any(e => e.Value == key))
					{
						items.Add(new XElement("item", key));
						updated = true;
					}
				}
				else
				{
					var item = items.Elements().FirstOrDefault(e => e.Value == key);
					if (item != null)
					{
						item.Remove();
						updated = true;
					}
				}
			}

			if (updated)
			{
				settings.Add(items);

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
