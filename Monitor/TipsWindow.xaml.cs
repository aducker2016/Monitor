using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Monitor
{
    /// <summary>
    /// TipsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TipsWindow : Window
    {
        public bool IsClosed { get; set; }

        private DispatcherTimer Timer { get; set; }

        public TipsWindow()
        {
            InitializeComponent();

            IsClosed = false;

            Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromMilliseconds(200);
            Timer.Tick += new EventHandler(TimeOut);
            Timer.Start();

            this.Top = SystemParameters.WorkArea.Height;
            this.Left = SystemParameters.WorkArea.Width - this.Width;
        }

        private void TimeOut(object sender, EventArgs e)
        {
            for (int i = 0; i < this.Height / 3; i++)
            {
                this.Top -= 3;
                Thread.Sleep(6);
            }
            Timer.Stop();
            Timer = null;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            IsClosed = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
