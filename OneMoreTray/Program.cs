//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreTray
{
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


    public class App : ApplicationContext
    {
        private readonly NotifyIcon trayIcon;

        public App()
        {
            // initialize Tray Icon
            trayIcon = new NotifyIcon()
            {
                Icon = Properties.Resources.Logo,
                ContextMenu = new ContextMenu(new MenuItem[] {
                    new MenuItem("Exit", Exit)
                }),
                Visible = true
            };
        }

        void Exit(object sender, EventArgs e)
        {
            // hide tray icon, otherwise it will remain shown until user mouses over it
            trayIcon.Visible = false;
            Application.Exit();
        }
    }
}