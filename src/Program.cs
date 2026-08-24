using System;
using System.Threading;
using System.Windows.Forms;
using DdcCi.BrightnessTray.App;
using DdcCi.BrightnessTray.Core;

namespace DdcCi.BrightnessTray
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            bool isNewInstance;
            using (Mutex mutex = new Mutex(true, @"Local\DdcCiBrightnessTray", out isNewInstance))
            {
                if (!isNewInstance) return;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                using (TrayAppController controller = new TrayAppController(
                    new BrightnessService(delegate { return new MonitorScanner().Scan(); })))
                {
                    Application.Run(controller);
                }
            }
        }
    }
}
