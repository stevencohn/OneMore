//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Collections.Generic;
	using System.Windows.Forms;


	/// <summary>
	/// Route events from any control contained in a given container to the specified
	/// target control. This is a broadcast approach to event bubbling where an event from any
	/// control within a hierarchy of controls is directed to one specific listener control.
	/// </summary>
	internal class EventRouter : IDisposable
	{
		private sealed class Trash
		{
			public Control Control;
			public string EventName;
			public EventHandler Handler;
		}

		private readonly List<Trash> trash;
		private bool disposedValue;


		public EventRouter()
		{
			trash = new List<Trash>();
		}


		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					foreach (var t in trash)
					{
						var type = t.Control.GetType();
						var ev = type.GetEvent(t.EventName);
						ev.RemoveEventHandler(t.Control, t.Handler);
					}

					trash.Clear();
				}

				disposedValue = true;
			}
		}


		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}


		/// <summary>
		/// Registers a named event for all controls recursively owned by the given control
		/// and redirects those events to the given handler.
		/// </summary>
		/// <param name="control">Container control with recursive child controls</param>
		/// <param name="eventName">The name of the event, e.g. "Click" or "MouseClicked"</param>
		/// <param name="handler">The event handler to invoke, can be (s,e)=>{lambda}</param>
		public void Register(Control control, string eventName, EventHandler handler)
		{
			var type = control.GetType();
			var ev = type.GetEvent(eventName);
			ev.AddEventHandler(control, handler);
			trash.Add(new Trash { Control = control, EventName = eventName, Handler = handler });

			foreach (Control child in control.Controls)
			{
				Register(child, eventName, handler);
			}
		}
	}
}
