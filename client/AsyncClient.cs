using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace client
{

    // State object for receiving data from remote device.  
    public class StateObject
    {
        // Client socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 256;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
    }

    class AsyncClient
    {
        // The port number for the remote device.  
        // private const int port = 11000;
        // private const int port = 5555;

        // ManualResetEvent instances signal completion.  
        private static ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        private static Socket client = null;   
        private static NetworkStream stream = null;   

        // The response from the remote device.  
        private static String response = String.Empty;

        private static bool isconnected = false;

        public static void connect(string address, int port)
        {
            IPAddress ipAddress = IPAddress.Parse(address);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.  
            // Socket client = new Socket(ipAddress.AddressFamily,
            client = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Connect to the remote endpoint.  
            client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
            connectDone.WaitOne();
            Console.WriteLine("Socket connected done");
        }

        public static void init()
        {
            var size = 1024;
            var buffer = new byte[size];
            // Task.Run(async () =>
            Task.Factory.StartNew(() =>
            {
                while(true)
                {
                    if(isconnected)
                    {
                        try
                        {
                            if(stream.DataAvailable)
                            {
                                var bytesRead = client.Receive(buffer);
                                var actualData = new byte[bytesRead];
                                Array.Copy(buffer, actualData, bytesRead);
                                OnDataReceived(actualData);
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            });
        }

        static void OnDataReceived(byte[] data)
        {
            Task.Factory.StartNew(() =>
            {
                Console.WriteLine("[client] received : {0}", Encoding.Default.GetString(data));
            });
        }

        static void OnDisconnected(Socket s)
        {
            if(s != null)
            {
                s.Shutdown(SocketShutdown.Both);
                s.Close();
            }
        }


        public static void close()
        {
            // Release the socket.  
            try
            {
                stream.Close();
                client.Shutdown(SocketShutdown.Both);
                client.Close();
                client = null;
                isconnected = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        public static void send(ref byte[] data)
        {
            // Connect to a remote device.  
            try
            {

                Send(client, data);
                sendDone.WaitOne();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                isconnected = true;
                stream = new NetworkStream(client);

                // Signal that the connection has been made.  
                connectDone.Set();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void Receive(Socket client)
        {
            try
            {
                // Create the state object.  
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.  
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // 20180727

                // Retrieve the state object and the client socket   
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Get the rest of the data.  
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.  
                    if (state.sb.Length > 1)
                    {
                        response = state.sb.ToString();
                    }
                    // Signal that all bytes have been received.  
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        // private static void Send(Socket client, String data)
        private static void Send(Socket client, byte[] byteData)
        {
            // Convert the string data to byte data using ASCII encoding.  
            // byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.  
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
