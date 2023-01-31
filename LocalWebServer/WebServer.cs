using EmbedIO;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LocalWebServer.app
{
    public class AppWebServer :IDisposable
    {
        //public WebServer(Action<ulong> getChat, Action getUsers)
        //{
        //    GetChat = getChat;
        //    GetUsers = getUsers;
        //}
        //internal Action<ulong> GetChat;
        //internal Action GetUsers;

        //private TcpListener myListener;
        //private int port = 5050; // Select any free port you wish   
        //The constructor which make the TcpListener start listening on th  
        //given port. It also calls a Thread on the method StartListen().   
        //public WebServer()
        //{
        //    try
        //    {
        //        //start listing on the given port  
        //        myListener = new TcpListener(port);
        //        myListener.Start();
        //        Console.WriteLine("Web Server Running... Press ^C to Stop...");
        //        //start the thread which calls the method 'StartListen'  
        //        Thread th = new Thread(new ThreadStart(StartListen));
        //        th.Start();
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("An Exception Occurred while Listening :" + e.ToString());
        //    }
        //}
        //public void StartListen()
        //{
        //    int iStartPos = 0;
        //    String sRequest;
        //    String sDirName;
        //    String sRequestedFile;
        //    String sErrorMessage;
        //    String sLocalDir;
        //    String sMyWebServerRoot = "C:\\MyWebServerRoot\\";
        //    String sPhysicalFilePath = "";
        //    String sFormattedMessage = "";
        //    String sResponse = "";
        //    while (true)
        //    {
        //        //Accept a new connection  
        //        Socket mySocket = myListener.AcceptSocket();
        //        Console.WriteLine("Socket Type " + mySocket.SocketType);
        //        if (mySocket.Connected)
        //        {
        //            Console.WriteLine("\nClient Connected!!\n==================\nCLient IP { 0}\n", mySocket.RemoteEndPoint);  
        //            //make a byte array and receive data from the client   
        //            Byte[] bReceive = new Byte[1024];
        //            int i = mySocket.Receive(bReceive, bReceive.Length, 0);
        //            //Convert Byte to String  
        //            string sBuffer = Encoding.ASCII.GetString(bReceive);
        //            //At present we will only deal with GET type  
        //            if (sBuffer.Substring(0, 3) != "GET")
        //            {
        //                Console.WriteLine("Only Get Method is supported..");
        //                mySocket.Close();
        //                return;
        //            }
        //            // Look for HTTP request  
        //            iStartPos = sBuffer.IndexOf("HTTP", 1);
        //            // Get the HTTP text and version e.g. it will return "HTTP/1.1"  
        //            string sHttpVersion = sBuffer.Substring(iStartPos, 8);
        //            // Extract the Requested Type and Requested file/directory  
        //            sRequest = sBuffer.Substring(0, iStartPos - 1);
        //            //Replace backslash with Forward Slash, if Any  
        //            sRequest.Replace("\\", "/");
        //            //If file name is not supplied add forward slash to indicate   
        //            //that it is a directory and then we will look for the   
        //            //default file name..  
        //            if ((sRequest.IndexOf(".") < 1) && (!sRequest.EndsWith("/")))
        //            {
        //                sRequest = sRequest + "/";
        //            }
        //            //Extract the requested file name  
        //            iStartPos = sRequest.LastIndexOf("/") + 1;
        //            sRequestedFile = sRequest.Substring(iStartPos);
        //            //Extract The directory Name  
        //            sDirName = sRequest.Substring(sRequest.IndexOf("/"), sRequest.LastIndexOf("/") - 3);
        //        }
        //   }
        //}
        public static string Url = "http://localhost:8787/";

        private readonly string _filePath;
        private WebServer _server;

        public AppWebServer(string filePath)
        {
            _filePath = filePath;
            StartWebServer();
        }


        public void StartWebServer()
        {
            _server = new WebServer(Url);

           //server.RegisterModule(new LocalSessionModule());
           // _server.RegisterModule(new StaticFilesModule(_filePath));
            //_server.Module<StaticFilesModule>().UseRamCache = true;
            //_server.Module<StaticFilesModule>().DefaultExtension = ".html";
            //_server.Module<StaticFilesModule>().DefaultDocument = "index.html";
            //_server.Module<StaticFilesModule>().UseGzip = false;



            Task.Factory.StartNew(async () =>
            {
                Debug.WriteLine("Starting Server");
                await _server.RunAsync();
            });
        }
        public void Dispose()
        {
            _server?.Dispose();
        }

    }
}
