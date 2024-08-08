//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S3011 // Reflection should not be used to increase accessibility

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Windows.Forms;


	/// <summary>
	/// Implement event bubbling within a control and all of its descendants, routing events
	/// from child to parent.
	/// </summary>
	internal class EventRouter : Loggable, IDisposable
	{
		private sealed class Trash
		{
			public Control Control;
			public string EventName;
			public Delegate Handler;
		}

		private readonly List<Trash> trash;
		private bool disposedValue;

		public EventRouter()
		{
			trash = new List<Trash>();
		}

		#region Dispose
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					foreach (var item in trash)
					{
						var type = item.Control.GetType();
						var vent = type.GetEvent(item.EventName);
						vent.RemoveEventHandler(item.Control, item.Handler);
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
		#endregion Dispose


		/// <summary>
		/// Route events for all descendants of the given container control, from child to parent,
		/// bubbling up to the container control.
		/// </summary>
		/// <param name="control">Control containing hierarchy of child controls</param>
		/// <param name="eventName">
		/// The name of the event, e.g. Click or MouseClicked.
		/// The named event must have a corresponding "On" method, e.g. OnClick or OnMouseClicked.
		/// </param>
		public void Register(Control control, string eventName)
		{
			foreach (Control child in control.Controls)
			{
				Register(control, child, eventName);
			}
		}


		/// <summary>
		/// Special case for MoreListView IMoreHostItem collection
		/// </summary>
		/// <param name="control"></param>
		/// <param name="items"></param>
		/// <param name="eventName"></param>
		public void Register(Control control, System.Collections.IEnumerable items, string eventName)
		{
			foreach (var item in items)
			{
				// special case for our MoreListView and hosted controls
				if (item is IMoreHostItem child)
				{
					if (!trash.Exists(t => t.Control == child.Control && t.EventName == eventName))
					{
						Register(control, child.Control, eventName);
					}
				}
			}
		}


		private void Register(Control parent, Control child, string eventName)
		{
			var bound = false;
			var ptype = parent.GetType();
			var ctype = child.GetType();
			var vent = ctype.GetEvent(eventName);
			if (vent != null && ptype.GetEvent(eventName) != null)
			{
				var method = ptype.GetMethod($"On{eventName}",
					BindingFlags.NonPublic | BindingFlags.Instance);

				if (method != null)
				{
					//logger.WriteLine(
					//	$"registering {eventName} for {ptype.FullName} from {ctype.FullName}");

					var action = (EventHandler)((s, e) =>
					{
						//logger.WriteLine(
						//	$"raising {eventName}({e.GetType().FullName}) for {parent.GetType().Name}");

						try
						{
							method.Invoke(parent, new object[] { e });
						}
						catch (Exception exc)
						{
							logger.WriteLine("error raising", exc);
						}
					});

					var pointer = action.Method.MethodHandle.GetFunctionPointer();

					var handler = (Delegate)vent.EventHandlerType
						.GetConstructor(new[] { typeof(object), typeof(IntPtr) })
						.Invoke(new[] { action.Target, pointer });

					vent.AddEventHandler(child, handler);

					trash.Add(new Trash
					{
						Control = child,
						EventName = eventName,
						Handler = handler
					});

					bound = true;
				}
			}

			foreach (Control grandChild in child.Controls)
			{
				Register(bound ? child : parent, grandChild, eventName);
			}
		}
	}
}
