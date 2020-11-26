//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Runtime.Serialization;
	using System.Windows.Forms;
	using System.Xml.Linq;


	/// <summary>
	/// OneNote hierarchy levels
	/// </summary>
	/// <remarks>
	/// The int values should correspond to the HierarchyView.ImageList indexes
	/// </remarks>
	internal enum HierarchyLevels
	{
		Notebook = 0,
		SectionGroup = 1,
		Section = 2,
		Page
	}


	/// <summary>
	/// A node in the HierarchyTree tree view
	/// </summary>
	internal class HierarchyNode : TreeNode
	{

		#region Standard constructors
		public HierarchyNode()
			: base()
		{
		}


		public HierarchyNode(string text)
			: base(text)
		{
		}


		public HierarchyNode(string text, TreeNode[] children)
			: base(text, children)
		{
		}


		public HierarchyNode(string text, int imageIndex, int selectedImageIndex)
			: base(text, imageIndex, selectedImageIndex)
		{
		}


		public HierarchyNode(string text, int imageIndex, int selectedImageIndex, TreeNode[] children)
			: base(text, imageIndex, selectedImageIndex, children)
		{
		}


		protected HierarchyNode(SerializationInfo serializationInfo, StreamingContext context)
			: base(serializationInfo, context)
		{
		}
		#endregion Standard constructors


		public HierarchyNode(string label, XElement root)
			: this(label)
		{
			switch (root.Name.LocalName)
			{
				case "Notebook": HierarchyLevel = HierarchyLevels.Notebook; break;
				case "SectionGroup": HierarchyLevel = HierarchyLevels.SectionGroup; break;
				case "Section": HierarchyLevel = HierarchyLevels.Section; break;

				default:
					HierarchyLevel = HierarchyLevels.Page;
					ShowCheckBox = true;
					break;
			}

			Root = root;
		}


		public bool ShowCheckBox { get; set; }


		public HierarchyLevels HierarchyLevel { get; private set; }


		public bool Hyperlinked { get; set; }


		public XElement Root { get; private set; }
	}
}
