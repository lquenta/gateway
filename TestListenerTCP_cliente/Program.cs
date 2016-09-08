using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TestListenerTCP_cliente
{
    class Program
    {
        static void Main(string[] args)
        {
            string output = "";

            try
            {
                // Create a TcpClient. 
                // The client requires a TcpServer that is connected 
                // to the same address specified by the server and port 
                // combination
                Console.WriteLine("puerto:");
                int port = Int32.Parse(Console.ReadLine());
                Console.Write("Escribir IP a conectarse:");
                string ip = Console.ReadLine(); ;
                TcpClient client = new TcpClient(ip, port);
                Console.WriteLine("escribir mensaje:");
                string message = Console.ReadLine();

                
                byte[] bytes = new byte[256];
                // Translate the passed message into ASCII and store it as a byte array.
                Byte[] data = new Byte[256];
                data = System.Text.Encoding.ASCII.GetBytes(message);

                // Get a client stream for reading and writing. 
                // Stream stream = client.GetStream();
                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);

                output = "Sent: " + message;
                Console.WriteLine(output);

                // Buffer to store the response bytes.
                data = new Byte[256];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes23 = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes23);
                output = "Received: " + responseData;
                Console.WriteLine(output);

                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (SocketException e)
            {
                output = "SocketException: " + e.ToString();
                Console.WriteLine(output);
            }
            catch (Exception ex)
            {
                output = "SocketException: " + ex.ToString();
                Console.WriteLine(output);
            }

            Console.ReadLine();
            /*
             try
            {
                //obtain DNS host name
                //string ip = GetPublicIP();
                Console.Write("Escribir IP a conectarse:");
                string ip = Console.ReadLine(); ;
                IPAddress IP_address = IPAddress.Parse(ip);
                System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient();
                client.Connect(IP_address, 3307);
                try
                {
                    while (true)
                    {

                        Stream s = client.GetStream();
                        StreamReader sr = new StreamReader(s);
                        StreamWriter sw = new StreamWriter(s);
                        sw.AutoFlush = true;
                        Console.WriteLine("escriba mensaje");
                        string tramaEnvuiar = " M`☻◄☺G☺        ☻ `4128000108231624000115123456781234FFFF00000005550 0174128";
                        sw.WriteLine(tramaEnvuiar);
                        while (true)
                        {
                            string respuesta = sr.ReadLine();
                            Console.WriteLine(respuesta);
                            break;
                        }

                    }
                }
                finally
                {
                    // code in finally block is guranteed 
                    // to execute irrespective of 
                    // whether any exception occurs or does 
                    // not occur in the try block
                    client.Close();
                }
            }catch(Exception ex){
                Console.WriteLine(ex.Message);
            }*/
        }
    }
}
