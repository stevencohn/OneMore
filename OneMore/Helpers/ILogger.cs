//************************************************************************************************
// Copyright © 2016 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Xml.Linq;


	/// <summary>
	/// Log file API, inherited by all Commands as 'logger' member
	/// </summary>
	public interface ILogger : IDisposable
	{
		/// <summary>
		/// 
		/// </summary>
		bool IsDebug { get; }


		/// <summary>
		/// 
		/// </summary>
		bool IsVerbose { get; }


		/// <summary>
		/// Gets the system file path of the log file
		/// </summary>
		string LogPath { get; }


		/// <summary>
		/// If debug logging is enabled, writes a blank line to the log file
		/// </summary>
		void Debug();


		/// <summary>
		/// If debug logging is enabled, writes a text message along with a newline.
		/// Could be a new entry or an entry started with a Write
		/// </summary>
		/// <param name="message"></param>
		void Debug(string message);


		/// <summary>
		/// If debug logging is enabled, writes the XML of the given XElement
		/// </summary>
		/// <param name="element"></param>
		void Debug(XElement element);


		/// <summary>
		/// If debug logging is enabled, stops the stopwatch and Writes a message along
		/// with the mm.ss timespan since the stopwatch was started.
		/// </summary>
		/// <param name="message">The message to log</param>
		/// <param name="keepRunning">True to keep the timer running</param>
		void DebugTime(string message, bool keepRunning = false);


		/// <summary>
		/// Dumps all accessible properties of the given object to JSON
		/// </summary>
		/// <param name="obj"></param>
		void Dump(object obj);


		/// <summary>
		/// Ends the current log section; clears the preamble
		/// </summary>
		void End();


		/// <summary>
		/// Writes a new message line and begins a section that will be
		/// prefaced with a ".." preamble to indent related log entries
		/// </summary>
		/// <param name="message"></param>
		void Start(string message = null);


		/// <summary>
		/// Initiates the stopwatch, used for timing critical secitons of code
		/// </summary>
		void StartClock();


		/// <summary>
		/// Stops the stopwatch
		/// </summary>
		void StopClock();


		/// <summary>
		/// Starts a diagnostics section without headers; end with Stop()
		/// </summary>
		void StartDiagnostic();


		/// <summary>
		/// If verbose logging is enabled, writes a blank line to the log file
		/// </summary>
		void Verbose();


		/// <summary>
		/// If verbose logging is enabled, writes a text message along with a newline.
		/// Could be a new entry or an entry started with a Write
		/// </summary>
		/// <param name="message"></param>
		void Verbose(string message);


		/// <summary>
		/// If verbose logging is enabled, writes the XML of the given XElement
		/// </summary>
		/// <param name="element"></param>
		void Verbose(XElement element);


		/// <summary>
		/// If verbose logging is enabled, stops the stopwatch and Writes a message along
		/// with the mm.ss timespan since the stopwatch was started.
		/// </summary>
		/// <param name="message">The message to log</param>
		/// <param name="keepRunning">True to keep the timer running</param>
		void VerboseTime(string message, bool keepRunning = false);


		/// <summary>
		/// Writes a new text message without a newline, can be used to 
		/// append text to an already-started log entry
		/// </summary>
		/// <param name="message"></param>
		void Write(string message);


		/// <summary>
		/// Writes a blank line to the log file
		/// </summary>
		void WriteLine();


		/// <summary>
		/// Writes a text message along with a newline. Could be a new entry or an
		/// entry started with a Write
		/// </summary>
		/// <param name="message"></param>
		void WriteLine(string message);


		/// <summary>
		/// Dumps out a serialized Exception instance to the log
		/// </summary>
		/// <param name="exc"></param>
		void WriteLine(Exception exc);


		/// <summary>
		/// Dumps out a serialized Exception instance to the log, with a preface message
		/// </summary>
		/// <param name="message"></param>
		/// <param name="exc"></param>
		void WriteLine(string message, Exception exc);


		/// <summary>
		/// Write a text message, followed by a newline, and then dump out the
		/// given XElement as a serialized string.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="element"></param>
		void WriteLine(string message, XElement element);


		/// <summary>
		/// Writes the XML of the given XElement
		/// </summary>
		/// <param name="element"></param>
		void WriteLine(XElement element);


		/// <summary>
		/// Stops the stopwatch and Writes a message along with the mm.ss timespan
		/// since the stopwatch was started.
		/// </summary>
		/// <param name="message">The message to log</param>
		/// <param name="keepRunning">True to keep the timer running</param>
		void WriteTime(string message, bool keepRunning = false);
	}
}
