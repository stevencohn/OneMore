//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using Microsoft.Win32;

	public class OneNoteLink
	{
		public bool IsLocal { get; set; }
		public string NotebookName { get; set; }
		public List<string> SectionGroups { get; set; } = new();
		public string SectionName { get; set; }
		public string PageName { get; set; }
		public Guid SectionId { get; set; }
		public Guid PageId { get; set; }
		public Guid? ObjectId { get; set; }  // null if link is to the page, not a paragraph
		public string LinkTerminator { get; set; }  // e.g. "end", "E", "F3"
		public string RawSectionPath { get; set; }

		/// <summary>True if the .one file was confirmed on disk (local only).</summary>
		public bool SectionFileVerified { get; set; }

		/// <summary>
		/// If the initial parse guessed the wrong notebook root, this records
		/// how the segments were re-interpreted after the file was found.
		/// </summary>
		public string VerificationNote { get; set; }
	}


	/// <summary>
	/// Parses OneNote URI links (as produced by OneNote's "Copy Link to Page" and
	/// "Copy Link to Paragraph" commands) into their constituent parts: notebook name,
	/// section groups, section name, page name, and optional paragraph object ID.
	/// </summary>
	/// <remarks>
	/// <para>
	/// OneNote URIs come in two forms:
	/// </para>
	///
	/// <para><b>Local notebooks:</b></para>
	/// <code>
	/// onenote:///\\COMPUTERNAME\Usersjohn\Documents\Local%20Notebooks\MyNotebook\SectionGroup\Section.one#PageName&amp;section-id={...}&amp;page-id={...}&amp;end
	/// </code>
	/// <para>
	/// The host portion encodes a local path: <c>\\COMPUTERNAME\Usersjohn</c> maps to
	/// <c>C:\Users\john</c> on the local machine. The parser detects this by comparing
	/// the host name to <see cref="Environment.MachineName"/> and reconstructs the true
	/// local file path before attempting to verify or parse segment boundaries.
	/// </para>
	///
	/// <para><b>Remote (OneDrive) notebooks:</b></para>
	/// <code>
	/// onenote:https://d.docs.live.net/{userid}/Documents/MyNotebook/SectionGroup/Section.one#PageName&amp;section-id={...}&amp;page-id={...}&amp;end
	/// </code>
	/// <para>
	/// The notebook name is always the path segment immediately after
	/// <c>/{userid}/Documents/</c>. Any segments between the notebook and
	/// <c>Section.one</c> are section groups.
	/// </para>
	///
	/// <para><b>Paragraph-level links</b> include an additional <c>object-id</c> parameter
	/// and use <c>&amp;E</c> rather than <c>&amp;end</c> as the terminator:</para>
	/// <code>
	/// ...&amp;page-id={...}&amp;object-id={...}&amp;E
	/// </code>
	/// <para>
	/// When present, <see cref="OneNoteLink.ObjectId"/> is populated; otherwise it is
	/// <c>null</c>, indicating a page-level link.
	/// </para>
	///
	/// <para><b>Local file verification:</b> After parsing a local URI, the parser confirms
	/// that the <c>.one</c> file exists on disk. If not found at the decoded path, three
	/// recovery strategies are attempted in order:</para>
	/// <list type="number">
	///   <item>
	///     <b>Common roots scan</b> — the notebook-relative tail (e.g.
	///     <c>MyNotebook\Section.one</c>) is probed under well-known local notebook
	///     folders such as <c>Documents\OneNote Notebooks</c> and
	///     <c>Documents\Local Notebooks</c>.
	///   </item>
	///   <item>
	///     <b>Drive letter probe</b> — for local (non-UNC) paths, the same relative
	///     path is tried on every available fixed drive, handling notebooks that have
	///     been moved to a different drive.
	///   </item>
	/// </list>
	/// <para>
	/// If all strategies fail, <see cref="OneNoteLink.SectionFileVerified"/> is
	/// <c>false</c> and <see cref="OneNoteLink.VerificationNote"/> describes the
	/// outcome. The parsed names are still populated as a best guess.
	/// </para>
	///
	/// <para><b>Usage:</b></para>
	/// <code>
	/// // Basic usage — notebook root inferred automatically
	/// OneNoteLink link = OneNoteLinkParser.Parse(uriString);
	///
	/// // Explicit notebook root — skips heuristic and probes directly
	/// OneNoteLink link = OneNoteLinkParser.Parse(uriString, @"C:\Users\john\Documents\Local Notebooks");
	///
	/// Console.WriteLine(link.NotebookName);           // "MyNotebook"
	/// Console.WriteLine(link.SectionName);            // "Section"
	/// Console.WriteLine(string.Join(" > ", link.SectionGroups)); // "SectionGroup"
	/// Console.WriteLine(link.PageName);               // "PageName"
	/// Console.WriteLine(link.ObjectId.HasValue);      // true if paragraph-level link
	/// Console.WriteLine(link.SectionFileVerified);    // true if .one file found on disk
	/// </code>
	/// </remarks>
	public static class OneNoteLinkParser
	{
		private static readonly Regex UriPattern = new(
			@"^onenote:(?<body>.+\.one)#(?<fragment>.+)$",
			RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static readonly Regex FragmentPattern = new(
			@"^(?<page>[^&]+)&section-id=\{(?<sectionId>[^}]+)\}&page-id=\{(?<pageId>[^}]+)\}(?:&object-id=\{(?<objectId>[^}]+)\})?&(?<terminator>[A-Za-z0-9]+)$",
			RegexOptions.IgnoreCase | RegexOptions.Compiled);


		public static OneNoteLink Parse(string rawUri, string localNotebooksRoot = null)
		{
			if (string.IsNullOrWhiteSpace(rawUri))
				throw new ArgumentException("URI is empty.", nameof(rawUri));

			rawUri = rawUri.Trim();

			var uriMatch = UriPattern.Match(rawUri);
			if (!uriMatch.Success)
				throw new FormatException($"Not a recognized OneNote URI: {rawUri}");

			var body = uriMatch.Groups["body"].Value;
			var fragment = uriMatch.Groups["fragment"].Value;

			var fragMatch = FragmentPattern.Match(fragment);
			if (!fragMatch.Success)
				throw new FormatException($"Could not parse OneNote URI fragment: {fragment}");

			var result = new OneNoteLink
			{
				PageName = Decode(fragMatch.Groups["page"].Value),
				SectionId = Guid.Parse(fragMatch.Groups["sectionId"].Value),
				PageId = Guid.Parse(fragMatch.Groups["pageId"].Value),
				RawSectionPath = body,
			};

			var objectIdGroup = fragMatch.Groups["objectId"];
			result.ObjectId = objectIdGroup.Success
				? Guid.Parse(objectIdGroup.Value)
				: (Guid?)null;

			result.LinkTerminator = fragMatch.Groups["terminator"].Value;

			if (body.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
				body.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
			{
				ParseRemotePath(body, result);
			}
			else
			{
				ParseLocalPath(body, result, localNotebooksRoot);
			}

			return result;
		}


		// ---------------------------------------------------------------------------------------
		// Remote path

		private static void ParseRemotePath(string body, OneNoteLink result)
		{
			result.IsLocal = false;

			// Extract the path portion directly rather than via new Uri(body) to avoid
			// UriFormatException on unusual OneDrive URLs with unencoded spaces or other
			// characters that System.Uri rejects as invalid.
			var schemeEnd = body.IndexOf("//", StringComparison.Ordinal);
			var hostStart = schemeEnd >= 0 ? schemeEnd + 2 : 0;
			var pathStart = body.IndexOf('/', hostStart);
			var segments = pathStart >= 0
				? body.Substring(pathStart + 1).Split('/')
				: Array.Empty<string>();

			if (segments.Length < 3)
				throw new FormatException("Remote OneNote path too short to contain a notebook.");

			// Locate the notebook by finding the "Documents" segment (OneDrive personal:
			// /{userid}/Documents/{notebook}/...; OneDrive for Business may have more prefix
			// segments). Fall back to index 2 when no "Documents" segment is present.
			int notebookIndex = 2;
			for (int i = 0; i < segments.Length - 1; i++)
			{
				if (segments[i].Equals("Documents", StringComparison.OrdinalIgnoreCase))
				{
					notebookIndex = i + 1;
					break;
				}
			}

			if (notebookIndex >= segments.Length)
				throw new FormatException("Remote OneNote path too short to contain a notebook.");

			result.NotebookName = Decode(segments[notebookIndex]);

			for (int i = notebookIndex + 1; i < segments.Length - 1; i++)
			{
				result.SectionGroups.Add(Decode(segments[i]));
			}

			result.SectionName = StripOneExtension(Decode(segments[segments.Length - 1]));
		}


		// ---------------------------------------------------------------------------------------
		// Local path

		private static void ParseLocalPath(string body, OneNoteLink result, string localNotebooksRoot)
		{
			result.IsLocal = true;

			// Decode before replacing separators so that %5C in the URI body
			// becomes a backslash path separator rather than a literal filename character.
			var stripped = Decode(body.TrimStart('/'));
			stripped = stripped.Replace('/', '\\');

			// Reconstruct the full file path: UNC paths need leading \\ restored
			var filePath = stripped;
			if (body.StartsWith("///\\\\"))          // onenote:///\\server\...
			{
				filePath = "\\\\" + stripped.TrimStart('\\');
			}

			var segments = filePath.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

			if (segments.Length < 2)
				throw new FormatException("Local OneNote path is too short.");

			var sectionFile = segments[segments.Length - 1];          // always Section.one
			result.SectionName = StripOneExtension(sectionFile);

			// If the UNC host is this machine, convert the pseudo-UNC to a real local path
			if (segments.Length > 1 &&
				segments[0].Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase))
			{
				filePath = ResolveLocalUncPath(segments);
				segments = filePath.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
			}

			// --- Determine notebook index ---
			int notebookIndex = localNotebooksRoot != null
				? FindNotebookIndexByRoot(segments, filePath, localNotebooksRoot)
				: FindNotebookIndex(segments);

			result.NotebookName = segments[notebookIndex];
			result.SectionGroups.Clear();
			for (int i = notebookIndex + 1; i < segments.Length - 1; i++)
				result.SectionGroups.Add(segments[i]);

			// --- Verify the file exists; adjust if not ---
			VerifyAndAdjust(filePath, segments, notebookIndex, result);
		}


		private static string ResolveLocalUncPath(string[] segments)
		{
			// segments[0] = machine name, segments[1] = "Userssteve" (encodes C:\Users\steve)
			if (segments.Length < 2)
				return string.Join("\\", segments);

			var machineName = segments[0];
			if (!machineName.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase))
				return string.Join("\\", segments);  // genuinely a remote UNC, leave as-is

			// Decode the share name: "Userssteve" → "Users" + username → <ProfilesDir>\steve
			var share = segments[1]; // e.g. "Userssteve"
			const string usersPrefix = "Users";

			string localBase;
			if (share.StartsWith(usersPrefix, StringComparison.OrdinalIgnoreCase))
			{
				var username = share.Substring(usersPrefix.Length); // "steve"
				localBase = Path.Combine(GetProfilesDirectory(), username);
			}
			else
			{
				// Fallback: not a Users share, can't decode — leave as UNC
				return string.Join("\\", segments);
			}

			// Reconstruct: <ProfilesDir>\steve + Documents\Local Notebooks\...
			var remainder = segments.Skip(2).ToArray();
			return remainder.Length > 0
				? Path.Combine(localBase, string.Join("\\", remainder))
				: localBase;
		}


		// ---------------------------------------------------------------------------------------
		// Verification + adjustment

		/// <summary>
		/// Checks that the .one file exists at the parsed path.
		/// If not, tries a set of recovery strategies in order:
		///   1. The path is a UNC path but this machine uses a local mirror — scan
		///      common local notebook roots for the same relative tail.
		///   2. The drive letter changed — probe all local drives.
		/// Updates result in-place when a match is found.
		/// </summary>
		private static void VerifyAndAdjust(
			string filePath, string[] segments, int notebookIndex, OneNoteLink result)
		{
			if (File.Exists(filePath))
			{
				result.SectionFileVerified = true;
				return;
			}

			// Strategy 1: scan well-known local notebook roots for the relative tail
			// (tail = Notebook\[Groups\]Section.one)
			var tail = BuildTail(segments, notebookIndex);
			var commonRoots = GetCommonNotebookRoots();

			foreach (var root in commonRoots)
			{
				var candidate = Path.Combine(root, tail);
				if (File.Exists(candidate))
				{
					ApplyFoundPath(candidate, notebookIndex, result,
						$"UNC path not reachable; found matching tail under '{root}'");
					return;
				}
			}

			// Strategy 2: same relative path but on a different drive letter
			if (segments.Length > 0 && segments[0].Length == 2 && segments[0][1] == ':')
			{
				var relativePart = string.Join("\\", segments.Skip(1));
				foreach (var drive in DriveInfo.GetDrives()
					.Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
					.Select(d => d.Name.TrimEnd('\\')))
				{
					var candidate = drive + "\\" + relativePart;
					if (File.Exists(candidate))
					{
						var newSegments = candidate.Split(new[] { '\\' },
							StringSplitOptions.RemoveEmptyEntries);

						int newNotebookIndex = FindNotebookIndex(newSegments);
						ApplyFoundPath(candidate, newNotebookIndex, result,
							$"Drive letter adjusted from '{segments[0]}' to '{drive}'");
						return;
					}
				}
			}

			// No recovery possible — mark unverified but leave the best-guess parse intact
			result.SectionFileVerified = false;
			result.VerificationNote = "Section file not found; path is best-guess only.";
		}


		/// <summary>
		/// Re-applies notebook/section-group names from a confirmed file path.
		/// </summary>
		private static void ApplyFoundPath(
			string confirmedPath, int notebookIndex, OneNoteLink result, string note)
		{
			result.RawSectionPath = confirmedPath;
			result.SectionFileVerified = true;
			result.VerificationNote = note;

			var segs = confirmedPath.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

			// notebookIndex may now be stale if the path changed shape; re-clamp
			notebookIndex = Math.Max(0, Math.Min(notebookIndex, segs.Length - 2));

			result.NotebookName = segs[notebookIndex];
			result.SectionGroups.Clear();
			for (int i = notebookIndex + 1; i < segs.Length - 1; i++)
			{
				result.SectionGroups.Add(segs[i]);
			}

			result.SectionName = StripOneExtension(segs[segs.Length - 1]);
		}


		/// <summary>
		/// Returns the relative tail starting from the notebook segment.
		/// e.g.  Notebook\SectionGroup\Section.one
		/// </summary>
		private static string BuildTail(string[] segments, int notebookIndex) =>
			string.Join("\\", segments.Skip(notebookIndex));


		/// <summary>
		/// Returns folders that are commonly used as OneNote notebook roots
		/// on the local machine, in priority order.
		/// </summary>
		private static IEnumerable<string> GetCommonNotebookRoots()
		{
			var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			var user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

			// GetFolderPath can return "" on service accounts or minimal environments;
			// Path.Combine("", "x") yields a relative path that Directory.Exists
			// would check against the CWD rather than the intended location.
			var candidates = new List<string>();
			if (!string.IsNullOrEmpty(docs))
			{
				candidates.Add(Path.Combine(docs, "OneNote Notebooks"));
				candidates.Add(Path.Combine(docs, "Local Notebooks"));
				candidates.Add(Path.Combine(docs, "OneNote"));
			}
			if (!string.IsNullOrEmpty(user))
			{
				candidates.Add(Path.Combine(user, "OneNote Notebooks"));
				candidates.Add(Path.Combine(user, "Documents", "OneNote Notebooks"));
			}
			return candidates.Where(Directory.Exists);
		}


		// ---------------------------------------------------------------------------------------
		// Helpers (unchanged)

		private static int FindNotebookIndex(string[] segments)
		{
			for (int i = 0; i < segments.Length - 1; i++)
			{
				var s = segments[i];
				if (s.IndexOf("Notebook", StringComparison.OrdinalIgnoreCase) >= 0 ||
					s.Equals("OneNote", StringComparison.OrdinalIgnoreCase))
				{
					if (i + 1 < segments.Length - 1)
						return i + 1;
				}
			}

			return segments[0].Contains(':')
				? Math.Min(4, segments.Length - 2)   // local drive: skip C:\Users\name\Documents
				: Math.Min(2, segments.Length - 2);  // UNC: skip \\server\share
		}


		private static int FindNotebookIndexByRoot(
			string[] segments, string filePath, string root)
		{
			root = root.TrimEnd('\\');
			if (!filePath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
			{
				return FindNotebookIndex(segments);   // root doesn't match, fall back
			}

			var rootSegCount = root.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Length;
			return Math.Min(rootSegCount, segments.Length - 2);
		}


		private static string StripOneExtension(string name) =>
			name.EndsWith(".one", StringComparison.OrdinalIgnoreCase)
				? name.Substring(0, name.Length - 4)
				: name;


		private static string Decode(string s) => Uri.UnescapeDataString(s);


		private static string GetProfilesDirectory()
		{
			const string subkey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList";
			using var key = Registry.LocalMachine.OpenSubKey(subkey);
			var value = key?.GetValue("ProfilesDirectory") as string;
			if (!string.IsNullOrEmpty(value))
			{
				return Environment.ExpandEnvironmentVariables(value);
			}

			var sysDrive = Environment.GetEnvironmentVariable("SystemDrive") ?? "C:";
			return sysDrive + @"\Users";
		}
	}
}
