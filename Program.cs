using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace SerialCommunicationManager
{



    public class TcArchimedeExternalFunctions {

        public TcArchimedeExternalFunctions() { ; }

        [DllImport("ArchimedeInterfaceLibrary.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int fInitializeClient(StringBuilder pIPAddress, UInt16 pPort, StringBuilder pSenderId, UInt16 pLogLevel);

        [DllImport("ArchimedeInterfaceLibrary.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int fInitializeClientWithConfigurationFile(StringBuilder pSenderId, StringBuilder pConfigurationFile);

        [DllImport("ArchimedeInterfaceLibrary.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int fSendFormattedMeasureData(StringBuilder pMeasure, UInt16 pLenght);

        [DllImport("ArchimedeInterfaceLibrary.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int fRelease();


        public int fArchimedeInitializeRemoteCommunication(StringBuilder pIPAddress, UInt16 pPort, StringBuilder pSenderId, UInt16 pLogLevel)
        {
            return (fInitializeClient(pIPAddress, pPort, pSenderId, pLogLevel));
        }

        public int fArchimedeInitializeRemoteCommunication(StringBuilder pSenderId, StringBuilder pConfigurationFile)
        {
            return (fInitializeClientWithConfigurationFile(pSenderId, pConfigurationFile));
        }

        public int fArchimedeSendMeasureData(StringBuilder pMessage, UInt16 pLength)
        {
            return (fSendFormattedMeasureData(pMessage, pLength));
        }


        public int fArchimedeReleaseRemoteConnection(StringBuilder pSenderId, StringBuilder pConfigurationFile)
        {
            return (fRelease());
        }


    };
    public class TcSerialCommunicationManager : IDisposable
    {
        private const uint kJsonDataReceiveMode = 0;
        private const uint kValueListDataReceiveMode = 1;
        private SerialPort cmSerialCommunicationPort;
        private TcArchimedeExternalFunctions cmArchimedeExtFunctions;
        private UInt64 rDataPacketCount = 0;

        public TcSerialCommunicationManager(uint pDataReceiveMode, string pPortname, Parity pParityMode, uint pBaudRate, uint pBitsPerByte, StopBits pStopBits)
        {
            try
            {
                cmArchimedeExtFunctions = new TcArchimedeExternalFunctions();
                cmSerialCommunicationPort = new SerialPort();
                cmSerialCommunicationPort.PortName = pPortname;
                cmSerialCommunicationPort.Parity = pParityMode;
                cmSerialCommunicationPort.BaudRate = (int)pBaudRate;
                cmSerialCommunicationPort.DataBits = (int)pBitsPerByte;
                cmSerialCommunicationPort.StopBits = pStopBits;
                rDataPacketCount = 0;
                cmSerialCommunicationPort.Open();

                if (!cmSerialCommunicationPort.IsOpen)
                {
                    Console.WriteLine("The UART Communication is not enstablished");
                    throw new IOException("Open UART communication operation fails");
                }
                else
                {
                    Console.WriteLine("The UART Communication is enstablished");
                }


                if (pDataReceiveMode == kJsonDataReceiveMode) {
                    cmSerialCommunicationPort.DataReceived += fSerialReceiveJsonData;
                }
                else if (pDataReceiveMode == kValueListDataReceiveMode) {
                    cmSerialCommunicationPort.DataReceived += fSerialReceiveValueListData;
                }
                
            } catch (Exception e) { 
                throw e; 
            }
        }

        ~TcSerialCommunicationManager() {
            
            Dispose();
            if (cmSerialCommunicationPort != null)
            {
                cmSerialCommunicationPort.Close();
            }

        }

        void fSerialReceiveJsonData(object s, SerialDataReceivedEventArgs eventargs){
            string rData;
            int rResult = 0;
            JObject cJObject;

            rData = cmSerialCommunicationPort.ReadLine();
           
            try {
                cJObject = JObject.Parse(rData);
            } catch (Exception e) {
                return;
            }


            foreach (KeyValuePair<string, JToken> cKVPair in cJObject) {
                Measure cMeasure = new Measure();
                cMeasure.ReferenceID = "SensorId";
                cMeasure.ReferenceType = "Sensor";
                cMeasure.Timestamp = DateTime.Now;
                cMeasure.MeasureType = cKVPair.Key;
                cMeasure.Value = (double) cKVPair.Value.ToObject<int>();

                string rJsonMeasure = JsonConvert.SerializeObject(cMeasure);
                StringBuilder cJsonMessage = new StringBuilder(rJsonMeasure);
                UInt16 rLength = (UInt16)rJsonMeasure.Length;


                rResult = cmArchimedeExtFunctions.fArchimedeSendMeasureData(cJsonMessage, rLength);
                if (rResult < 0) {
                    throw new IOException("Send Measure operation fails");
                }
            }

            Console.WriteLine("Received Data Packet N. {0} on Port {1} ", ++rDataPacketCount, cmSerialCommunicationPort.PortName);
        }

        void fSerialReceiveValueListData(object s, SerialDataReceivedEventArgs eventargs)
        {
            string rData;
            string[] aValues;
            int rResult = 0;
            string[] aMeasureTypes = { "ACC_X", "ACC_Y", "ACC_Z", "GYRO_X", "GYRO_Y", "GYRO_Z" };
            int i = 0;

            rData = cmSerialCommunicationPort.ReadLine();
            aValues = rData.Split(',');

            foreach (string rValue in aValues) {
                Measure cMeasure = new Measure();
                cMeasure.ReferenceID = "SensorId";
                cMeasure.ReferenceType = "Sensor";
                cMeasure.Timestamp = DateTime.Now;
                cMeasure.MeasureType = aMeasureTypes[i++];

                double rTempValue;
                double.TryParse(rValue, out rTempValue);
                cMeasure.Value = rTempValue;

                string rJsonMeasure = JsonConvert.SerializeObject(cMeasure);
                StringBuilder cJsonMessage = new StringBuilder(rJsonMeasure);
                UInt16 rLength = (UInt16)rJsonMeasure.Length;


                rResult = cmArchimedeExtFunctions.fArchimedeSendMeasureData(cJsonMessage, rLength);
                if (rResult < 0)
                {
                    throw new IOException("Send Measure operation fails");
                }
            }

            Console.WriteLine("Received Data Packet N. {0} on Port {1} ", ++rDataPacketCount, cmSerialCommunicationPort.PortName);
        }

        public void Dispose() {
            if (cmSerialCommunicationPort != null)
            {
                cmSerialCommunicationPort.Dispose();
            }
        }
    };


    class Program
    {

        private const int kLaunchSerialCommunicationManager_fail = -1;
        private const int kLaunchSerialCommunicationManager_success = 0;

        private const int kArchimedeConnect_fail = -2;
        private const int kArchimedeConnect_success = 0;

        private const int kSetArguments_WrongNumberOfArgs_fail = -3;
        private const int kSetArguments_InvalidArg_fail = -4;
        private const int kSetArguments_success = 0;

        private static TcArchimedeExternalFunctions cArchimedeExtFunctions = new TcArchimedeExternalFunctions(); //Contains ArchimedeLibraryInterface.dll wrapped functions
        private static string rmProgramName = "UART Communication Manager"; 
        private static bool rmIsRunning = true; //while the program is running
        private static uint rmSleepTimeInterval = 1000; //in milliseconds


        public static void fKeepInIdle(bool pIsRunning = true, uint pSleepTimeInterval = 1000)
        {
            Console.WriteLine("KeepInIdle() entry");
            while (pIsRunning)
            {
                Thread.Sleep((int)pSleepTimeInterval);
            }
            Console.WriteLine("KeepInIdle() exit");
            return;
        }
        public static int fLaunchSerialCommunicationManager(uint pDataReceiveMode, string pPortname, Parity pParity, uint pBaudrate, uint pBitsPerByte, StopBits pStopbits) {
            try{
                TcSerialCommunicationManager cComManager = new TcSerialCommunicationManager(pDataReceiveMode, pPortname, pParity, pBaudrate, pBitsPerByte, pStopbits);
                Console.WriteLine("Open serial communication with success");
                return(0);
            } catch (Exception e) {
                Console.WriteLine("Open serial communication fails with Exception Message {0}", e.Message);
                return(-1);
            }
        }
        public static int fArchimedeConnect(string pSenderId, string pConfigurationFile) {
        
            try
            {
                int rResult = cArchimedeExtFunctions.fArchimedeInitializeRemoteCommunication(new StringBuilder(pSenderId), new StringBuilder(pConfigurationFile));
                Console.WriteLine("Initialize remote communication with success");
                return (0);
            }
            catch (Exception e)
            {
                Console.WriteLine("Initialize remote communication fails with Exception Message {0}", e.Message);
                return(-1);
            }


        }
        public static int fArchimedeConnect(string pIPAddress, string pSenderId, UInt16 pPort, UInt16 pLogLevel)
        {
            try
            {
                int rResult = cArchimedeExtFunctions.fArchimedeInitializeRemoteCommunication(new StringBuilder(pIPAddress), pPort, new StringBuilder(pSenderId), pLogLevel);
                if (rResult == 0) {
                    return (kArchimedeConnect_success);
                } else {
                    return (kArchimedeConnect_fail);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Initialize remote communication fails with Exception Message {0}", e.Message);
                return(kArchimedeConnect_fail);
            }
        }
        public static int fSetArguments(string [] args, out uint [] pReceiveDataModes, out string [] pPortname, out uint [] pBaudrate) {
            
            uint n = (uint)(args.Length / 3);
            pReceiveDataModes = new uint[(int)n];
            pPortname = new string[(int)n];
            pBaudrate = new uint[(int)n];


            if (!(args.Length >= 3 && args.Length % 3 == 0))
            {
                Console.WriteLine("Wrong number of argument");
                return (kSetArguments_WrongNumberOfArgs_fail);
            }

            for (int i = 0;  i < args.Length; i += 3) {
                if (!uint.TryParse(args[i], out pReceiveDataModes[i / 3])){
                    Console.WriteLine("Argument nr. {0} is invalid", i);
                    return (kSetArguments_InvalidArg_fail);
                }

                pPortname[i / 3] = args[i+1];
                if (!uint.TryParse(args[i+2], out pBaudrate[i / 3])){
                    Console.WriteLine("Argument nr. {0} is invalid", i + 2);
                    return (kSetArguments_InvalidArg_fail);
                }
            }

            return (0);
        }
        
        static void Main(string[] args) {

            int rResult = 0;
            string rIPAddress = "172.18.255.10";
            string rSenderId = "Serial Communication Manager";
            UInt16 rPort = 50085;
            UInt16 rLogLevel = 0;

            uint[] aReceiveDataModes;
            string [] aPortnames;
            uint [] aBaudrates;
            uint rBitsPerByte = 8;
            Parity rParity = Parity.None;
            StopBits rStopbits = StopBits.One;


           
            if ((rResult = fSetArguments(args, out aReceiveDataModes, out aPortnames, out aBaudrates)) < 0) { 
                return; 
            }

            if ((rResult = fArchimedeConnect(rIPAddress, rSenderId, rPort, rLogLevel)) < 0){
                return;
            }

            for (int i = 0; i < aPortnames.Length; i++) {
                if ((rResult = fLaunchSerialCommunicationManager(aReceiveDataModes[i], aPortnames[i], rParity, aBaudrates[i], rBitsPerByte, rStopbits)) < 0){
                    return;
                }
            }
            
            fKeepInIdle(rmIsRunning, rmSleepTimeInterval);

        }


    }
}
