//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Pipeline
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using System.Threading.Tasks;


	/// <summary>
	/// One stage of the background pipeline, such as scanning for hashtags. A stage knows how
	/// often it wants to run, whether the user has it enabled, and whether it is ready; the
	/// <see cref="PipelineService"/> that hosts it knows nothing else about it.
	/// </summary>
	internal interface IPipelineStage : IDisposable
	{
		/// <summary>
		/// Gets a short name used in log messages.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the delay, in milliseconds, between runs of this stage.
		/// </summary>
		int Interval { get; }

		/// <summary>
		/// Gets whether the stage should run. This is read every cycle, so a stage that the
		/// user turns off stops, and resumes when turned back on, without a restart.
		/// </summary>
		bool IsEnabled { get; }

		/// <summary>
		/// Called once when the pipeline starts, before the first cycle.
		/// </summary>
		void Initialize();

		/// <summary>
		/// Determines whether the stage can run now. A stage that is not ready is skipped for
		/// this cycle and asked again soon, without holding up the other stages.
		/// </summary>
		Task<bool> IsReady(CancellationToken token);

		/// <summary>
		/// Does the stage's work once.
		/// </summary>
		/// <param name="context">Data shared by the stages of the current cycle</param>
		Task Run(PipelineContext context, CancellationToken token);
	}


	/// <summary>
	/// Data handed from one stage to the stages after it within a single cycle. A new context
	/// is created for every cycle, so nothing carries over from one cycle to the next.
	/// </summary>
	internal sealed class PipelineContext
	{
		private readonly Dictionary<Type, object> items = new();


		/// <summary>
		/// Stores a value for later stages, replacing any earlier value of the same type.
		/// </summary>
		public void Set<T>(T value) where T : class
		{
			items[typeof(T)] = value;
		}


		/// <summary>
		/// Gets a value stored by an earlier stage of this cycle.
		/// </summary>
		/// <returns>False if no earlier stage stored one</returns>
		public bool TryGet<T>(out T value) where T : class
		{
			if (items.TryGetValue(typeof(T), out var item))
			{
				value = (T)item;
				return true;
			}

			value = null;
			return false;
		}
	}
}
