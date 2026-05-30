//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Cli;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	#region Wrappers
	internal class DisableSpellCheckCommand : ProofingCommand, ICliPageCommand
	{
		public DisableSpellCheckCommand() : base() { }

		public string CommandName => "DisableSpellCheck";
		public string Description => "Disable spell checking on one or more pages";

		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("notebook", "Name of notebook", required: true)
			.AddString("section", "Path of section", required: false)
			.AddString("page", "Name of page", required: false);

		public override async Task Execute(params object[] args)
		{
			var cliParams = args.Length > 0 ? args[0] as CliParameterSet : null;
			if (cliParams != null)
			{
				await ExecuteOnPage(cliParams, ProofingCommand.NoLang);
				return;
			}

			await base.Execute(ProofingCommand.NoLang);
		}
	}

	internal class EnableSpellCheckCommand : ProofingCommand, ICliPageCommand
	{
		public EnableSpellCheckCommand() : base() { }

		public string CommandName => "EnableSpellCheck";
		public string Description => "Enable spell checking on one or more pages";

		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("notebook", "Name of notebook", required: true)
			.AddString("section", "Path of section", required: false)
			.AddString("page", "Name of page", required: false);

		public override async Task Execute(params object[] args)
		{
			var cliParams = args.Length > 0 ? args[0] as CliParameterSet : null;
			if (cliParams != null)
			{
				await ExecuteOnPage(cliParams, Thread.CurrentThread.CurrentUICulture.Name);
				return;
			}

			await base.Execute(Thread.CurrentThread.CurrentUICulture.Name);
		}
	}
	#endregion Wrappers


	/// <summary>
	/// Enables or disables proofing language of the selected content.
	/// 
	/// Also driven by Edit/Proofing Language menu to set an alternate language; this menu only
	/// appears if you've configured Office with two or more proofing languages, which can be done
	/// in any Office application including OneNote by going to the Options dialog and then the
	/// Languages sheet.
	/// </summary>
	internal class ProofingCommand : Command
	{
		public const string NoLang = "yo";


		public ProofingCommand()
		{
		}


		/*
		 * Note that if the lang is ambiguous for some text that is not the CurrentCulture
		 * codec then OneNote will infer the language and inject a lang attribute in the CDATA
		 */


		public override async Task Execute(params object[] args)
		{
			var cultureName = (string)args[0];

			logger.StartClock();

			await using var one = new OneNote(out var page, out var ns);
			if (page == null)
			{
				logger.WriteTime("spell check cancelled; no page context");
				return;
			}

			var range = new Models.SelectionRange(page);
			range.GetSelection(allowNonEmpty: true);

			var partial = false;
			if (range.Scope == SelectionScope.Range || range.Scope == SelectionScope.Run)
			{
				// update only selected text...

				var selections = page.Root.Elements(ns + "Outline")
					.Descendants(ns + "T")
					.Where(e => e.Attributes("selected").Any(a => a.Value.Equals("all")));

				if (selections.Any())
				{
					selections.ForEach(e =>
					{
						// remove all "lang=" attributes within selected CDATA
						var wrapper = e.GetCData().GetWrapper();
						var atts = wrapper.Descendants().Attributes("lang");
						if (atts.Any())
						{
							atts.Remove();
							e.Value = wrapper.GetInnerXml();
						}

						// set lang on T element
						e.SetAttributeValue("lang", cultureName);
					});

					partial = true;
				}
				else
				{
					logger.WriteLine("selections not found; setting entire page");
				}
			}

			if (!partial)
			{
				ApplyLanguageToPage(page.Root, ns, cultureName);
			}

			await one.Update(page, true);

			var area = partial ? "selection" : "full page";
			var abled = cultureName == NoLang ? "disabled" : "enabled";
			logger.WriteTime($"spell check {abled} for {cultureName} on {area}");
		}


		protected async Task ExecuteOnPage(CliParameterSet cliParams, string cultureName)
		{
			cliParams.TryGet("pageId", out string pageId);
			if (string.IsNullOrWhiteSpace(pageId)) return;

			await using var one = new OneNote();
			var page = await one.GetPage(pageId, OneNote.PageDetail.All);
			var ns = page.Namespace;

			ApplyLanguageToPage(page.Root, ns, cultureName);
			await one.Update(page, true);

			var abled = cultureName == NoLang ? "disabled" : "enabled";
			logger.Verbose($"spell check {abled} on page: {page.Title}");
		}


		private static void ApplyLanguageToPage(XElement root, XNamespace ns, string cultureName)
		{
			root.DescendantsAndSelf()
				.Where(e => e.Name.LocalName != "OCRData")
				.Attributes("lang")
				.Remove();

			root.Descendants(ns + "T").ForEach(e =>
			{
				var wrapper = e.GetCData().GetWrapper();
				var atts = wrapper.Descendants().Attributes("lang");
				if (atts.Any())
				{
					atts.Remove();
					e.Value = wrapper.GetInnerXml();
				}
			});

			root.Add(new XAttribute("lang", cultureName));
			root.Element(ns + "Title")?.Add(new XAttribute("lang", cultureName));
		}
	}
}
