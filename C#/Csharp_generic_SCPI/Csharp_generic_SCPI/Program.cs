using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csharp_generic_SCPI
{
    class Program
    {
        static void RunExample(SCPIConnection connection)
        {
            //Query the instrument type
            string idnResponse = connection.Query("*IDN?");
            System.Console.WriteLine("*IDN query response is: " + idnResponse);

            //Write the settings 
            connection.Write(":SENS1:FREQ:STAR 2GHZ");    
            connection.Write(":SENS1:FREQ:STOP 4GHZ");
            connection.Write(":SENS1:SWE:POIN 21");
            connection.Write("SENS1:HOLD:FUNC HOLD");

            string startFreq = connection.Query(":SENS1:FREQ:STAR?");
            string stopFreq = connection.Query(":SENS1:FREQ:STOP?");
            string noPoints = connection.Query(":SENS1:SWE:POIN?");

            System.Console.WriteLine("Start frequency is :" + startFreq + " Hz");
            System.Console.WriteLine("Stop frequency is :" + stopFreq + " Hz");
            System.Console.WriteLine("Number of points is: " + noPoints);

            connection.Write("TRIG:SING");
            string opcDone = connection.Query("*OPC?");
            System.Console.WriteLine("Sweep done " + 
                Convert.ToString(Convert.ToBoolean(Convert.ToInt32(opcDone))));

            //Get measurements data
            string fTraceData = connection.Query("CALC1:DATA:FDAT?");
            string sTraceData = connection.Query("CALC1:DATA:SDAT?");

            System.Console.WriteLine("Formatted data:");
            System.Console.WriteLine(fTraceData);

            System.Console.WriteLine("S parameters data:");
            System.Console.WriteLine(sTraceData);
            
        }
        static void Main(string[] args)
        {
            String resourceNameRawSocket = "TCPIP0::127.0.0.1::5001::SOCKET";
            String resourceNameVXI = "TCPIP0::127.0.0.1::inst0::INSTR";
            
            try
            {
                System.Console.WriteLine("===================== TCP Raw Sockets =====================");
                RawSocketsConnection rawSocketsConnection = new RawSocketsConnection(resourceNameRawSocket);
                rawSocketsConnection.Connect(10000);
                RunExample(rawSocketsConnection);
                rawSocketsConnection.Disconnect();
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Unexpected exception : {0} when using TCP Raw Sockets on: {1}",
                    e.ToString(), resourceNameRawSocket);
            }            
            
            try
            {
                System.Console.WriteLine("===================== NI Visa libraries & VXI Protocol =====================");
                NIVXI11Connection vxiConnection = new NIVXI11Connection(resourceNameVXI);
                vxiConnection.Connect(10000);
                RunExample(vxiConnection);
                vxiConnection.Disconnect();
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Unexpected exception : {0} when using NI Visa libraries and VXI Protocol on : {1} ",
                    e.ToString(), resourceNameVXI);
            }           
        }
    }
}
