//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003  // Type is not CLS-compliant

namespace River.OneMoreAddIn.Settings
{
	using Microsoft.Office.Core;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Reflection;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class KeyboardSheet : SheetBase
	{
		#region KeyMap
		private sealed class KeyMap : INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;

			public KeyMap(string methodName, string description, Hotkey hotkey)
			{
				MethodName = methodName;
				Description = description;
				Hotkey = hotkey;
			}

			public string MethodName { get; private set; }
			public string Description { get; private set; }
			public Hotkey Hotkey { get; private set; }

			public void SetKeys(Keys keys, Keys modifiers)
			{
				Hotkey.SetKeys(keys, modifiers);
				PropertyChanged(this, new PropertyChangedEventArgs(nameof(Hotkey)));
			}
		}
		#endregion KeyMap


		private readonly IRibbonUI ribbon;
		private readonly BindingList<KeyMap> map;
		private List<KeyMap> defaultMap;


		public KeyboardSheet(SettingsProvider provider, IRibbonUI ribbon)
			: base(provider)
		{
			InitializeComponent();

			Name = "KeyboardSheet";
			Title = Resx.SnippetsSheet_Text;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introLabel",
					"clearButton",
					"resetButton",
					"resetAllButton"
				});

				cmdColumn.HeaderText = Resx.KeyboardSheet_cmdColumn_HeaderText;
				keyColumn.HeaderText = Resx.KeyboardSheet_keyColumn_HeaderText;
			}

			gridView.AutoGenerateColumns = false;
			gridView.Columns[0].DataPropertyName = "Description";
			gridView.Columns[1].DataPropertyName = "Hotkey";

			this.ribbon = ribbon;

			map = new BindingList<KeyMap>(LoadKeyboardMap());
			gridView.DataSource = map;
		}


		private List<KeyMap> LoadKeyboardMap()
		{
			defaultMap = typeof(AddIn).GetMethods()
				.Select(m => new
				{
					MethodName = m.Name,
					Attr = (CommandAttribute)m.GetCustomAttribute(typeof(CommandAttribute), false)
				})
				.Where(a => a.Attr != null)
				.Select(a => new KeyMap(
					a.MethodName,
					Resx.ResourceManager.GetString(a.Attr.ResID),
					new Hotkey(a.Attr.DefaultKeys)
					))
				.OrderBy(k => k.Description)
				.ToList();

			// create clones to preserve the defaults

			return defaultMap
				.Select(s => new KeyMap(s.MethodName, s.Description, new Hotkey(s.Hotkey)))
				.ToList();
		}


		private void AssignOnKeyDown(object sender, KeyEventArgs e)
		{
			if ( // clear assignment (Back is explicit clear, None is implicit resolve)
				 e.KeyCode == Keys.Back ||
				// any combination of ctrl+shift+alt+win
				(e.Modifiers != 0 &&
				 // ensure modifiers also comes with a value key
				 e.KeyCode != Keys.ControlKey &&
				 e.KeyCode != Keys.LControlKey &&
				 e.KeyCode != Keys.RControlKey &&
				 e.KeyCode != Keys.ShiftKey &&
				 e.KeyCode != Keys.LShiftKey &&
				 e.KeyCode != Keys.RShiftKey &&
				 e.KeyCode != Keys.Menu && // alt
				 e.KeyCode != Keys.LMenu &&
				 e.KeyCode != Keys.RMenu) ||
				// F1..F24
				(e.Modifiers == 0 &&
				 e.KeyCode >= Keys.F1 &&
				 e.KeyCode <= Keys.F24))
			{
				// ignore Shift without a Fn key
				if ((e.Modifiers == Keys.Shift) && (e.KeyCode < Keys.F1 || e.KeyCode > Keys.F24))
				{
					return;
				}

				if (gridView.SelectedCells.Count > 0)
				{
					var index = gridView.SelectedCells[0].RowIndex;
					map[index].SetKeys(e.KeyData & Keys.KeyCode, e.Modifiers);

					ResolveDuplicates(index);
				}

				e.Handled = true;
			}
		}


		private void ResolveDuplicates(int index)
		{
			var hotkey = map[index].Hotkey;

			// Back is explicit clear, None is implicit resolve
			if (hotkey.Keys == Keys.Back)
			{
				return;
			}

			int i = 0;
			while (i < map.Count)
			{
				if (i != index && map[i].Hotkey.Equals(hotkey))
				{
					map[i].SetKeys(Keys.None, Keys.None);
					break;
				}

				i++;
			}

			// reset any blank command to default key if not used elsewhere
			for (i = 0; i < map.Count; i++)
			{
				if (i != index)
				{
					if (map[i].Hotkey.Keys == Keys.None)
					{
						// lookup the default for this command
						var defkey = defaultMap[i].Hotkey;

						// ensure the default isn't used elsewhere
						if (!map.Any(k => k.Hotkey.Equals(defkey)))
						{
							map[i].SetKeys(defkey.Keys, defkey.Modifiers);
						}
					}
				}
			}
		}


		private void ClearCommand(object sender, System.EventArgs e)
		{
			if (gridView.SelectedCells.Count > 0)
			{
				var index = gridView.SelectedCells[0].RowIndex;
				map[index].SetKeys(Keys.Back, Keys.None);
			}
		}


		private void ResetCommand(object sender, System.EventArgs e)
		{
			if (gridView.SelectedCells.Count > 0)
			{
				var index = gridView.SelectedCells[0].RowIndex;
				var defkey = defaultMap[index].Hotkey;
				map[index].SetKeys(defkey.Keys, defkey.Modifiers);
				ResolveDuplicates(index);
			}
		}


		private void ResetAllDefaults(object sender, System.EventArgs e)
		{
			for (int i = 0; i < map.Count; i++)
			{
				map[i].SetKeys(defaultMap[i].Hotkey.Keys, defaultMap[i].Hotkey.Modifiers);
			}
		}


		public override bool CollectSettings()
		{
			// record changes from defaults
			var element = new XElement("commands");
			for (int i = 0; i < map.Count; i++)
			{
				if (!map[i].Hotkey.Equals(defaultMap[i].Hotkey))
				{
					element.Add(new XElement("command",
						new XAttribute("command", map[i].MethodName),
						new XAttribute("keys", map[i].Hotkey.Keys | map[i].Hotkey.Modifiers)
						));
				}
			}

			var updated = true;

			// compare against saved settings
			var settings = provider.GetCollection(Name)?.Get<XElement>("commands");
			if (settings != null)
			{
				updated = true;
			}

			if (updated)
			{
				var collection = provider.GetCollection(Name);
				collection.Add("commands", element);
				provider.SetCollection(collection);

				ribbon.InvalidateControl("ribOneMoreMenu");
			}

			return updated;
		}
	}
}
