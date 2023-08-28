using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gui_Miner
{
    internal static class Program
    {
        private const string MutexName = "GuiMinerSingleInstanceMutex";
        private static Mutex singleInstanceMutex;

        [STAThread]
        static void Main()
        {
            // Create a named mutex to enforce single instance
            singleInstanceMutex = new Mutex(true, MutexName, out bool createdNew);

            if (createdNew)
            {
                // This instance is the first one, run the application
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());

                // Release the mutex when the application exits
                singleInstanceMutex.ReleaseMutex();
            }
            else
            {
                // Another instance is already running, notify the user or take appropriate action
                MessageBox.Show("Another instance of the application is already running.");
            }
        }
    }
}