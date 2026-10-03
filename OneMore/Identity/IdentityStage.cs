//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Identity
{
	using River.OneMoreAddIn.Pipeline;
	using System;
	using System.Threading;
	using System.Threading.Tasks;


	/// <summary>
	/// The identity stage of the background pipeline. It runs first, so the stages after it
	/// can ask the <see cref="IdentitySnapshot"/> it publishes what each page is now.
	/// </summary>
	/// <remarks>
	/// It does not depend on any user setting: favorites and layouts need identities even for
	/// someone who has turned hashtags off.
	/// </remarks>
	internal sealed class IdentityStage : Loggable, IPipelineStage
	{
		/// <summary>
		/// The delay between passes, in milliseconds. A pass reads only the hierarchy, a few
		/// tens of milliseconds for the largest notebook measured, so it can run often enough
		/// that a reopened notebook is picked up before anything needs it.
		/// </summary>
		public const int PassInterval = 2 * 60 * 1000;

		private readonly Func<IHierarchySource> sourceFactory;
		private PageIdentityProvider provider;
		private bool disposed;


		public IdentityStage()
			: this(null, null)
		{
		}


		/// <summary>
		/// Initialize a stage with its own provider and source, for testing.
		/// </summary>
		internal IdentityStage(PageIdentityProvider provider, Func<IHierarchySource> sourceFactory)
		{
			this.provider = provider;
			this.sourceFactory = sourceFactory ?? (() => new OneNoteHierarchySource());
		}


		public string Name => "identity";

		public int Interval => PassInterval;

		public bool IsEnabled => true;


		public void Initialize()
		{
			provider ??= new PageIdentityProvider();
		}


		public Task<bool> IsReady(CancellationToken token)
		{
			return Task.FromResult(true);
		}


		public async Task Run(PipelineContext context, CancellationToken token)
		{
			provider ??= new PageIdentityProvider();

			var source = sourceFactory();
			try
			{
				var snapshot = await new IdentityPass(provider, source).Run(token);
				if (snapshot is not null)
				{
					context.Set(snapshot);
				}
			}
			finally
			{
				if (source is IAsyncDisposable disposable)
				{
					await disposable.DisposeAsync();
				}
			}
		}


		public void Dispose()
		{
			if (!disposed)
			{
				provider?.Dispose();
				provider = null;
				disposed = true;
			}
		}
	}
}
