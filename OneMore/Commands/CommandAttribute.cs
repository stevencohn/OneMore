//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Windows.Forms;


    /// <summary>
    /// Declares properties of a Command class
    /// </summary>
	[AttributeUsage(AttributeTargets.Method)]
    internal class CommandAttribute: Attribute
    {

        public CommandAttribute(string resID, Keys defaultKeys, string category)
        {
            ResID = resID;
            DefaultKeys = defaultKeys;
            Category = category;
        }


        /// <summary>
        /// Gets the name of the category with which the command is associated
        /// </summary>
        /// <remarks>
        /// This is not a resource ID but rather the exact name, used for building
        /// the markdown for the Wiki keyboard reference page
        /// </remarks>
        public string Category { get; private set; }


        /// <summary>
        /// Gets the default accelerator keys for the command which can be overriden
        /// by user settings
        /// </summary>
        public Keys DefaultKeys { get; private set; }


		/// <summary>
		/// Gets the resource ID specifying the display name of the command
		/// </summary>
		public string ResID { get; private set; }
	}
}
