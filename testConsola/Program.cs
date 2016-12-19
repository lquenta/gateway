using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace testConsola
{
    class Program
    {
        static TcpListener listener;
        const int LIMIT = 1;
        static string localhost_ip;
        static string Get_ip_local_address()
        {
            System.Net.IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                Console.WriteLine("ip encontrado:" + ip.ToString());
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
               

            }
            Console.WriteLine("ip servidor");
            localIP = Console.ReadLine();
            return localIP;
        }
        static string output = "";
        static void Main(string[] args)
        {
            string sSource = "GatewayATC_LOG";
            if (!System.Diagnostics.EventLog.SourceExists(sSource))
            {
                Console.Write("wirt");
                
                System.Diagnostics.EventLog.CreateEventSource(sSource, sSource);
            }
            EventLog.WriteEntry("Aplicacion", "message");
            EventLog.WriteEntry(sSource, "Gateway Iniciado");
            Console.Read();
        }

    }
    class SocketHelper
    {
        TcpClient msClient;
        string mstrMessage;
        string mstrResponse;
        byte[] bytesSent;
        public void processMsg(TcpClient client, NetworkStream stream,string respuesta)
        {
                //mstrResponse = "test de trama desde gateway";


                //armar la trama de respuesta

                //mstrResponse = "Repsuesta";

            bytesSent = Encoding.ASCII.GetBytes(respuesta);
            stream.Write(bytesSent, 0, bytesSent.Length);
            stream.Flush();
            Console.WriteLine("Respuesta enviada");
        }
    }
}
