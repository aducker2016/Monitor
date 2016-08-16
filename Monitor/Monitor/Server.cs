using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Monitor
{
    class Server : Base
    {
        MainWindow window;
        Socket serverSocket;
        List<Socket> sockets = new List<Socket>();

        public override void create(MainWindow win, string ips, string port)
        {
            window = win;

            string ipstr = "127.0.0.1";
            string[] ipss = ips.Split(new Char[] { '#' });
            if (ipss.Length > 0)
            {
                ipstr = ipss[0];
            }

            IPAddress ip = IPAddress.Parse(ipstr);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(ip, int.Parse(port)));
            serverSocket.Listen(30);
            output("启动监听:" + serverSocket.LocalEndPoint.ToString());

            Thread thread = new Thread(connect);
            thread.IsBackground = true;
            thread.Start();
        }

        public override void connect()
        {
            // 监听连接
            while (true)
            {
                Socket socket = serverSocket.Accept();
                sockets.Add(socket);
                output("客户端连接成功:" + socket.RemoteEndPoint.ToString());

                Thread thread = new Thread(receive);
                thread.IsBackground = true;
                thread.Start(socket);
            }
        }

        public override bool send(string s)
        {
            // 发送数据
            bool res = false;
            for (int i = sockets.Count - 1; i >= 0; i--)
            {
                Socket socket = sockets[i];
                try
                {
                    socket.Send(Encoding.UTF8.GetBytes(s + "#"));
                    res = true;
                }
                catch
                {
                    output("客户端断开连接:" + socket.RemoteEndPoint.ToString());
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                    sockets.RemoveAt(i);
                }
            }
            return res;
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
                    output("客户端断开连接:" + socket.RemoteEndPoint.ToString() + " error:" + ex.Message);
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                    sockets.Remove(socket);
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
