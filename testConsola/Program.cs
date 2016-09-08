using System;
using System.Collections.Generic;
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
            TcpListener tcpListener = null;
            localhost_ip = Get_ip_local_address();
            IPAddress ipAddress = IPAddress.Parse(localhost_ip);
            //IPAddress ipAddress = Dns.GetHostEntry("localhost").AddressList[0];
            try
            {
                Console.WriteLine("Puerto:");
                int port = Int32.Parse(Console.ReadLine());
                tcpListener = new TcpListener(ipAddress, port);
                tcpListener.Start();
                output = ipAddress + "Esperando conexion en ..."  ;
                Console.Write(output);
            }
            catch (Exception ex)
            {
                output = "Error:" + ex.ToString();
                Console.WriteLine(output);
            }
            while (true)
            {
                Thread.Sleep(10);
                TcpClient tcpClient = tcpListener.AcceptTcpClient();
                byte[] bytes = new byte[256];
                NetworkStream stream = tcpClient.GetStream();
                Byte[] data = new Byte[256];
                Int32 bytes23 = stream.Read(data, 0, data.Length);
                string responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes23);
                output = "Received: " + responseData;
                Console.WriteLine(output);
                Console.WriteLine("Escribir respuesta");
                string mandarRespuesta = Console.ReadLine();


                SocketHelper helper = new SocketHelper();
                Console.WriteLine("escritura en stream");
                helper.processMsg(tcpClient, stream, mandarRespuesta);
                stream.Flush();
                Console.ReadLine();
                


            }

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
