//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Cli;
	using System.Xml.Linq;


	internal class GetHierarchyCommand : Command, ICliCommand
	{
		public GetHierarchyCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		#region CLI Implementation

		public string CommandName => "GetHierarchy";


		public string Description => "Get the hierarchy of sections and pages";


		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("notebook", "Name of notebook", required: false)
			.AddString("section", "Path of section", required: false);

		#endregion CLI Implementation


		public override async Task Execute(params object[] args)
		{
			var cliParams = args.Length > 0 ? args[0] as CliParameterSet : null;

			await using var one = new OneNote();

			string notebookName = null;
			string sectionPath = null;

			if (cliParams != null)
			{
				cliParams.TryGet<string>("notebook", out notebookName);
				cliParams.TryGet<string>("section", out sectionPath);
			}

			var hasNotebook = !string.IsNullOrEmpty(notebookName);
			var hasSection = !string.IsNullOrEmpty(sectionPath);

			XElement result;

			if (!hasNotebook)
			{
				// no notebook — return all notebooks with sections, no pages
				result = await one.GetNotebooks(OneNote.Scope.Sections);
				for (int attempt = 1; attempt < 4 && (result == null || !result.HasElements); attempt++)
				{
					await Task.Delay(500 * attempt);
					result = await one.GetNotebooks(OneNote.Scope.Sections);
				}
			}
			else if (!hasSection)
			{
				// notebook only — return that notebook with sections
				result = await FindNotebookByName(one, notebookName);
			}
			else
			{
				// both notebook and section — return the section with pages
				result = await FindSectionByPath(one, notebookName, sectionPath);
			}

			CliOutput = result?.ToString() ?? string.Empty;

			await Task.Yield();
		}


		private static async Task<XElement> GetNotebooksWithRetry(OneNote one)
		{
			var notebooks = await one.GetNotebooks(OneNote.Scope.Notebooks);
			for (int attempt = 1; attempt < 4 && (notebooks == null || !notebooks.HasElements); attempt++)
			{
				await Task.Delay(500 * attempt);
				notebooks = await one.GetNotebooks(OneNote.Scope.Notebooks);
			}

			return notebooks;
		}


		private static async Task<XElement> FindNotebookByName(OneNote one, string name)
		{
			var notebooks = await GetNotebooksWithRetry(one);
			if (notebooks == null) return null;

			var ns = one.GetNamespace(notebooks);
			var notebook = notebooks
				.Elements(ns + "Notebook")
				.FirstOrDefault(n => string.Equals(
					n.Attribute("name")?.Value, name,
					StringComparison.InvariantCultureIgnoreCase));

			if (notebook == null) return null;

			return await one.GetNotebook(notebook.Attribute("ID").Value, OneNote.Scope.Sections);
		}


		private static async Task<XElement> FindSectionByPath(
			OneNote one, string notebookName, string sectionPath)
		{
			var notebooks = await GetNotebooksWithRetry(one);
			if (notebooks == null) return null;

			var ns = one.GetNamespace(notebooks);
			var notebook = notebooks
				.Elements(ns + "Notebook")
				.FirstOrDefault(n => string.Equals(
					n.Attribute("name")?.Value, notebookName,
					StringComparison.InvariantCultureIgnoreCase));

			if (notebook == null) return null;

			var notebookTree = await one.GetNotebook(
				notebook.Attribute("ID").Value, OneNote.Scope.Sections);

			if (notebookTree == null) return null;

			// walk each segment of the path, allowing section groups as intermediate nodes
			var node = notebookTree;
			var parts = sectionPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

			foreach (var part in parts)
			{
				node = node
					.Elements()
					.FirstOrDefault(e =>
						(e.Name.LocalName == "Section" || e.Name.LocalName == "SectionGroup") &&
						string.Equals(
							e.Attribute("name")?.Value, part,
							StringComparison.InvariantCultureIgnoreCase));

				if (node == null) return null;
			}

			var sectionId = node.Attribute("ID")?.Value;
			if (string.IsNullOrEmpty(sectionId)) return null;

			return await one.GetSection(sectionId);
		}
	}
}
