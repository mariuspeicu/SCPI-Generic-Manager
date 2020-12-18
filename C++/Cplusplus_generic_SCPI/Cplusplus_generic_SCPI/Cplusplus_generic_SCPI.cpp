// Cplusplus_generic_SCPI.cpp : Defines the entry point for the console application.
//
#include <iostream>
#include "VXIConnection.h"
#include "RawSocketConnection.h"
#include "CustomException.h"


static void printExit() {
	printf("Press any key to exit...");
	char keyBuffer[100];
	scanf_s("%c", keyBuffer, 100);
}


static void SendCommand(SCPIConnection* connection, std::string cmd) 
{
	try
	{
		connection->Send(cmd);
	}
	catch (CustomException& exception)
	{
		std::cout << exception.toString() << std::endl;
		return;
	}
}

static std::string QueryCommand(SCPIConnection* connection, std::string cmd)
{
	std::string response;
	try
	{
		response = connection->Query(cmd);
	}
	catch (CustomException& exception)
	{
		std::cout << exception.toString() << std::endl;
	}
	return response;
}

static void checkVNA(SCPIConnection* connection) 
{
	std::string idnResponse = QueryCommand(connection, "*IDN?");
	std::cout << "*IDN? = " << idnResponse << std::endl;

	std::string opcResponse = QueryCommand(connection, "*OPC?");
	std::cout << "*OPC? = " << opcResponse << std::endl;

	//std::string fData = QueryCommand(connection, ":CALC1:PAR1:DATA:FDAT?");
	//std::cout << fData << std::endl;

}

static void presetVNA(SCPIConnection* connection)
{
	SendCommand(connection, ":SYST:PRES");
	std::cout << "Presetting..." << std::endl;
	std::string opcResponse = QueryCommand(connection, "*OPC?");
	std::cout << "*OPC? = " << opcResponse << std::endl;
}

static void doSetup(SCPIConnection* connection)
{
	
}

static void doCalibration(SCPIConnection* connection)
{
	std::string rtn;

	//Set calibration method to SOLT
	SendCommand(connection, ":SENS1:CORR:COLL:METH SOLT");
	rtn = QueryCommand(connection, ":SENS1:CORR:COLL:METH?");
	std::cout << "Calibration method is " << rtn << std::endl;

	//Set Full SOL calibration on Port1
	SendCommand(connection, ":SENS1:CORR:COEF:PORT1:FULL1");
	rtn = QueryCommand(connection, ":SENS1:CORR:COLL:TYP?");
	std::cout << "Calibration type is " << rtn << std::endl;

	//Set calibration coefficients  (Cal Kit)
	SendCommand(connection, ":SENS1:CORR:COLL:PORT1:CONN CFNT");
	rtn = QueryCommand(connection, ":SENS1:CORR:COLL:PORT1:CONN?");
	std::cout << "Calibration kit at port 1 is " << rtn << std::endl;

	char keyBuffer[100];
	//Connect addapters and do the measurements
	printf("Connect LOAD and press enter...");
	scanf_s("%c", keyBuffer, 100);
	SendCommand(connection, ":SENS1:CORR:COLL:PORT1:LOAD");

	printf("Connect OPEN and press enter...");
	scanf_s("%c", keyBuffer, 100);
	SendCommand(connection, ":SENS1:CORR:COLL:PORT1:OPEN");

	printf("Connect SHORT and press enter...");
	scanf_s("%c", keyBuffer, 100);
	SendCommand(connection, ":SENS1:CORR:COLL:PORT1:SHORT");

	//Save the calibration
	SendCommand(connection, ":SENS1:CORR:COLL:SAVE");
	rtn = QueryCommand(connection, ":SENS1:CORR:STAT?");
	std::cout << "Calibration state is " << rtn << std::endl;

}

static void doMeasurement(SCPIConnection* connection)
{
	std::string rtn;

	

	//Store Formatted data 
	SendCommand(connection, ":MMEM:STOR 'C:\\AnritsuVNA\\Measurement.tdf'");

	//Store SParameter
	SendCommand(connection, ":MMEM:STOR 'C:\\AnritsuVNA\\Measurement.s1p'");
}


static void runTest(SCPIConnection* session) 
{
	try
	{
		session->Connect();
	}
	catch (CustomException& exception)
	{
		std::cout << exception.toString() << std::endl;
		printExit();
		return;
	}
	std::string response;
	//Set Start Frequency to 1GHz
	SendCommand(session, ":SENS1:FREQ:STAR 1GHZ");
	response = QueryCommand(session, ":SENS1:FREQ:STAR?");
	std::cout << "Start Frequency is " << response << std::endl;

	//Set Stop Frequency to 1GHz
	SendCommand(session, ":SENS1:FREQ:STOP 3GHZ");
	response = QueryCommand(session, ":SENS1:FREQ:STOP?");
	std::cout << "Stop Frequency is " << response << std::endl;

	//Set number of points to 21
	SendCommand(session, ":SENS1:SWE:POIN 21");
	response = QueryCommand(session, ":SENS1:SWE:POIN?");
	std::cout << "Number of points is " << response << std::endl;

	//Set sweep mode to single sweep and hold
	SendCommand(session, ":SENS1:HOLD:FUNC HOLD");
	response = QueryCommand(session, ":SENS1:HOLD:FUNC?");
	std::cout << "Sweep mode is " << response << std::endl;

	//Do a singe sweep
	SendCommand(session, ":TRIG:SING");
	std::cout << "Sweeping... " << response << std::endl;
	response = QueryCommand(session, "*OPC?");
	std::cout << "Sweep done is " << response << std::endl;

	//Get Formatted Data LOGMAG
	std::string fData = QueryCommand(session, ":CALC1:PAR1:DATA:FDAT?");
	std::cout << "======== FORMTATTED DATA ========" << std::endl << fData << std::endl;

	//Get SParameter
	std::string sData = QueryCommand(session, ":CALC1:PAR1:DATA:SDAT?");
	std::cout << "======== S DATA ========" << std::endl << sData << std::endl;

}

int main()
{	
	VXIConnection vxi("TCPIP0::127.0.0.1::inst0::INSTR");
	RawSocketConnection rawSocket("TCPIP0::127.0.0.1::5001::SOCKET");	
	std::cout << "===================== VXI-11 Protocol =====================" << std::endl;
	//runTest(&vxi);
	std::cout << "===================== TCP Raw Sockets =====================" << std::endl;
	runTest(&rawSocket);
	
    return 0;
}

