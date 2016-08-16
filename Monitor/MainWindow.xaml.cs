using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Configuration;
using System.IO;

namespace Monitor
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Base monitor;
        TipsWindow tips;
        bool isClient;
        NotifyIcon notifyIcon;

        private DispatcherTimer Timer { get; set; }

        public delegate void DelegateReceive(string s);

        public MainWindow()
        {
            InitializeComponent();

            //配置
            string KEY_RUNTYPE = "runtype";
            string KEY_SERVERIPS = "serverips";
            string KEY_SERVERPORT = "serverport";
            string VAL_SERVER = "server";
            string VAL_CLIENT = "client";

            string runtype = ConfigurationManager.AppSettings[KEY_RUNTYPE];
            if (runtype != VAL_SERVER && runtype != VAL_CLIENT)
            {
                if (MessageBoxResult.Yes == System.Windows.MessageBox.Show("作为服务端运行？ 是：服务端  否：客户端", "运行模式", MessageBoxButton.YesNo))
                {
                    runtype = VAL_SERVER;
                }
                else
                {
                    runtype = VAL_CLIENT;
                }
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings.Remove(KEY_RUNTYPE);
                config.AppSettings.Settings.Remove(KEY_SERVERIPS);
                config.AppSettings.Settings.Remove(KEY_SERVERPORT);
                config.AppSettings.Settings.Add(KEY_RUNTYPE, runtype);
                config.AppSettings.Settings.Add(KEY_SERVERIPS, "127.0.0.2#127.0.0.3");
                config.AppSettings.Settings.Add(KEY_SERVERPORT, "8885");
                config.Save();
                ConfigurationManager.RefreshSection("appSettings");
            }
            string serverips = ConfigurationManager.AppSettings[KEY_SERVERIPS];
            string serverport = ConfigurationManager.AppSettings[KEY_SERVERPORT];

            //隐藏主界面
#if !DEBUG
            Hide();
#endif

            //系统托盘
            notifyIcon = new NotifyIcon();
            notifyIcon.Text = "监控中心";
            notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            notifyIcon.Visible = true;

            string runtypetext = (runtype == VAL_SERVER) ? "服务端模式" : "客户端模式";
            System.Windows.Forms.MenuItem runtypeshow = new System.Windows.Forms.MenuItem(runtypetext);
            System.Windows.Forms.MenuItem exit = new System.Windows.Forms.MenuItem("退出");
            exit.Click += new EventHandler(OnExit);
            System.Windows.Forms.MenuItem[] childen = new System.Windows.Forms.MenuItem[] { runtypeshow, exit };
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(childen);

            //运行
            if (runtype == VAL_SERVER)
            {
                //服务端
                monitor = new Server();
                monitor.create(this, serverips, serverport);
                isClient = false;

                Timer = new DispatcherTimer();
                Timer.Interval = TimeSpan.FromMilliseconds(30000);
                Timer.Tick += new EventHandler(TimeOut);
                Timer.Start();
            }
            else if (runtype == VAL_CLIENT)
            {
                //客户端
                monitor = new Client();
                monitor.create(this, serverips, serverport);
                isClient = true;
            }
        }

        private void CreateClient(object sender, RoutedEventArgs e)
        {
            //monitor = new Client();
            //monitor.create(this);
            //isClient = true;
        }

        private void CreateServer(object sender, RoutedEventArgs e)
        {
            //monitor = new Server();
            //monitor.create(this);
            //isClient = false;
        }

        private void Send(object sender, RoutedEventArgs e)
        {
            monitor.send(TextSend.Text);
        }

        public void receive(string s)
        {
            if (TextReceive.Text.Length > 0)
            {
                TextReceive.Text += "\n\n";
            }
            TextReceive.Text += s;

            if (isClient)
            {
                if (null == tips || tips.IsClosed)
                {
                    tips = new TipsWindow();
                    //tips.Owner = this;
                    tips.Show();
                }
                if (tips.TextReceive.Text.Length > 0)
                {
                    tips.TextReceive.Text += "\n\n";
                }
                tips.TextReceive.Text += s;
            }
        }

        private void OnExit(object sender, EventArgs e)
        {
            Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            notifyIcon.Dispose();
            notifyIcon = null;

            if (null != tips && !tips.IsClosed)
            {
                tips.Close();
            }
        }

        private void TimeOut(object sender, EventArgs e)
        {
            string filename = "SendData.log";

            try
            {
                StreamReader reader = new StreamReader(filename);
                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Length > 0)
                    {
                        monitor.send(line);
                    }
                }
                reader.Close();
            }
            catch
            { }

            try
            {
                StreamWriter writer = new StreamWriter(filename, false, Encoding.UTF8);
                writer.Flush();
                writer.Close();
            }
            catch
            { }
        }
    }
}
