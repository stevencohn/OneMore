//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003  // Type is not CLS-compliant

namespace River.OneMoreAddIn.Settings
{
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Reflection;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class AliasSheet : SheetBase
	{
		private sealed class CommandAlias
		{
			public string MethodName { get; set; }
			public string Command { get; set; }
			public string Alias { get; set; }
		}

		public const string CollectionName = "AliasSheet";
		public const string SettingsName = "aliases";

		private readonly BindingList<CommandAlias> map;


		public AliasSheet(SettingsProvider provider)
			: base(provider)
		{
			InitializeComponent();

			Name = CollectionName;
			Title = Resx.AliasSheet_Text;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introLabel"
				});

				cmdColumn.HeaderText = Resx.word_Command;
				aliasColumn.HeaderText = Resx.word_Alias;
			}

			gridView.AutoGenerateColumns = false;
			gridView.Columns[0].DataPropertyName = "Command";
			gridView.Columns[1].DataPropertyName = "Alias";

			map = new BindingList<CommandAlias>(
				DiscoverCommands().OrderBy(c => c.Command).ToList());

			gridView.DataSource = map;
		}


		private IEnumerable<CommandAlias> DiscoverCommands()
		{
			var settings = provider.GetCollection(Name)?.Get<XElement>(SettingsName);

			// heavily relies on naming convention, suffix must be "Cmd"
			var methods = typeof(AddIn).GetMethods()
				.Where(m =>
					m.Name.EndsWith("Cmd") &&
					m.GetCustomAttribute(typeof(IgnorePaletteAttribute)) == null);

			foreach (var methodName in methods.Select(m => m.Name))
			{
				// remove "Cmd" suffix from method name
				var nam = methodName.Substring(0, methodName.Length - 3);

				// translate to display name
				var name = Resx.ResourceManager.GetString($"rib{nam}Button_Label");
				if (string.IsNullOrEmpty(name))
				{
					name = Resx.ResourceManager.GetString($"om{name}Button_Label");
				}

				if (!string.IsNullOrWhiteSpace(name))
				{
					var setting = settings?.Elements("alias")
						.FirstOrDefault(e => e.Attribute("methodName")?.Value == methodName);

					var alias = setting?.Value;

					yield return new CommandAlias
					{
						MethodName = methodName,
						Command = name,
						Alias = alias
					};
				}
			}
		}


		public override bool CollectSettings()
		{
			// build new settings element from set aliases
			var element = new XElement(SettingsName);
			map.Where(m => !string.IsNullOrWhiteSpace(m.Alias)).ForEach(m =>
			{
				element.Add(new XElement("alias",
					new XAttribute("methodName", m.MethodName),
					m.Alias
					));
			});

			var updated = false;

			// compare against saved settings
			var collection = provider.GetCollection(Name);
			if (collection != null)
			{
				var settings = collection.Get<XElement>(SettingsName);
				if (settings != null)
				{
					if (settings.Elements().Count() != element.Elements().Count())
					{
						updated = true;
					}
					else
					{
						foreach (var candidate in element.Elements("alias"))
						{
							var setting = settings.Elements("alias")
								.FirstOrDefault(e =>
									e.Attribute("methodName")?.Value == candidate.Attribute("methodName").Value);

							if (setting == null || setting.Value != candidate.Value)
							{
								updated = true;
								break;
							}
						}
					}
				}
				else
				{
					updated = element.HasElements;
				}
			}

			if (updated)
			{
				if (element.HasElements)
				{
					collection.Add(SettingsName, element);
					provider.SetCollection(collection);
				}
				else
				{
					provider.RemoveCollection(Name);
				}
			}

			return updated;
		}


		private void ValidateAlias(object sender, DataGridViewCellValidatingEventArgs e)
		{
			var command = (string)gridView.Rows[e.RowIndex].Cells[0].Value;
			var alias = ((string)e.FormattedValue).ToLower();

			gridView.Rows[e.RowIndex].Cells[1].ErrorText = string.Empty;

			if (map.Any(m => m.Alias?.ToLower() == alias && m.Command != command))
			{
				System.Media.SystemSounds.Beep.Play();
				e.Cancel = true;
			}
		}
	}
}
