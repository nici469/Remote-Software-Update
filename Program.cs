using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.IO;
using Microsoft.Toolkit.Uwp.Notifications;

namespace Remote_Software_Update
{
    /// <summary>
    /// 
    /// </summary>
    class Program
    {
        /// <summary>
        /// an ever-increasing number to store the current software version
        /// </summary>
        const int versionNumber = 4;
        
        /// <summary>
        /// for debug only: to test if webclient.downloadfile can accept local directory as uri
        /// </summary>
        static void TestWebClientFTP()
        {
            WebClient myclient = new WebClient();
            myclient.DownloadFile("URI/Update.exe", "Downloads/Update.exe");
            Console.WriteLine("update downloaded");
            //for testing process.start
            Process.Start("Update.exe","Testing2 process");
            Console.ReadKey(true);
        }
        static async Task<int> Main(string[] args)
        {
            //testing only
            //TestWebClientFTP();

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
        /// it throws an exception if current instance is not terminated after sometime......
        /// </summary>
        /// <param name="newVersionCode"></param>
        public static void ExecuteUpdate(int newVersionCode) {
            //this method is and should be executed only after determining that a verified update exists in the 
            //downloads folder
            string newFileName = "Server" + newVersionCode + ".exe";

            //move the update.exe file to the current directory and rename
            File.Move("Downloads/Update.exe", newFileName);

            //Get the name of the current exe process
            string currentProcessName = Process.GetCurrentProcess().ProcessName;

            //run the new file with the current process name as argument
            Process.Start(newFileName, currentProcessName);

            //wait 60s to be exited, then throw an exception if the newfile process fails to close the current one
            Console.WriteLine("Current exe is awaiting auto-termination");

            Thread.Sleep(60000);

            throw new Exception("update file failed to terminate old process");
        }

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

            Console.WriteLine("An Update.exe file exists in Downloads folder");
            ///attempt to get the version code of the update file
            try
            {
                
                //create a separate appdomain to execute the Update.exe file in the downloads folder
                //this ensures the file can be deleted after use since the new appdomain can be safely disposed
                ///if the file is instead run in the cuurent appDomain, it will throw an exception if a deletion is attempted
                AppDomain domain = AppDomain.CreateDomain("newDomain");
                newVersionCode = domain.ExecuteAssembly("Downloads/Update.exe", new string[] { "version" });

                //safely dispose of the created appDomain
                AppDomain.Unload(domain);
                
            }
            catch(Exception e)
            {///if the attempt to get version code fails, the update.exe file is not a valid update file
                File.Delete("Downloads/Update.exe");///delete the invalid update file so it isnt processed at next software startup
                verifiedUpdateExists = false;
                Console.WriteLine("Error: An exception occurred when trying to execute Downloads/Update.exe");
                
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
            {//if the current software instance is not outdated

                verifiedUpdateExists=false;
                Console.WriteLine("NewVersion code: " + newVersionCode);
                Console.WriteLine("Info: no valid update exists.. Downloads/Update.exe will be deleted");
                
                
                File.Delete("Downloads/Update.exe");///delete the invalid update file so it isnt processed at next software startup
                return;
            }


        }

        /// <summary>
        /// carries out the operation of closing and deleting the old file once the new one,
        /// which should be the current instance, is running.
        /// This method is made asynchronus because "new thread" functions cannot receive parameters: it could have simply been executed 
        /// on a separate thread
        /// </summary>
        static  async Task FinaliseUpdate(string oldProcess) {
            Console.WriteLine("Attempting to Finalise Update");
            //still to be populated
            Process[] oldFileProcess = Process.GetProcessesByName(oldProcess);
            Console.WriteLine("process {0} will now be closed", oldFileProcess);

            try
            {
                //close the old process
                foreach (Process process in oldFileProcess)
                {
                    Console.WriteLine("attempting to kill " + process.ProcessName);
                    process.Kill();
                    Console.WriteLine(process.ProcessName + " process killed, waiting for process exit");
                    process.WaitForExit();
                    Console.WriteLine("attempting to dispose " + process.ProcessName);
                    process.Dispose();
                }

                //delete the old exe file
                Console.WriteLine("Attempting to delete old executable");
                System.IO.File.Delete(oldProcess + ".exe");
                Console.WriteLine("old file deleted");
            }
            catch (Exception e)
            {
                Console.WriteLine("");
                Console.WriteLine("An exception occured when finalising update");
                Console.WriteLine("The old process could not be killed or could not be deleted:");
                Console.WriteLine("The file may not exist in the current directory");
                Console.WriteLine(e.Message);
            }
            
            
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
        const string linkHistoryURL = "URI/linkHistory.txt";//create this on a cloud server such as dropbox

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
            Console.WriteLine("");//emptyline
            Console.WriteLine("Attempting to check for Update");
            WebClient client = new WebClient();

            string updateData;//to store the downloaded string containing the latest software info

            try
            {
                
                
                updateData = client.DownloadString(linkHistoryURL);
                Console.WriteLine(updateData);
                
                //UpdateVersion(updateData);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Error: could not check for update");
                return;
            }
            //updateData string download must have been successful at this point, and it is a 
            //comma separated string
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
                //attempt to execute the local Update.exe file in the downoads folder
                try
                {
                    int exeVersionCode = AppDomain.CurrentDomain.ExecuteAssembly("Downloads/Update.exe", new string[] { "version" });

                    //if the Update.exe file found in the downloads folder has the same version code as the latest found online
                    //no further action is carried out
                    if (exeVersionCode == latestVersionCode) { return; }
                    else {
                        ///if the version code of the local update file does not match what was found online, delete the local update file
                        ///this way, it can be redownloaded at the next check for update thread execution
                        File.Delete("Downloads/Update.exe");
                    }
                }
                catch(Exception e)
                {
                    //if executing the local update.exe file in the downloads folder fails, delete the file so it will not be processed
                    //again when the software runs
                    File.Delete("Downloads/Update.exe");
                    Console.WriteLine("Error: " + e.Message);

                }
                
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
                    Console.WriteLine("Failed to download update file");
                    return;
                }
                //notify the user if the update download was successful
                NotifyUser("An update is available");
                Console.WriteLine("Update: a software update is available");
            }
            


        }

        /// <summary>
        /// creates a toast notification with the specified message
        /// </summary>
        /// <param name="message"></param>
        public static void NotifyUser(string message)
        {
            new ToastContentBuilder().AddArgument("send1", 222).AddHeader("", "NSONG Automation", "none")
                .AddText(message)
                //.AddText(message2)
                .Show();
        }

        

        /// <summary>
        /// method for download the regular uplink data necessary for the primary functioning of this
        /// software
        /// </summary>
        static void GetUplinkData() { }

    }
}
