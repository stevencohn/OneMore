//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreProtocolHandler
{
	using System;
	using System.Diagnostics;
	using System.IO.Pipes;
	using System.Text;


	class Program
	{
		private const string PipeName = "River.OneMore";


		static void Main(string[] args)
		{
			try
			{
				using (var client = new NamedPipeClientStream(".",
					PipeName, PipeDirection.Out, PipeOptions.Asynchronous))
				{
					// The connect function will indefinitely wait for the pipe to become available
					// If that is not acceptable specify a maximum waiting time (in ms)
					client.Connect(1000);
					Debug.WriteLine("[Client] Pipe connection established");

					var data = args[0].Trim();

					byte[] buffer = Encoding.UTF8.GetBytes(data);
					client.Write(buffer, 0, buffer.Length);
					client.Flush();
					client.Close();
				}

			}
			catch (Exception exc)
			{
				Debug.WriteLine(exc.Message);
			}
		}
	}
}
