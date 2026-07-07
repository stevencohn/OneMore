//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Cli;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class PutPageCommand : Command, ICliCommand
	{
		public PutPageCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		#region CLI Implementation

		public string CommandName => "PutPage";


		public string Description => "Write page XML from a file to a OneNote page";


		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("notebook", "Name of notebook", required: true)
			.AddString("section", "Path of section", required: true)
			.AddString("page", "Name of page", required: false)
			.AddString("infile", "Path to file containing page XML", required: true)
			.AddBoolean("force", "Overwrite an existing page when --page is specified",
				required: false, defaultValue: false);

		#endregion CLI Implementation


		public override async Task Execute(params object[] args)
		{
			var cliParams = args.Length > 0 ? args[0] as CliParameterSet : null;

			string notebookName = null;
			string sectionPath = null;
			string pageName = null;
			string infile = null;
			var force = false;

			if (cliParams != null)
			{
				cliParams.TryGet("notebook", out notebookName);
				cliParams.TryGet("section", out sectionPath);
				cliParams.TryGet("page", out pageName);
				cliParams.TryGet("infile", out infile);
				cliParams.TryGet("force", out force);
			}

			if (!File.Exists(infile))
			{
				CliOutput = $"File not found: {infile}";
				await Task.Yield();
				return;
			}

			XElement root;
			string xml;
			try
			{
				xml = File.ReadAllText(infile);
				root = XElement.Parse(xml);
			}
			catch (Exception exc)
			{
				CliOutput = $"Error parsing XML: {exc.Message}";
				await Task.Yield();
				return;
			}

			// create a temp copy of the XML to validate, so we don't modify the original file
			var safePage = new Page(XElement.Parse(xml));
			safePage.OptimizeForSave(true);

			// validate schema before touching OneNote so errors surface cleanly
			var errors = new List<string>();
			if (!OneNote.ValidateSchema(safePage.Root, errors))
			{
				CliOutput = string.Join(Environment.NewLine, errors);
				await Task.Yield();
				return;
			}

			await using var one = new OneNote();

			if (!string.IsNullOrWhiteSpace(pageName))
			{
				var pageIds = await one.FindPagesByPath(notebookName, sectionPath, pageName);

				if (pageIds.Length == 0)
				{
					// page not found — create it in the named section
					var sectionId = await one.FindSectionIdByPath(notebookName, sectionPath);
					if (string.IsNullOrEmpty(sectionId))
					{
						CliOutput = $"Section not found: {notebookName}/{sectionPath}";
						await Task.Yield();
						return;
					}

					one.CreatePage(sectionId, out var newPageId);
					root.SetAttributeValue("ID", newPageId);
					root.SetAttributeValue("name", pageName);
					await one.Update(new Page(root));
				}
				else if (!force)
				{
					CliOutput = $"Page already exists; use --force to overwrite: {notebookName}/{sectionPath}/{pageName}";
					await Task.Yield();
					return;
				}
				else
				{
					// force overwrite: apply the file XML to the existing page
					root.SetAttributeValue("ID", pageIds[0]);
					await one.Update(new Page(root));
				}
			}
			else
			{
				// no page name — update using the ID already embedded in the XML
				await one.Update(new Page(root));
			}

			CliOutput = string.Empty;
			await Task.Yield();
		}
	}
}
