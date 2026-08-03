//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{

	/// <summary>
	/// A single attachment (image, document, audio, etc.) extracted from an Evernote
	/// &lt;resource&gt; element, keyed by the MD5 hash used by matching &lt;en-media&gt;
	/// references in the note content.
	/// </summary>
	internal class EvernoteResource
	{

		/// <summary>
		/// The lowercase hex MD5 hash of Data, matching the hash attribute of an
		/// en-media element that references this resource.
		/// </summary>
		public string Hash { get; set; }

		/// <summary>
		/// The MIME type reported by the resource, e.g. "image/png" or "application/pdf".
		/// </summary>
		public string Mime { get; set; }

		/// <summary>
		/// The original file name if the resource carried one; may be empty.
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// The raw decoded resource bytes.
		/// </summary>
		public byte[] Data { get; set; }

		/// <summary>
		/// True if the Mime type indicates this resource is an image, which can be
		/// embedded directly as page content rather than only linked.
		/// </summary>
		public bool IsImage => Mime != null && Mime.StartsWith("image/");
	}
}
