using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using testConexion1_cerberus;
using System.Threading;
namespace GATEWAY
{
   

    public partial class Service1 : ServiceBase
    {

        AutoResetEvent StopRequest = new AutoResetEvent(false);
        private ManualResetEvent _shutdownEvent = new ManualResetEvent(false);
        private Thread _thread;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _thread = new Thread(WorkerThreadFunc);
            _thread.Name = "My Worker Thread";
            _thread.IsBackground = true;
            _thread.Start();

            
        }

        protected override void OnStop()
        {
            _shutdownEvent.Set();
            if (!_thread.Join(3000))
            { // give the thread 3 seconds to stop
                _thread.Abort();
            }
        }
        private void WorkerThreadFunc()
        {
            while (!_shutdownEvent.WaitOne(0))
            {
                IniciarListenerINAC();
            }
        }
        static string localhost_ip;
        static string Get_ip_local_address()
        {
            System.Net.IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }
            return localIP;
        }
        static string output = "";
        public void IniciarListenerINAC()
        {
            string sSource = "GatewayATC_LOG";
            EventLog.WriteEntry(sSource, "Gateway Iniciado");

            TcpListener tcpListener = null;
            localhost_ip = Get_ip_local_address();
            IPAddress ipAddress = IPAddress.Parse(localhost_ip);
            //IPAddress ipAddress = Dns.GetHostEntry("localhost").AddressList[0];
            try
            {
                int puerto = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["puerto"]);
                tcpListener = new TcpListener(ipAddress, puerto);
                tcpListener.Start();
                output = "IP local:" + ipAddress + ",esperando conexion en puerto:" + puerto.ToString() + "...";
                //Console.Write(output);
                EventLog.WriteEntry(sSource, output);
            }
            catch (Exception ex)
            {
                output = "Error:" + ex.ToString();
                EventLog.WriteEntry(sSource, output, EventLogEntryType.Error, 234);
            }
            while (true)
            {
                tcpListener.Start();
                //Console.WriteLine("Esperando mensaje del POS...");
                Thread.Sleep(10);
                TcpClient tcpClient = tcpListener.AcceptTcpClient();
                tcpClient.NoDelay = true;
                byte[] bytes = new byte[256];
                NetworkStream stream = tcpClient.GetStream();
                stream.Read(bytes, 0, bytes.Length);
                SocketHelper helper = new SocketHelper();
                EventLog.WriteEntry(sSource, "Mensaje recibido");
                //Console.Write("Mensaje recibido");
                helper.processMsg(tcpClient, stream, bytes);
                stream.Flush();
                if (StopRequest.WaitOne(10000)) return;
                tcpClient.Close();


            }

        }
        public class RespuestaServicioBISA
        {
            public string referenceNumber { get; set; }
            public string cardNumber { get; set; }
            public string expiration { get; set; }
            public string code { get; set; }


        }
        public static string RandomDigits(int length)
        {
            var random = new Random();
            string s = string.Empty;
            for (int i = 0; i < length; i++)
            {
                s = string.Concat(s, random.Next(10).ToString());
            }
            return s;
        }
        public static RespuestaServicioBISA LLamarServicioBisa(decimal amount, string commerce, string currency, string movilNumber, string smsPIN,string str_reference_number)
        {
            RespuestaServicioBISA respuesta = new RespuestaServicioBISA();
            ServicePointManager.ServerCertificateValidationCallback =
           delegate(object s, X509Certificate certificate,
                    X509Chain chain, SslPolicyErrors sslPolicyErrors)
           { return true; };

            testConexion1_cerberus.BISAService.purchasePOSATCRequest requestPurchase = new testConexion1_cerberus.BISAService.purchasePOSATCRequest();
            testConexion1_cerberus.BISAService.aquaClient clienteBisa = new testConexion1_cerberus.BISAService.aquaClient();
            clienteBisa.ClientCredentials.UserName.UserName = System.Configuration.ConfigurationManager.AppSettings["usuarioBISA_SOAP"];
            clienteBisa.ClientCredentials.UserName.Password = System.Configuration.ConfigurationManager.AppSettings["passwordBISA_SOAP"];
            Random rnd = new Random();
            string numeroAleatorio = RandomDigits(16);
            //string referenceNumber = commerce + numeroAleatorio; //16 pos
            string referenceNumber = str_reference_number;
            requestPurchase.amount = amount;
            requestPurchase.commerce = commerce;
            requestPurchase.currency = currency;
            requestPurchase.movilNumber = movilNumber;
            requestPurchase.referenceNumber = referenceNumber;
            requestPurchase.smsPIN = smsPIN;
            testConexion1_cerberus.BISAService.purchasePOSATCResponse responsePurchase = new testConexion1_cerberus.BISAService.purchasePOSATCResponse();
            clienteBisa.ClientCredentials.ServiceCertificate.SetDefaultCertificate(StoreLocation.LocalMachine, StoreName.My, X509FindType.FindBySubjectName, System.Configuration.ConfigurationManager.AppSettings["nombreCertificadoBISA_SOAP"]);
            try
            {
                clienteBisa.Open();
                responsePurchase = clienteBisa.purchasePOSATC(requestPurchase);
                respuesta.code = responsePurchase.code;
                respuesta.cardNumber = responsePurchase.cardNumber;
                respuesta.expiration = responsePurchase.expiration.ToString("yyMM", System.Globalization.CultureInfo.InvariantCulture);
                respuesta.referenceNumber = referenceNumber;
            }
            catch (Exception)
            {
                respuesta.cardNumber = "";
                respuesta.code = "99";
                respuesta.expiration = "";
                respuesta.referenceNumber = referenceNumber;
            }

            return respuesta;
        }
       
    }
    class SocketHelper
    {
        TcpClient msClient;
        string mstrMessage;
        string mstrResponse;
        byte[] bytesSent;
        public void processMsg(TcpClient client, NetworkStream stream, byte[] bytesRecibidos)
        {
            string sSource = "GatewayATC_LOG";
            
            
            msClient = client;
            byte[] tramaFinal = null;
            if (mstrMessage != "")
            {
                EventLog.WriteEntry(sSource, "Mensaje del POS RECIBIDO:"+mstrMessage);
                mstrMessage = Encoding.ASCII.GetString(bytesRecibidos, 0, bytesRecibidos.Length);
                //Console.WriteLine(Convert.ToBase64String(bytesRecibidos));
                byte[] longitud = new byte[] { bytesRecibidos[0], bytesRecibidos[1] };
                byte[] longitudSalida = new byte[] { 0, 61 };
                byte[] transaccionFinanciera = new byte[] { bytesRecibidos[2] };
                byte[] destinoNII = new byte[] { bytesRecibidos[3], bytesRecibidos[4] };
                byte[] origen = new byte[] { bytesRecibidos[5], bytesRecibidos[6] };
                byte[] MTIrespuesta = new byte[] { bytesRecibidos[7], 16 };
                byte[] MTIconsulta = new byte[] { bytesRecibidos[7], bytesRecibidos[8] };
                byte[] bitmap = new byte[] { bytesRecibidos[9], bytesRecibidos[10], bytesRecibidos[11], bytesRecibidos[12], bytesRecibidos[13], bytesRecibidos[14], bytesRecibidos[15], bytesRecibidos[16] };
                byte[] longde63 = new byte[] { bytesRecibidos[17], bytesRecibidos[18] };
                byte[] longde63_envio = new byte[] { 0, 68 };
                byte[] numTerminal = new byte[] { bytesRecibidos[19], bytesRecibidos[20], bytesRecibidos[21], bytesRecibidos[22], bytesRecibidos[23], bytesRecibidos[24], bytesRecibidos[25], bytesRecibidos[26] };
                byte[] fecha = new byte[] { bytesRecibidos[27], bytesRecibidos[28], bytesRecibidos[29], bytesRecibidos[30] };
                byte[] hora = new byte[] { bytesRecibidos[31], bytesRecibidos[32], bytesRecibidos[33], bytesRecibidos[34] };
                byte[] sysTraceNumber = new byte[] { bytesRecibidos[35], bytesRecibidos[36], bytesRecibidos[37], bytesRecibidos[38], bytesRecibidos[39], bytesRecibidos[40] };
                string numeroCelular = mstrMessage.Substring(41, 8);
                string claveMovil = mstrMessage.Substring(49, 4);
                string monto = mstrMessage.Substring(57, 10);
                decimal montoDecimal = Decimal.Parse(monto) / 100;
                string currency = mstrMessage.Substring(69, 1);
                string commerce = mstrMessage.Substring(70, 6);
                byte[] reference_number = numTerminal.Concat(fecha).ToArray().Concat(hora).ToArray().Concat(sysTraceNumber).ToArray();
                string str_reference_number = Encoding.ASCII.GetString(reference_number);
                StringBuilder argumentos = new StringBuilder();
                argumentos.AppendLine("numero celular:" + numeroCelular);
                argumentos.AppendLine("numero celular:" + numeroCelular);
                argumentos.AppendLine("Clave movil:" + claveMovil);
                argumentos.AppendLine("monto:" + montoDecimal.ToString());
                argumentos.AppendLine("moneda:" + currency);
                argumentos.AppendLine("commerce:" + commerce);
                Console.WriteLine("str_reference_number" + str_reference_number);
                argumentos.AppendLine("Enviando a BISA...");
                EventLog.WriteEntry(sSource, argumentos.ToString());
                GATEWAY.Service1.RespuestaServicioBISA respuestaBisa = GATEWAY.Service1.LLamarServicioBisa(montoDecimal, commerce, currency, numeroCelular, claveMovil,str_reference_number);
                /*Program.RespuestaServicioBISA respuestaBisa = new Program.RespuestaServicioBISA();
                respuestaBisa.code = "00";
                respuestaBisa.cardNumber = "1234567890123456";
                respuestaBisa.expiration = "9922";
                Console.WriteLine("Codigo de respuesta:" + respuestaBisa.code);
                Console.WriteLine("Card Number:" + respuestaBisa.cardNumber);
                Console.WriteLine("ExpirationDate:" + respuestaBisa.expiration);
                Console.WriteLine(".....Fin de mensaje.....");
                Console.WriteLine("Armado de trama de respuesta");*/

                string header = mstrMessage.Substring(1, 39);
                string tramaNumeroTarjeta = (respuestaBisa.cardNumber == null) ? "0".PadLeft(16, '0') : respuestaBisa.cardNumber;
                string tramaFechaVencimiento = (respuestaBisa.expiration == null) ? "0000" : respuestaBisa.expiration;
                string tramaCodigoRespuesta = respuestaBisa.code.PadLeft(2, '0');

                tramaFinal = longitudSalida.Concat(transaccionFinanciera).ToArray().Concat(origen).ToArray().Concat(destinoNII).ToArray().Concat(MTIrespuesta).ToArray().Concat(bitmap).ToArray().Concat(longde63_envio).ToArray().Concat(numTerminal).ToArray().Concat(fecha).ToArray().Concat(hora).ToArray().Concat(sysTraceNumber).ToArray();

                string tramaEnvio = tramaNumeroTarjeta + tramaFechaVencimiento + tramaCodigoRespuesta;
                EventLog.WriteEntry(sSource, tramaEnvio);
                mstrResponse = tramaEnvio;
            }
            bytesSent = tramaFinal.Concat(Encoding.ASCII.GetBytes(mstrResponse)).ToArray();
            stream.Write(bytesSent, 0, bytesSent.Length);
            stream.Flush();
            EventLog.WriteEntry(sSource, "Respuesta enviada");
            //Console.WriteLine("Respuesta enviada");
        }
    }
}
