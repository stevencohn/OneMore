//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.FillCellModels
{
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Xml.Linq;


	/// <summary>
	/// Common members of fillers used to fill down or fill across cells in a table
	/// </summary>
	internal abstract class Filler : IFiller
	{
		protected readonly TableCell cell;
		protected readonly XNamespace ns;


		protected Filler(TableCell cell)
		{
			var clone = cell.Root.Clone();
			ns = cell.Root.GetNamespaceOfPrefix(OneNote.Prefix);

			// deselect
			clone.Descendants()
				.Where(e => e.Name.LocalName == "T" && e.Attribute("selected")?.Value == "all")
				.ToList()
				.ForEach((e) => { e.Attribute("selected").Remove(); });

			this.cell = new TableCell(clone);
		}


		public virtual TableCell Cell => cell;


		public virtual FillType Type { get; }


		public abstract string Increment(int increment);


		public abstract int Subtract(IFiller other);
	}
}
