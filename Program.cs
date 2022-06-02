using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.IO;

namespace Remote_Software_Update
{
    class Program
    {
        /// <summary>
        /// an ever-increasing number to store the current software version
        /// </summary>
        const int versionNumber = 4;
        ///static string oldFileNme;
        static async Task<int> Main(string[] args)
        {
            ///check if the input argument is not empthy and then if
            ///first input argument indicates a version request
            if (args.Length>0 && (args[0] == "version" || args[0] == "-v")) { Console.WriteLine ("Software version: "+versionNumber); return versionNumber; }
            
            else if(args.Length > 0)
            { ///if there are arguments but the first one isnt _version_, then it must be the oldFileName, 
              ///and this executable must be the newFile about to dispose of the old
                string oldProcess = args[0];

                ///finalise update as an asynchronus task
                var FinaliseUpdateAsync = FinaliseUpdate(oldProcess);

                
            }

            ///var currentProvess = Process.GetCurrentProcess().ProcessName;
            ///Console.WriteLine("name of current process is: " + currentProvess);
            
            ///variables to store the existence of a verified update, and hence, its versioncode
            bool verifiedUpdateExists = false;
            int newVersionCode = 0;

            CheckForLocalUpdate(ref verifiedUpdateExists, ref newVersionCode);


            ///if a verified update exists, Execute the update, otherwise continue regular software operation
            if (verifiedUpdateExists)
            {
                ExecuteUpdate(newVersionCode);
            }
            else
            {
                ContinueNormalOperation();
            }
            return 0;

            
            Console.ReadKey(true);
            return 0;
        }

        /// <summary>
        /// Moves any available verified update file from the downloads folder to the current app directory and 
        /// executes the update file, then waits for the update file to terminate the current instance of app execution..
        /// it throws an exception if current instance is not terminated after sometime......EMPTY
        /// </summary>
        /// <param name="newVersionCode"></param>
        public static void ExecuteUpdate(int newVersionCode) { }

        /// <summary>
        /// Checks for any downloaded local update.exe file, at produces the version code of the 
        /// update file....CODED
        /// </summary>
        /// <param name="verifiedUpdateExists"></param>
        /// <param name="newVersionCode"></param>
        public static void CheckForLocalUpdate(ref bool verifiedUpdateExists, ref int newVersionCode) {
            //if Downloads folder does not exist in the current directory, abandon update check and assume
            //no verifiable update exists
            if (!Directory.Exists("Downloads"))
            {
                verifiedUpdateExists = false;
                return;
            }
            //if no update file exists in the Downloads folder, assume no verifiable update exists.
            //abandon update check
            else if (!File.Exists("Downloads/Update.exe"))
            {
                verifiedUpdateExists = false;
                return;
            }
            //an update.exe file must have been found at this point

            //attempt to get the version code of the update file
            try
            {
                newVersionCode = AppDomain.CurrentDomain.ExecuteAssembly("Downloads/Update.exe",new string[] { "version"});
            }
            catch(Exception e)
            {///if the attempt to get version code fails, the update.exe file is not a valid update file
                File.Delete("Downloads/Update.exe");///delete the invalid update file so it isnt processed at next software startup
                verifiedUpdateExists = false;
                return;
            }
            //if the attempt to get the update.exe versioncode succeeded, compare the  new versioncode with the versioncode
            //of the current running executable

            if (newVersionCode > versionNumber)
            {
                verifiedUpdateExists = true;
                return;
            }
            else
            {
                verifiedUpdateExists=false;
                File.Delete("Downloads/Update.exe");///delete the invalid update file so it isnt processed at next software startup
                return;
            }


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
            Console.WriteLine("normal operation commenced");
            int count = 0;//to be incremented approximately every 1 minute
            
            StartUpdateThread();//Update thread is started at first execution and then every 2 hours
            
            while (true)
            {
                //check for update every 2 hours on a separate thread
                if (count >= 120) {
                    count = 0;//reset the counter every time it gets to 120(2hours)
                    StartUpdateThread();
                }

                //download the uplink data necessary for the primary software functionss
                ThreadStart uplinkStart = new ThreadStart(GetUplinkData);
                Thread getUplink = new Thread(uplinkStart);
                getUplink.Start();

                Thread.Sleep(60000);//sleep the main thread for 1 minute
                count++;
            }
        }

        /// <summary>
        /// the download url of the file containing software history, the download links of software versions and
        /// their corresponding hashes and id.... not yet defined
        /// </summary>
        const string linkHistoryURL = "";//create this on a cloud server such as dropbox

        /// <summary>
        /// starts a new thread to handle all software update download actions
        /// </summary>
        static void StartUpdateThread()
        {
            Console.WriteLine("Starting update thread");
            ThreadStart newStart = new ThreadStart(CheckForUpdate);
            Thread GetUpdate = new Thread(newStart);
            GetUpdate.Name = "GetUdate";
            GetUpdate.Start();
        }

        /// <summary>
        /// check if there are any online update available from the cloud server defined by the linkHistoryURL
        /// 
        /// </summary>
        static void CheckForUpdate()
        {
            

            //create a Downloads folder in the curent directory if it doesnt exist
            if (!Directory.Exists("Downloads")) { 
                Directory.CreateDirectory("Downloads");
                Console.WriteLine("Downloads folder created");
            }

            Console.WriteLine("Attempting to check for Update");
            WebClient client = new WebClient();

            string updateData;//to store the downloaded string containing the latest software info

            try
            {
                
                
                updateData = client.DownloadString(linkHistoryURL);
                Console.WriteLine(updateData);
                
                UpdateVersion(updateData);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("could not check for update");
                return;
            }
            //updateData string download must have been successful at this point
            var stringprocessor = new ProcessString();

            string[] updateDataSeparated = stringprocessor.SeparateLines(updateData, ',');

            ///versioncode should be the first element of the downloaded updateData string from the linkHistoryFile
            int latestVersionCode = int.Parse(updateDataSeparated[0]);

            //if the latest update data doesnt contain any higher version code, no relevant update exists
            if (latestVersionCode <= versionNumber)
            {
                return;
            }
            //code blocks below are executed only if latestVersion code is greater then that of the current exe instance

            //check if an Update.exe file already exists in the downloads folder, and verify if its version code 
            //matches with what was update from the linkHistoryURL

            if (File.Exists("Downloads/Update.exe"))
            {
                int exeVersionCode = AppDomain.CurrentDomain.ExecuteAssembly("Downloads/Update.exe");

                //if the Update.exe file found in the downloads folder has the same version code as the latest found online
                //no further action is carried out
                if(exeVersionCode == latestVersionCode) { return; }
            }
            else
            {///if no update.exe file exists in the downloads folder or its version does not match what was found online
                //download link of the latest update.exe file should be the second element of the string downloaded from linkHistoryURL
                var downloadLink = updateDataSeparated[1];

                try
                {
                    client.DownloadFile(downloadLink, "Downloads/Update.exe");
                }
                catch(Exception e)
                {//if the download attempt fails, do nothing further
                    return;
                }

                NotifyUser("An update is available");
            }
            


        }

        /// <summary>
        /// creates a toast notification with the specified message
        /// </summary>
        /// <param name="message"></param>
        public static void NotifyUser(string message)
        {

        }

        /// <summary>
        /// Compares current software version with the latest, then performs the necessary update actions
        /// UP NEXT
        /// </summary>
        /// <param name="historyData"></param>
        static void UpdateVersion(string historyData)
        {

        }

        /// <summary>
        /// method for download the regular uplink data necessary for the primary functioning of this
        /// software
        /// </summary>
        static void GetUplinkData() { }

    }
}
