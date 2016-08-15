using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Monitor
{
    class Client : Base
    {
        MainWindow window;
        Dictionary<string, Socket> socketMap = new Dictionary<string, Socket>();

        public override void create(MainWindow win, string ips, string port)
        {
            window = win;

            string[] ipss = ips.Split(new Char[] { '#' });
            foreach (string ip in ipss)
            {
                socketMap[ip + ":" + port] = null;
            }

            Thread thread = new Thread(connect);
            thread.IsBackground = true;
            thread.Start();
        }

        public override void connect()
        {
            byte[] tmp = new byte[1];
            while(true)
            {
                string[] keys = socketMap.Keys.ToArray();
                foreach (string key in keys)
                {
                    if (null == socketMap[key])
                    {
                        // 连接
                        try
                        {
                            string[] info = key.Split(new Char[] { ':' });
                            IPAddress ip = IPAddress.Parse(info[0]);
                            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            socket.Connect(new IPEndPoint(ip, int.Parse(info[1])));
                            socketMap[key] = socket;
                            output("连接服务器成功:" + socket.RemoteEndPoint.ToString());

                            Thread thread = new Thread(receive);
                            thread.IsBackground = true;
                            thread.Start(socket);
                        }
                        catch
                        {

                        }
                    }
                    else
                    {
                        // 检查是否断开
                        try
                        {
                            Socket socket = socketMap[key];
                            socket.Send(tmp, 0, 0);
                        }
                        catch
                        {
                            Socket socket = socketMap[key];
                            output("断开服务器连接:" + socket.RemoteEndPoint.ToString());
                            socket.Shutdown(SocketShutdown.Both);
                            socket.Close();
                            socketMap[key] = null;
                        }
                    }                   
                }
                Thread.Sleep(10000);
            }
        }

        public override void send(string s)
        {
            // 发送数据
            foreach (var item in socketMap)
            {
                if (null != item.Value)
                {
                    try
                    {
                        Socket socket = item.Value;
                        socket.Send(Encoding.UTF8.GetBytes(s + "#"));
                    }
                    catch
                    {
                    }
                }
            }
        }

        public override void receive(object obj)
        {
            // 接收数据
            Socket socket = (Socket)obj;
            byte[] datas = new byte[10240];
            while (true)
            {
                try
                {
                    int receiveNumber = socket.Receive(datas);
                    string[] receiveStrs = Encoding.UTF8.GetString(datas, 0, receiveNumber).Split(new Char[] { '#' });
                    foreach (string s in receiveStrs)
                    {
                        if (s.Length > 0)
                        {
                            output("[ " + System.DateTime.Now.ToString() + "  :  " + socket.RemoteEndPoint.ToString() + " ]\n-> " + s);
                        }
                    }
                }
                catch (Exception ex)
                {
                    break;
                }
            }
        }

        public void output(string s)
        {
            MainWindow.DelegateReceive r = new MainWindow.DelegateReceive(window.receive);
            window.Dispatcher.Invoke(r, new object[] { s });
        }
    }
}
