//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{

	internal interface IAtom
	{

		/// <summary>
		/// Gets the length of the textual value in this atom.
		/// </summary>
		int Length { get; }


		/// <summary>
		/// Determines if the content is empty
		/// </summary>
		/// <returns>True if the content is empty; false otherwise</returns>
		bool Empty();


		/// <summary>
		/// Appends the given string to the content of this atom
		/// </summary>
		/// <param name="s"></param>
		void Append(string s);


		/// <summary>
		/// Extracts the specified substring from the contents of this atom
		/// </summary>
		/// <param name="index">The starting index of the substring</param>
		/// <param name="length">The length of the substring</param>
		/// <returns>The extracted substring</returns>
		string Extract(int index, int length);


		/// <summary>
		/// Replaces the substring specified by the given index and length with
		/// the given replacement string
		/// </summary>
		/// <param name="index">The starting index of the substring to replace</param>
		/// <param name="length">The length of the substring to replace</param>
		/// <param name="replacement">The string to insert</param>
		void Replace(int index, int length, string replacement);
	}
}
