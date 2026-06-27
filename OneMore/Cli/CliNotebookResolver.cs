//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Cli
{
	using System;
	using System.Linq;
	using System.Threading.Tasks;


	/// <summary>
	/// Resolves a user-supplied notebook name (which may be a display nickname) to the
	/// canonical internal name stored in the OneNote hierarchy before a CLI command runs.
	/// <para>
	/// OneNote distinguishes between a notebook's <c>name</c> (its internal identifier, used
	/// by all API calls) and its <c>nickname</c> (the display label shown in the OneNote UI).
	/// CLI users typically know only the nickname. This resolver matches the supplied value
	/// against <c>name</c> first; if no match is found it falls back to <c>nickname</c> and
	/// replaces the parameter value with the corresponding <c>name</c> so the rest of the
	/// command pipeline always receives the canonical name.
	/// </para>
	/// </summary>
	public static class CliNotebookResolver
	{
		/// <summary>
		/// If <paramref name="parameters"/> contains a non-empty <c>notebook</c> value that does
		/// not match any notebook by <c>name</c> but does match one by <c>nickname</c>, updates
		/// the parameter in place with the canonical <c>name</c>. No-ops when the parameter is
		/// absent, already matches by name, or no notebook is found under either property.
		/// </summary>
		public static async Task ResolveNotebookName(CliParameterSet parameters)
		{
			if (!parameters.TryGet<string>("notebook", out var input)
				|| string.IsNullOrWhiteSpace(input))
			{
				return;
			}

			await using var one = new OneNote();
			var notebooks = await one.GetNotebooks(OneNote.Scope.Notebooks);

			if (notebooks is null || !notebooks.HasElements)
			{
				return;
			}

			var ns = one.GetNamespace(notebooks);
			var all = notebooks.Elements(ns + "Notebook").ToList();

			var byName = all.FirstOrDefault(n => string.Equals(
				n.Attribute("name")?.Value, input,
				StringComparison.InvariantCultureIgnoreCase));

			if (byName is not null)
			{
				return;
			}

			var byNickname = all.FirstOrDefault(n => string.Equals(
				n.Attribute("nickname")?.Value, input,
				StringComparison.InvariantCultureIgnoreCase));

			if (byNickname is not null)
			{
				var resolvedName = byNickname.Attribute("name")?.Value;
				if (!string.IsNullOrEmpty(resolvedName))
				{
					parameters.Set("notebook", resolvedName);
				}
			}
		}
	}
}
