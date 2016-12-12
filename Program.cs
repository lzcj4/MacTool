
using System;
using System.Threading;
using System.Windows.Forms;

namespace MacTool
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += (sender, e) =>
            {
                MessageBox.Show(string.Format("出现未知异常:{0}\r\n{1}", e.Exception.Message, e.Exception.StackTrace));
            };
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                MessageBox.Show(string.Format("出现未知异常:{0}", e.ExceptionObject.ToString()));
            };

            Application.Run(new FrmMac());
        }
    }
}
