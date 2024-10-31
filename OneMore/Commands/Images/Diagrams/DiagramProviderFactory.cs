//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;


	/// <summary>
	/// Makes a best provider for the requested diagram type.
	/// </summary>
	internal static class DiagramProviderFactory
	{
		public const string MermaidType = "mermaid";
		public const string PlantUmlType = "plantuml";


		/// <summary>
		/// Makes a provider for the specified diagram type
		/// </summary>
		/// <param name="diagramType"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public static IDiagramProvider MakeProvider(string diagramType)
		{
			if (diagramType == PlantUmlType)
			{
				return new PlantUmlDiagramProvider();
			}

			if (diagramType == MermaidType)
			{
				return new KrokiDiagramProvider(diagramType);
			}

			throw new InvalidOperationException($"invalid diagram provider type {diagramType}");
		}
	}
}
