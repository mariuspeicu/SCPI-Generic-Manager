========================================================================
						Cplusplus_generic_SCPI.sln
========================================================================

Cplusplus_generic_SCPI.sln is a generic C++ example of a SCPI session 
manager which can be used to send SCPI commands to instruments. 
It can be used with both VXI-11 protocol and Raw Sockets protocol. 
For VXI-11 implementation it uses NI's VISA libraries (visa32) 
This porgram can be used as a starting point for implementing an 
protocol-indepentend remote test application. 


USAGE
	There is a generic interface called SCPIConnection which defines 
	the available operations for a SCPI session
		- Connect() //Connects to the instrument
        - Disconnect() //Disconnects from the instrument
		- Write(command) //Sends a command to the instrument
		- Query(queryCommand) //Sends a query command and reads the 
			response from the instrument 

	This interface is implemented by NIVXI11Connection and 
	RawSocketsConnection. 
	
	The resource name for VXI-11 sessions has the structure 
		"TCPIP0::127.0.0.1::inst0::INSTR"
		
	The resource name for Raw Sockets sessions has the structure
		"TCPIP0::127.0.0.1::5001::SOCKET"
	

COMPILE
	Open Cplusplus_generic_SCPI.sln.sln project in MVS2015 or newer and build it

RUNNING
	In order to run it one has to have NI VISA runtime libraries 
	(visa32.dll) on the PC. They come with NI VISA software. 
	

/////////////////////////////////////////////////////////////////////////////
