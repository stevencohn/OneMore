//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreTray
{
	using River.OneMoreAddIn;
	using System;
	using System.Windows.Forms;


	internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new App());
        }
    }


    internal class App : ApplicationContext
    {
        private readonly NotifyIcon trayIcon;

        public App()
        {
            // initialize Tray Icon
            trayIcon = new NotifyIcon()
            {
                Icon = Properties.Resources.Logo,
                ContextMenu = new ContextMenu(new MenuItem[] {
                    new("Exit", DoExit)
                }),
                Visible = true
            };

            Logger.SetApplication("OneMoreTray");

			// hashtags scanner
			new River.OneMoreAddIn.Commands.HashtagService().Startup();
		}

		void DoExit(object sender, EventArgs e)
        {
            Logger.Current.WriteLine("shutting down tray");
            // hide tray icon, otherwise it will remain shown until user mouses over it
            trayIcon.Visible = false;
            Application.Exit();
        }
    }
}
