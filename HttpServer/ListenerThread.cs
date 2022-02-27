using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Binance2MT5.HttpServer
{
    public class ListenerThread
    {
        // Fields
        private string uriPrefix;
        private bool RunThread;

        // Constructor
        public ListenerThread(string uri)
        {
            uriPrefix = uri;
        }

        // Public methods
        public void Start()
        {
            Thread listenerThread = new Thread(Listen);
            listenerThread.Start();
        }

        public virtual string GetResponse(HttpListenerRequest request)
        {
            return "";
        }

        // Private methods
        private void Listen()
        {
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }

            // run thread as long as RunThread == true
            RunThread = true;

            // Create a listener.
            HttpListener listener = new HttpListener();

            listener.Prefixes.Add(uriPrefix);

            listener.Start();
            //Console.WriteLine(string.Format("Listen on: {0}",uriPrefix));

            while (RunThread)
            {
                // Note: The GetContext method blocks while waiting for a request. 
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;

                // Obtain a response object.
                HttpListenerResponse response = context.Response;

                // Construct a response.
                string responseString = GetResponse(request);

                try
                {
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

                    // Get a response stream and write the response to it.
                    response.ContentLength64 = buffer.Length;
                    System.IO.Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception in Listner:" + ex);
                }
            }

            listener.Stop();
        }

        public void StopListener()
        {
            RunThread = false;
        }
    }
}
