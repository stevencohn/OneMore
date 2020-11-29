//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
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


		/// <summary>
		/// Initialize the instance with the specified label for the given XML root that describes
		/// the page including its name and ID, and probably associated meta tags
		/// </summary>
		/// <param name="label"></param>
		/// <param name="root"></param>
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


		/// <summary>
		/// Gets or sets whether this node should show or hide its checkbox; set to false for
		/// notebooks, section groups, and sections. Set to true for pages
		/// </summary>
		public bool ShowCheckBox { get; set; }


		/// <summary>
		/// Gets the hierarchy level of this node, based on its XML description.
		/// </summary>
		public HierarchyLevels HierarchyLevel { get; private set; }


		/// <summary>
		/// Gets or sets whether this node should appear hyperlinked (hot) on mouse over; this is
		/// typically enabled for pages to enable navigation.
		/// </summary>
		public bool Hyperlinked { get; set; }


		/// <summary>
		/// Gets a reference to the node's XML
		/// </summary>
		public XElement Root { get; private set; }
	}
}
