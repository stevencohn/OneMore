//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Settings;
	using System;
	using System.IO;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using WindowsInput;
	using WindowsInput.Native;


	/// <summary>
	/// Converts the current line to native OneNote content if it contains recognizable
	/// markdown syntax, triggered by pressing Enter at the end of the line.
	/// </summary>
	internal class ConvertLineOnEnterCommand : Command
	{
		// cheap pre-check so ordinary typed lines are left untouched
		private static readonly Regex TokenPattern = new(
			@"^\s{0,3}(#{1,6}\s|>\s?|[-*+]\s|\d+[.)]\s|([-*_])\2{2,}\s*$)" +
			@"|\*\*[^*]+\*\*|__[^_]+__" +
			@"|(?<!\*)\*[^*\s][^*]*\*(?!\*)" +
			@"|(?<!_)_[^_\s][^_]*_(?!_)" +
			@"|`[^`\n]+`" +
			@"|\[[^\]]+\]\([^)\s]+\)",
			RegexOptions.Compiled);

		// OneNote's HTML importer does not reliably turn an isolated "<ul><li>...</li></ul>"
		// fragment into a native bulleted paragraph (confirmed: it silently drops the list
		// markup, leaving plain text). So bullet items are detected here and built directly
		// as a native <one:List><one:Bullet/></one:List> OE (see Models.Bullet and the
		// OneNote page schema, 0336.OneNoteApplication_2013.xsd) instead of relying on
		// HTML import. Numbered lists aren't handled this way (yet): a <one:Number/>
		// requires a shared numberSequence across sibling items, which a single isolated
		// line doesn't have enough context to assign correctly.
		private static readonly Regex BulletItemPattern = new(
			@"^\s{0,3}[-*+]\s+(.+)$", RegexOptions.Compiled);

		private static readonly Regex ListItemHtmlPattern = new(
			@"<li>(.*?)</li>", RegexOptions.Compiled | RegexOptions.Singleline);

		// Extracts the inline content Markdig wraps in <p>...</p> for what it renders
		// as a plain paragraph, e.g. when preserving an existing list item's own
		// List/Bullet element and just replacing its text (see existingList below).
		private static readonly Regex ParagraphHtmlPattern = new(
			@"<p>(.*?)</p>", RegexOptions.Compiled | RegexOptions.Singleline);


		public ConvertLineOnEnterCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			// TEMPORARY diagnostics (Phase 3): capture what actually has keyboard focus
			// on every firing, before either bail-out check below runs, so we can see
			// why Enter is still disrupted on OneMore dialogs, ribbon controls (e.g.
			// font family/size), and OneNote's own built-in dialogs (e.g. Edit
			// Hyperlink). Remove once the real fix lands.
			LogFocusDiagnostics("entry");

			// Cheap, COM-free check first: if a OneMore dialog/popup (Navigator, Search,
			// Command Palette, hashtag autocomplete, ...) currently has focus, it runs in
			// this same add-in process, so replay Enter in place right here and never
			// touch OneNote's COM object model at all - HotkeyManager only fires this
			// hotkey while OneNote's own window or one of these popups is foreground, so
			// ruling out "one of our own popups" here means whatever's left must be
			// OneNote itself.
			using (var self = System.Diagnostics.Process.GetCurrentProcess())
			{
				Native.GetWindowThreadProcessId(Native.GetForegroundWindow(), out var foregroundPid);
				if (foregroundPid == (uint)self.Id)
				{
					await ReplayEnter();
					return;
				}
			}

			using var one = new OneNote(out var page, out var ns);
			if (!page.IsValid)
			{
				await ReplayEnter();
				return;
			}

			var range = new SelectionRange(page);
			var cursor = range.GetSelection(true);

			if (range.Scope != SelectionScope.TextCursor || cursor is null)
			{
				// a real selection, or a caret somewhere this isn't modeled (Title,
				// table cell, the Navigation pane, Search, the ribbon, ...) - leave
				// Enter alone
				await ReplayEnter();
				return;
			}

			// capture the paragraph before Deselect() merges/removes the empty CDATA[]
			// cursor marker run out from under it
			var paragraph = cursor.Parent;
			range.Deselect();

			var text = GetRawText(paragraph, ns);

			var matched = !string.IsNullOrWhiteSpace(text) && TokenPattern.IsMatch(text);
			logger.WriteLine($"ConvertLineOnEnter: text=\"{text}\" matched={matched}");

			if (!matched)
			{
				await ReplayEnter();
				return;
			}

			var markdownSettings = new SettingsProvider().GetCollection(nameof(MarkdownSheet));
			var gfmLineBreaks = markdownSettings.Get("gfmLineBreaks", false);
			var singleSpacing = markdownSettings.Get("singleSpacing", false);
			var blankBeforeHeadings = markdownSettings.Get("blankBeforeHeadings", false);

			// cache all OE objectIDs, compare against later, to identify the new content
			var paragraphIDs = page.Root.Descendants(ns + "OE")
				.Select(e => e.Attribute("objectID").Value).ToList();

			var filepath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

			var body = OneMoreDig.ConvertMarkdownToHtml(
				filepath, text, gfmLineBreaks, singleSpacing, blankBeforeHeadings);

			logger.WriteLine($"ConvertLineOnEnter: html={body}");

			var existingList = paragraph.Element(ns + "List");
			var bulletMatch = BulletItemPattern.Match(text);
			var itemMatch = bulletMatch.Success ? ListItemHtmlPattern.Match(body) : Match.Empty;

			string preservedId = null;

			if (existingList != null && !itemMatch.Success)
			{
				// Already a native bullet/numbered list item (a <one:List> sibling of
				// the T runs, unrelated to any literal "-"/"*" text) and the line
				// didn't also introduce a brand new marker. Keep the paragraph and its
				// existing List (whatever Bullet/Number formatting it carries) exactly
				// as-is, and just swap in the newly-converted inline text - going
				// through the generic HTMLBlock-import path below would replace the
				// whole OE, discarding its List element and losing the bullet/number
				// entirely.
				var paragraphMatch = ParagraphHtmlPattern.Match(body);
				var inner = paragraphMatch.Success ? paragraphMatch.Groups[1].Value : body;

				paragraph.Elements(ns + "T").Remove();
				paragraph.Add(new XElement(ns + "T", new XCData(inner)));

				// objectID is stable across Update() for an OE we mutate in place
				// (rather than remove/recreate), so it can be found again below after
				// the page is re-fetched, letting Pass 2 still run against it (e.g. if
				// the new text also introduced inline code needing RewriteInlineCode)
				preservedId = paragraph.Attribute("objectID")?.Value;
			}
			else if (itemMatch.Success)
			{
				// build the native bullet OE directly rather than relying on HTML import
				InsertBulletItem(ns, paragraph, new Bullet(ns, itemMatch.Groups[1].Value));
			}
			else
			{
				var replacement = new XElement(ns + "HTMLBlock",
					new XElement(ns + "Data",
						new XCData($"<html><body>{body}</body></html>")
						));

				paragraph.AddAfterSelf(replacement);
				paragraph.Remove();
			}

			await one.Update(page);

			// Pass 2, cleanup...

			page = await one.GetPage(page.PageId, OneNote.PageDetail.Basic);

			var touched = page.Root.Descendants(ns + "OE")
				.Where(e => !paragraphIDs.Contains(e.Attribute("objectID").Value))
				.ToList();

			if (preservedId != null)
			{
				var preserved = page.Root.Descendants(ns + "OE")
					.FirstOrDefault(e => e.Attribute("objectID")?.Value == preservedId);

				if (preserved != null)
				{
					touched.Add(preserved);
				}
			}

			foreach (var t in touched)
			{
				logger.WriteLine("ConvertLineOnEnter: touched", t);
			}

			if (touched.Any())
			{
				var converter = new MarkdownConverter(page);

				converter
					.RewriteHeadings(touched, blankBeforeHeadings)
					.RewriteBlankLines(touched)
					.RewriteTodo(touched)
					.RewriteCode(touched)
					.RewriteInlineCode(touched)
					.SpaceOutParagraphs(touched, singleSpacing ? 0f : 12f);

				// Place the cursor on a new empty line right after the converted
				// content, exactly where a normal Enter keypress would have landed.
				// Without this, OneNote parks the cursor at the start of the newly
				// imported HTML block, so replaying Enter (see ReplayEnter below)
				// would split there instead - inserting a blank line *before* the
				// converted text and leaving the cursor at the front of it.
				touched.Last().AddAfterSelf(new XElement(ns + "OE",
					new XElement(ns + "T",
						new XAttribute("selected", "all"),
						new XCData(string.Empty)
						)));

				// force a full update: OptimizeForSave's omHash-based "unchanged, skip
				// it" shortcut must not be allowed to discard these targeted edits
				await one.Update(page, force: true);
				return;
			}

			await ReplayEnter();
		}


		// Concatenates each run's raw CDATA content, preserving any inline HTML that
		// already represents existing formatting (bold, italic, color, links, ...).
		// Markdig passes through raw inline HTML untouched while still parsing any
		// surrounding literal markdown syntax, so this lets existing styling on
		// untouched words survive the round trip. PageReader.ReadTextFrom, used
		// elsewhere for whole-page conversion, strips all of that down to bare text
		// instead - fine there since it's re-deriving a page from scratch, but not
		// here where most of the line is usually unrelated to the new markdown.
		private static string GetRawText(XElement paragraph, XNamespace ns)
		{
			var builder = new StringBuilder();
			foreach (var run in paragraph.Elements(ns + "T"))
			{
				var cdata = run.GetCData();
				builder.Append(cdata?.Value ?? run.Value);
			}

			// OneNote sometimes wraps a styled run's CDATA across lines, e.g.
			// "<span\nstyle='...'>text</span>" - meaningless whitespace to an HTML
			// parser, but a single OneNote paragraph is conceptually one markdown
			// line, so an embedded newline here reads to Markdig as ending that line.
			// Inside a list item specifically, that prematurely terminates the item
			// mid-tag, splitting "<span" from "style='...'>" into two separate blocks
			// that no longer parse as one HTML tag, so each half gets escaped as
			// literal text instead of surviving as raw HTML. Collapse to a single
			// space so a wrapped tag stays a valid single-line tag.
			return Regex.Replace(builder.ToString(), @"\s*[\r\n]+\s*", " ");
		}


		// Starting a brand new list (paragraph is a top-level OE and the paragraph
		// before it is plain, non-list content) indents the list one level under that
		// previous paragraph, matching how OneNote/Word autoformat behaves. Continuing
		// an existing list (previous sibling is already a list item), or converting a
		// paragraph that's already indented/nested for any reason, leaves it at its
		// current depth - it only ever becomes a new sibling there, never nested deeper.
		private static void InsertBulletItem(XNamespace ns, XElement paragraph, XElement bulletOE)
		{
			var isTopLevel = paragraph.Parent?.Parent?.Name.LocalName == "Outline";
			var previous = paragraph.PreviousNode as XElement;
			var continuesList = previous is not null && previous.Name.LocalName == "OE" &&
				previous.Element(ns + "List") is not null;

			if (isTopLevel && previous is not null && previous.Name.LocalName == "OE" && !continuesList)
			{
				var children = previous.Element(ns + "OEChildren");
				if (children is null)
				{
					children = new XElement(ns + "OEChildren");
					previous.Add(children);
				}

				children.Add(bulletOE);
				paragraph.Remove();
			}
			else
			{
				paragraph.AddAfterSelf(bulletOE);
				paragraph.Remove();
			}
		}


		// TEMPORARY diagnostics (Phase 3): logs the foreground process and whatever
		// currently holds keyboard focus (via GetGUIThreadInfo, the same approach
		// CaretLocator.LocateCaretViaGuiThread uses), so real focus signals can be
		// captured for OneMore dialogs, ribbon controls, and OneNote's own dialogs.
		// Remove once the real fix (Phase 3 follow-up) lands.
		private static void LogFocusDiagnostics(string label)
		{
			var foreground = Native.GetForegroundWindow();
			var threadId = Native.GetWindowThreadProcessId(foreground, out var foregroundPid);

			var info = new Native.GUITHREADINFO { cbSize = Marshal.SizeOf<Native.GUITHREADINFO>() };
			Native.GetGUIThreadInfo(threadId, ref info);

			var focusClass = new StringBuilder(256);
			var focusText = new StringBuilder(256);
			if (info.hwndFocus != IntPtr.Zero)
			{
				Native.GetClassName(info.hwndFocus, focusClass, focusClass.Capacity);
				Native.GetWindowText(info.hwndFocus, focusText, focusText.Capacity);
			}

			Logger.Current.WriteLine(
				$"ConvertLineOnEnter[{label}]: {DateTime.Now:HH:mm:ss.fff} " +
				$"foregroundPid={foregroundPid} hwndFocus=0x{info.hwndFocus.ToInt64():X} " +
				$"class=\"{focusClass}\" text=\"{focusText}\"");
		}


		// Forward the real Enter keystroke, which HotkeyManager's RegisterHotKey
		// swallowed before OneNote ever saw it. Suspend/Resume brackets the replay so
		// this synthetic keystroke - which travels through the same OS input pipeline
		// as a physical one - isn't caught by our own (or any other) registered hotkey,
		// which would otherwise re-trigger this command forever. Always replays "in
		// place" - i.e. to whatever currently has focus - rather than forcing OneNote's
		// window to the foreground first: this command never takes focus away from
		// anything, so there's never a legitimate target to reclaim it for. An earlier
		// version passed OneNote's window handle here for the SelectionScope bail-out
		// below, which actively stole focus away from a focused ribbon control or one
		// of OneNote's own dialogs (e.g. Edit Hyperlink) right before the replay,
		// sending the keystroke to the wrong place entirely.
		private static async Task ReplayEnter()
		{
			// TEMPORARY diagnostics (Phase 3): measure how long the suspend/replay/
			// resume round trip actually takes, to judge whether its latency is
			// contributing to the "Enter feels disrupted" reports. Remove with the
			// rest of the Phase 3 diagnostics once the real fix lands.
			var stopwatch = System.Diagnostics.Stopwatch.StartNew();

			HotkeyManager.Suspend();
			try
			{
				new InputSimulator().Keyboard.KeyPress(VirtualKeyCode.RETURN);

				// Give the target a moment to actually consume the keystroke before
				// hotkeys are re-armed below. It's Suspend() above - not this delay -
				// that prevents the replayed keystroke from re-triggering this hotkey;
				// this only needs to outlast actual OS keystroke delivery, which is
				// low single-digit milliseconds, so 100ms here was needlessly adding
				// to the latency of every bailed-out Enter (measured 103-126ms total,
				// perceptible enough to feel like Enter itself was broken in the
				// ribbon, OneNote's own dialogs, and OneMore's own dialogs).
				await Task.Delay(20);
			}
			finally
			{
				HotkeyManager.Resume();
				Logger.Current.WriteLine(
					$"ConvertLineOnEnter[replay]: took {stopwatch.ElapsedMilliseconds}ms");
			}
		}
	}
}
