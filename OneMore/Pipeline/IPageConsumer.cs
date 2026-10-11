//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Pipeline
{
	using River.OneMoreAddIn.Identity;
	using River.OneMoreAddIn.Models;
	using System.Threading.Tasks;


	/// <summary>
	/// Something that reads the content of pages found by the identity pass, such as the hashtag
	/// scanner. Reading a page from OneNote is the expensive step, so a <see cref="PageContentPass"/>
	/// loads each page once, however many consumers want it, and hands the same page to all of them.
	/// </summary>
	/// <remarks>
	/// A consumer is driven by a single pass at a time and may keep per-pass and per-notebook state
	/// between the calls below. Calls arrive in this order: <see cref="Begin"/>; for each notebook
	/// <see cref="BeginNotebook"/>, then for each page <see cref="NeedsPage"/> and, if any consumer
	/// wants it, <see cref="Consume"/>, then <see cref="EndNotebook"/>; and finally <see cref="End"/>.
	/// </remarks>
	internal interface IPageConsumer
	{
		/// <summary>
		/// Gets a short name used in log messages.
		/// </summary>
		string Name { get; }


		/// <summary>
		/// Gets the least time, in milliseconds, the pass must wait after reading a page that this
		/// consumer needs, even when the user has turned the throttle off. A consumer that can
		/// cause a long burst of reads, such as a first full index, uses this to keep the OneNote
		/// UI responsive. Zero means no minimum.
		/// </summary>
		int MinimumThrottle { get; }


		/// <summary>
		/// Called once at the start of a pass, before any notebook is visited.
		/// </summary>
		void Begin(IdentitySnapshot snapshot);


		/// <summary>
		/// Called before the pages of a notebook are visited.
		/// </summary>
		/// <returns>False to skip the notebook for this consumer, in which case neither
		/// <see cref="NeedsPage"/> nor <see cref="EndNotebook"/> is called for it</returns>
		bool BeginNotebook(IdentityNotebook notebook);


		/// <summary>
		/// Determines whether the page must be read for this consumer. This is the cheap decision
		/// made from the hierarchy alone, so it must not load anything.
		/// </summary>
		bool NeedsPage(IdentityPage page);


		/// <summary>
		/// Called with a page that was loaded because at least one consumer needs it. May be called
		/// for a page this consumer did not ask for only if it also asked for it; consumers are never
		/// given a page they did not need.
		/// </summary>
		/// <param name="one">The open OneNote, for a consumer that must write to the page</param>
		/// <param name="page">The loaded page, shared by all consumers of the pass</param>
		/// <param name="identity">The page as found in the hierarchy, with its page key</param>
		/// <returns>True if the consumer recorded or removed anything for the page</returns>
		Task<bool> Consume(OneNote one, Page page, IdentityPage identity);


		/// <summary>
		/// Called after the pages of a notebook that was accepted by <see cref="BeginNotebook"/>,
		/// including a notebook whose hierarchy could not be listed, in which case it is called
		/// with no dirty pages.
		/// </summary>
		/// <param name="dirtyPages">The number of pages for which <see cref="Consume"/> returned true</param>
		void EndNotebook(IdentityNotebook notebook, int dirtyPages);


		/// <summary>
		/// Called once at the end of a pass, whether or not it ran to completion.
		/// </summary>
		/// <param name="cancelled">True if the pass was cut short, in which case pages were missed
		/// and the consumer should not record the pass as complete</param>
		void End(IdentitySnapshot snapshot, bool cancelled);
	}
}
