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

        public CommandAttribute(string resID, Keys defaultKeys)
        {
            ResID = resID;
            DefaultKeys = defaultKeys;
        }


        /// <summary>
        /// The resource ID specifying the display name of the command
        /// </summary>
        public string ResID { get; private set; }


        /// <summary>
        /// The default accelerator keys for the command
        /// </summary>
        public Keys DefaultKeys { get; private set; }
    }
}
