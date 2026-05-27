//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace OneMoreCli
{
	using River.OneMoreAddIn.Cli;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;


	/// <summary>
	/// Discovers <see cref="ICliCommand"/> implementations in <c>River.OneMoreAddIn.dll</c>
	/// via reflection.
	/// </summary>
	internal static class CommandDiscovery
	{
		/// <summary>
		/// Finds all non-abstract types in <c>River.OneMoreAddIn.dll</c> that implement
		/// <see cref="ICliCommand"/> and have a public default constructor.
		/// Returns them sorted by <see cref="ICliCommand.CommandName"/>.
		/// <para>
		/// Handles <see cref="ReflectionTypeLoadException"/> gracefully — the OneMore assembly
		/// has COM/Office interop dependencies that may not resolve in a standalone CLI context.
		/// Types that fail to load are skipped silently.
		/// </para>
		/// </summary>
		public static List<ICliCommand> Discover()
		{
			var interfaceType = typeof(ICliCommand);
			var assembly = interfaceType.Assembly; // River.OneMoreAddIn.dll

			// GetTypes() throws ReflectionTypeLoadException when any type in the assembly
			// fails to load. The exception's Types array still contains the types that loaded.
			Type[] types;
			try
			{
				types = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException ex)
			{
				types = ex.Types.Where(t => t != null).ToArray();
			}

			var commands = new List<ICliCommand>();
			foreach (var type in types)
			{
				if (type.IsAbstract || type.IsInterface)
					continue;

				if (!interfaceType.IsAssignableFrom(type))
					continue;

				try
				{
					var instance = (ICliCommand)Activator.CreateInstance(type);
					commands.Add(instance);
				}
				catch
				{
					// Skip types whose constructors require arguments or throw
				}
			}

			return commands.OrderBy(c => c.CommandName).ToList();
		}
	}
}
