using System;
using System.Net;
using System.Threading;
using System.Text;
using System.IO;
using System.IO.IsolatedStorage;
using System.Diagnostics;

namespace Ringify
{

    public delegate void LoadCompletedEventHandler(object sender, EventArgs e);

    public class ReadState
    {
        public Stream Response { get; set; }
        public Stream AccumulatedResponse { get; set; }
        public byte[] Buffer { get; set; }
    }

    public class RequestState
    {
        public Object Client;
        const int BufferSize = 1024;
        public byte[] RequestData;
        public byte[] BufferRead;
        public WebRequest Request;
        public WebResponse Response;
        public Stream ResponseStream;
        public Stream RequestStream;
        // Create Decoder for appropriate enconding type.
        public Decoder StreamDecode = Encoding.UTF8.GetDecoder();

        public RequestState()
        {
            BufferRead = new byte[BufferSize];
            RequestData = null;
            Request = null;
            ResponseStream = null;
        }
    }

    // ClientGetAsync issues the async request.
    public class ClientGetAsync
    {
        public LoadCompletedEventHandler LoadCompleted;

        const int BUFFER_SIZE = 1024;
        private static String m_Filename;
        public ClientGetAsync()
        {

        }
            
        public void Load(Uri i_Uri, String i_FileName)
        {
            m_Filename = i_FileName;

            //WebClient Client = new WebClient();

            // Specify that the DownloadFileCallback method gets called
            // when the download completes.
            //Client.d += new AsyncCompletedEventHandler(DownloadFileCallback2);
            // Specify a progress notification handler.
            //Client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);
            //Client.DownloadStringAsync(httpSite, "serverdata.txt");

            // Create the request object.
            WebRequest wreq = WebRequest.Create(i_Uri);
            // wreq.Method = "PUT";

            // Create the state object.
            RequestState rs = new RequestState();
            
            // Put the request into the state object so it can be passed around.
            rs.Request = wreq;
            rs.Client = this;

            // Issue the async request.
            IAsyncResult r = (IAsyncResult)wreq.BeginGetResponse(
               new AsyncCallback(RespCallback), rs);
           
        }

        public void ReadCallback(IAsyncResult result)
        {
            ReadState readState = result.AsyncState as ReadState;
            int bytesRead = readState.Response.EndRead(result);
            if (bytesRead > 0)
            {
                readState.AccumulatedResponse.BeginWrite(readState.Buffer, 0, bytesRead, writeResult =>
                {
                    readState.AccumulatedResponse.EndWrite(writeResult);
                    readState.Response.BeginRead(readState.Buffer, 0, readState.Buffer.Length, ReadCallback, readState);
                }, null);
            }
            else
            {
                readState.AccumulatedResponse.Flush();
                readState.Response.Close();
                //pictureBox1.Image = Image.FromStream(readState.AccumulatedResponse);
            }
        }

        private static void UploadProgressCallback(object sender, UploadProgressChangedEventArgs e)
        {
            // Displays the operation identifier, and the transfer progress.
            Debug.WriteLine("{0}    uploaded {1} of {2} bytes. {3} % complete...",
                (string)e.UserState,
                e.BytesSent,
                e.TotalBytesToSend,
                e.ProgressPercentage);
        }
        private static void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            // Displays the operation identifier, and the transfer progress.
            Debug.WriteLine("{0}    downloaded {1} of {2} bytes. {3} % complete...",
                (string)e.UserState,
                e.BytesReceived,
                e.TotalBytesToReceive,
                e.ProgressPercentage);
        }

        public static void showusage()
        {
            Console.WriteLine("Attempts to GET a URL");
            Console.WriteLine("\r\nUsage:");
            Console.WriteLine("   ClientGetAsync URL");
            Console.WriteLine("   Example:");
            Console.WriteLine("      ClientGetAsync http://www.contoso.com/");
        }

        private static void RespCallback(IAsyncResult ar)
        {
            // Get the RequestState object from the async result.
            RequestState rs = null;

            try
            {
                // Get the RequestState object from the async result.
                rs = (RequestState)ar.AsyncState;

                // Get the WebRequest from RequestState.
                WebRequest req = rs.Request;

                // Call EndGetResponse, which produces the WebResponse object
                //  that came from the request issued above.
                WebResponse resp = req.EndGetResponse(ar);

                //  Start reading data from the response stream.
                Stream ResponseStream = resp.GetResponseStream();


                IsolatedStorageFile Store = IsolatedStorageFile.GetUserStoreForApplication();
                IsolatedStorageFileStream stream = Store.CreateFile(m_Filename);

                using (BinaryWriter sw = new BinaryWriter(stream))
                {

                    byte[] buffer = new byte[1024];
                    int offset = 0;
                    int Result = ResponseStream.Read(buffer, offset, buffer.Length);
                    while (Result > 0)
                    {
                        sw.Write(buffer, 0, Result);
                        Result = ResponseStream.Read(buffer, offset, buffer.Length);
                    }

                    sw.Close();
                }


                stream.Close();
            }
            catch (Exception ex)
            {
                Debugger.Trace(ex);
            }

            if (rs != null)
            {
                ClientGetAsync Client = (ClientGetAsync)rs.Client;
                Client.LoadCompleted(Client, EventArgs.Empty);
            }

        }
    }
}
