using System;
using NationalInstruments.VisaNS;

namespace Csharp_generic_SCPI
{    
    class NIVXI11Connection : SCPIConnection
    {
        private MessageBasedSession Session;
        private string EndChar;
        private string ResourceName;        

        public NIVXI11Connection(string resourceName) 
        {
            if (String.IsNullOrEmpty(resourceName))
            {
                throw new Exception("Resource name could not be empty or null!");
            }
            Session = null;
            EndChar = "";
            ResourceName = resourceName;
        }
        public void Connect(int timeOutMs)
        {
            try
            {
                if (ResourceName.Contains("::SOCKET"))
                {
                    EndChar = "\n";
                }
                if (Session != null)
                {
                    Disconnect();
                }

                Session = (MessageBasedSession)ResourceManager.GetLocalManager().Open(ResourceName);
                Session.Timeout = timeOutMs;

                // Setup termination
                switch (Session.HardwareInterfaceType)
                {
                    case HardwareInterfaceType.Tcpip:
                        Session.TerminationCharacterEnabled = true;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0} when trying to connect to {1}",
                    e.ToString(), ResourceName);
            }
        }
        public void Disconnect()
        {
            try
            {
                if (Session != null)
                {
                    Session.Dispose();
                    Session = null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0} when trying to diconnect from {1}",
                    e.ToString(), ResourceName);
            }
        }
        public string Query(string command)
        {
            string response = "";
            try
            {
                response = Session.Query(command + EndChar, Constants.OneByte);
                if (response != Convert.ToString(Constants.StartBlockChar))
                {
                    byte[] dataBuffer = new byte[Constants.BufferLength];
                    dataBuffer = Session.ReadByteArray(Constants.BufferLength);
                    response += System.Text.Encoding.UTF8.GetString(dataBuffer);
                }
                else
                {
                    //Block section structure
                    //#512808<DAB1>...<DAB12808>
                    //The header in the example above is comprised of 7 characters(#512808).
                    //It begins with the pound character (#). The next character (5) indicates
                    //there are 5 digits to follow that indicate the number of dataBuffer being 
                    //transmitted(12808).The next five characters(12808) indicate the number 
                    //of data dataBuffer being transmitted immediately after the header.

                    //Ignore the termination character
                    //Read until all the block is finished                 
                    Session.TerminationCharacterEnabled = false;

                    //Read first 1 character
                    //Length of the block description section
                    byte[] sizeBuffer1 = new byte[Constants.OneByte];
                    sizeBuffer1 = Session.ReadByteArray(Constants.OneByte);
                    int blockDescriptionLen = sizeBuffer1[0] - 0x30;

                    //Read block description length
                    byte[] sizeBuffer2 = new byte[blockDescriptionLen];
                    sizeBuffer2 = Session.ReadByteArray(blockDescriptionLen);
                    int actualBlockDataLen = Int32.Parse(System.Text.Encoding.UTF8.GetString(sizeBuffer2)) + 1;

                    // Read block data
                    byte[] dataBuffer = new byte[actualBlockDataLen];
                    dataBuffer = Session.ReadByteArray(actualBlockDataLen);

                    response = System.Text.Encoding.UTF8.GetString(dataBuffer);
                    Session.TerminationCharacterEnabled = false;
                }
                response = response.TrimEnd();
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0} when trying to query {1}",
                    e.ToString(), command);
            }
            return response;
        }
        public void Write(string command)
        {
            try
            {
                Session.Write(command + EndChar);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0} when trying to write {1}",
                    e.ToString(), command);
            }
        }

        public void SetTimeout(int timeOutMs)
        {
            Session.Timeout = timeOutMs;
        }
    }
}
