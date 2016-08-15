using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitor
{
    abstract class Base
    {
        public abstract void create(MainWindow win, string ips, string port);

        public abstract void connect();

        public abstract void send(string s);

        public abstract void receive(object obj);
    }
}
