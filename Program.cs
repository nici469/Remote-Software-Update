using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Net;

namespace Remote_Software_Update
{
    class Program
    {
        /// <summary>
        /// an ever-increasing number to store the current software version
        /// </summary>
        const int versionNumber = 4;
        //static string oldFileNme;
        static async Task<int> Main(string[] args)
        {
            //check if the input argument is not empthy and then if
            //first input argument indicates a version request
            if (args.Length>0 && (args[0] == "version" || args[0] == "-v")) { Console.WriteLine ("Software version: "+versionNumber); return versionNumber; }
            
            else if(args.Length > 0)
            { //if there are arguments but the first one isnt _version_, then it must be the oldFileName, 
              //and this executable must be the newFile about to dispose of the old
                string oldProcess = args[0];

                //finalise update as an asynchronus task
                var FinaliseUpdateAsync = FinaliseUpdate(oldProcess);

                
            }

            var currentProvess = Process.GetCurrentProcess().ProcessName;
            Console.WriteLine("name of current process is: " + currentProvess);

            ContinueNormalOperation();

            Console.ReadKey(true);
            return 0;
        }

        /// <summary>
        /// carries out the operation of closing and deleting the old file once the new one,
        /// which should be the current instance, is running.
        /// This method is made asynchronus because "new thread" functions cannot receive parameters: it could habe simply benn executed 
        /// on a separate thread
        /// </summary>
        static  async Task FinaliseUpdate(string oldProcess) {
            Console.WriteLine("Attempting to Finalise Update");
            //still to be populated
            Process[] oldFileProcess = Process.GetProcessesByName(oldProcess);

            //close the old process
            foreach (Process process in oldFileProcess)
            {
                Console.WriteLine("attempting to kill " + process.ProcessName);
                process.Kill();
                Console.WriteLine(process.ProcessName+" process killed, waiting for process exit");
                process.WaitForExit();
                Console.WriteLine("attempting to dispose " + process.ProcessName);
                process.Dispose();
            }

            //delete the old exe file
            Console.WriteLine("Attempting to delete old executable");
            System.IO.File.Delete(oldProcess + ".exe");
            Console.WriteLine("old file deleted");
            
        }

        /// <summary>
        /// carry out th primary purpose of the software
        /// </summary>
        static void ContinueNormalOperation() 
        {
            int count = 0;//to be incremented approximately every 1 minute
            while (true)
            {
                //check for update every 2 hours on a separate thread
                if (count > 120) {
                    ThreadStart newStart = new ThreadStart(CheckForUpdate);
                    Thread GetUpdate = new Thread(newStart);
                    GetUpdate.Name = "GetUdate";
                    GetUpdate.Start();
                }

                //download the uplink data necessary for the primary software functionss
                ThreadStart uplinkStart = new ThreadStart(GetUplinkData);
                Thread getUplink = new Thread(uplinkStart);
                getUplink.Start();

                Thread.Sleep(60000);//sleep the main thread for 1 minute
            }
        }

        /// <summary>
        /// check if there are any update available from the cloud server
        /// UP NEXT
        /// </summary>
        static void CheckForUpdate()
        {

        }

        /// <summary>
        /// method for download the regular uplink data necessary for the primary functioning of this
        /// software
        /// </summary>
        static void GetUplinkData() { }

    }
}
