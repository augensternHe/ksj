using System;
using System.Threading;
using System.Windows.Forms;

namespace csAcq4
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // 显示启动画面
            ShowSplashScreen();
            Application.Run(new Form1());
        }
        // 显示启动画面
        private static void ShowSplashScreen()
        {
            // 创建一个新线程来显示启动画面
            Thread splashThread = new Thread(() =>
            {
                SplashForm splashForm = new SplashForm();
                splashForm.StartPosition = FormStartPosition.CenterScreen; // 居中显示
                Application.Run(splashForm); // 启动窗体
            });

            // 启动线程
            splashThread.Start();

            //// 模拟主窗体加载的延迟，比如加载配置文件、初始化资源等
            //Thread.Sleep(3000);  // 这里是显示3秒，可以根据需要调整时间

            //// 关闭启动画面
            //splashThread.Abort();
        }
    }
}
