using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Csharp_generic_SCPI
{
    class RawSocketsConnection : SCPIConnection
    {
        private string ResourceName;
        private string IpAddress;
        private int PortNumber;
        private Socket Session;
        private string EndChar;

        public RawSocketsConnection(string resourceName)
        {
            if (String.IsNullOrEmpty(resourceName))
            {
                throw new Exception("Resource name could not be empty or null!");
            }
            ResourceName = resourceName;

            IpAddress = resourceName.Substring(resourceName.IndexOf("::") + 2,
                resourceName.LastIndexOf("::") - resourceName.IndexOf("::"));
            if (String.IsNullOrEmpty(IpAddress))
            {
                throw new Exception("Faulty resource name!");
            }

            string portNumerString = IpAddress.Substring(IpAddress.IndexOf("::")
                + 2, IpAddress.LastIndexOf("::") - IpAddress.IndexOf("::") - 2);
            if (String.IsNullOrEmpty(portNumerString))
            {
                throw new Exception("Unable to extract the port number!");
            }
            PortNumber = Convert.ToInt32(portNumerString);

            IpAddress = IpAddress.Substring(0, IpAddress.IndexOf("::"));
            if (String.IsNullOrEmpty(IpAddress))
            {
                throw new Exception("Unable to identify the IP address!");
            }
            EndChar = "\n";
        }

        public void Connect(int timeOutMs/*string resourceName*/)
        {
            // Connect to a remote device.  
            try
            {
                IPAddress ipAddress = IPAddress.Parse(IpAddress);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, PortNumber);

                // Create a TCP/IP  socket.  
                Session = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                Session.ReceiveTimeout = timeOutMs;
                Session.SendTimeout = timeOutMs;

                // Connect the socket to the remote endpoint 
                try
                {
                    Session.Connect(remoteEP);
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0} when trying to initialize socket connection {1} ",
                        ane.ToString(), ResourceName);
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0} when trying to initialize socket connection {1} ",
                        se.ToString(), ResourceName);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0} when trying to initialize socket connection {1} ", 
                        e.ToString(), ResourceName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0} when trying to create socket session to {1}",
                    e.ToString(), ResourceName);
            }
        }
        public void Disconnect()
        {
            try
            {
                // Release the socket.  
                Session.Shutdown(SocketShutdown.Both);
                Session.Close();
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0} when trying to disconnect socket session from {1}", 
                    ane.ToString(), ResourceName);
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0} when trying to disconnect socket session from {1}",
                     se.ToString(), ResourceName);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0} when trying to disconnect socket session from ", 
                    e.ToString(), ResourceName);
            }
        }
        public string Query(string command)
        {
            string response = "";
            Write(command);
            try
            {
                //Read first 1 character
                byte[] firstCharBuffer = new byte[Constants.OneByte];
                Session.Receive(firstCharBuffer, 1, SocketFlags.None);
                response = System.Text.Encoding.UTF8.GetString(firstCharBuffer);

                if (firstCharBuffer[0] == Constants.StartBlockChar)
                {
                    //Block section structure
                    //#512808<DAB1>...<DAB12808>
                    //The header in the example above is comprised of 7 characters(#512808).
                    //It begins with the pound character (#). The next character (5) indicates
                    //there are 5 digits to follow that indicate the number of dataBuffer being 
                    //transmitted(12808).The next five characters(12808) indicate the number 
                    //of data dataBuffer being transmitted immediately after the header.

                    //Read next 1 character
                    byte[] sizeBuffer1 = new byte[Constants.OneByte];
                    Session.Receive(sizeBuffer1, Constants.OneByte, SocketFlags.None);
                    int blockDescriptionLen = sizeBuffer1[0] - 0x30;

                    //Read data block size
                    byte[] sizeBuffer2 = new byte[blockDescriptionLen];
                    Session.Receive(sizeBuffer2, blockDescriptionLen, SocketFlags.None);
                    int actualBlockDataLen = Int32.Parse(System.Text.Encoding.UTF8.GetString(sizeBuffer2)) + 1;

                    //Read data block
                    byte[] dataBuffer = new byte[actualBlockDataLen];
                    byte[] finalDataBuffer = new byte[actualBlockDataLen];
                    int bytesReceived = 0;
                    while (bytesReceived < actualBlockDataLen)
                    {
                        int oldBytesReceived = bytesReceived;
                        int receivedBytes = Session.Receive(dataBuffer, actualBlockDataLen - bytesReceived, SocketFlags.None);
                        System.Buffer.BlockCopy(dataBuffer, 0, finalDataBuffer, oldBytesReceived, receivedBytes);
                        bytesReceived += receivedBytes;
                    }
                    response = System.Text.Encoding.UTF8.GetString(finalDataBuffer).TrimEnd(); ;
                }
                else
                {
                    byte[] dataBuffer = new byte[Constants.BufferLength];
                    int bytesReceived = Session.Receive(dataBuffer);
                    response += Encoding.ASCII.GetString(dataBuffer, 0, bytesReceived).TrimEnd("\r\n".ToCharArray());
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0} when trying to receive response from command {1}",
                    se.ToString(), command);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception :{0} when trying to receive response from command {1}",
                    e.ToString(), command);
            }
            return response;
        }

        public void Write(string command)
        {
            try
            {
                byte[] message = Encoding.ASCII.GetBytes(command + EndChar);        
                int bytesSent = Session.Send(message);

                if (bytesSent != message.Length)
                {
                    throw new Exception("Failed to write the entire message!");
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0} when trying to write {1}",
                    se.ToString(), command);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0} when trying to write {1}",
                    e.ToString(), command);
            }
        }

        public void SetTimeout(int timeOutMs)
        {
            Session.ReceiveTimeout = timeOutMs;
            Session.SendTimeout = timeOutMs;
        }
    }
}
