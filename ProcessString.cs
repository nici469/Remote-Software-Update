using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remote_Software_Update
{
    class ProcessString
    {

        /// <summary>
        /// cuts out portions of the data string from the first character up to and including the specified position
        /// and returns the new string
        /// </summary>
        /// <param name="data"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        string CutString(string stringData, int cutPos)
        {
            string outString = null;
            for (int i = cutPos + 1; i < stringData.Length; i++)
            {
                outString += stringData[i];
            }
            return outString;
        }

        /// <summary>
        /// returns a string array that is one element longer than the input string array, the new element being newLine
        /// </summary>
        /// <param name="lineArray"></param>
        /// <param name="newLine"></param>
        /// <returns></returns>
        string[] AddLine(string[] lineArray, string newLine)
        {
            //declare the output string array as one element longer than the input lineArray
            string[] outstringArray = new string[lineArray.Length + 1];

            //copy the elements of lineArray into the output string array
            for (int i = 0; i < lineArray.Length; i++)
            {
                outstringArray[i] = lineArray[i];
            }

            //add the new line to outstringArray
            outstringArray[lineArray.Length] = newLine;
            //return
            return outstringArray;
        }

        /// <summary>
        /// Converts an inputString into an array of strings using the specified "separator"
        /// </summary>
        /// <param name="inputString"></param>
        /// <param name="separator"></param>
        public string[] SeparateLines(string inputString, char separator)
        {
            //to store each separate strings in the inputString as an element of an array
            string[] lines = new string[0];

            string fullString = inputString;

            //to store the number of "separator" characters in the input string
            int noOfSepChar = 0;

            //count the number of "separator" characters in the fullString or inputString
            for (int i = 0; i < fullString.Length; i++)
            {
                if (fullString[i] == separator) noOfSepChar++;
            }


            //loop through all the separate strings in the fullString
            for (int sepCount = noOfSepChar; sepCount > 0; sepCount--)
            {
                //in each iteration, search for the nearest "separator" character
                int nearestSepPos = 0;

                for (int i = 0; i < fullString.Length; i++)
                {
                    if (fullString[i] == separator) { nearestSepPos = i; break; }
                }

                //extract the topmost string 
                string currrentString = null;
                for (int i = 0; i < nearestSepPos; i++)
                {
                    currrentString += fullString[i];
                }

                //cut out the any discovered string from fullString
                fullString = CutString(fullString, nearestSepPos);
                lines = AddLine(lines, currrentString);

            }

            //add any final string that didnt end with the separator, such as the last line in a string file
            if (fullString != null) { lines = AddLine(lines, fullString); }

            return lines;

        }



        /// <summary>
        /// to convert a CSV string into a JGD array:
        /// lines being rows, and comma determining columns
        /// </summary>
        /// <param name="csvString"></param>
        public string[][] CSVtoJGD(string data)
        {
            //separate the string into lines.. unnecessary if File.ReadAllLines is used
            string[] lines = SeparateLines(data, '\n');

            //initialise the jagged array[row][column]. the number of rows is the number of elements in the lines array
            string[][] csvJgdArray = new string[lines.Length][];

            //in each line, separtae the strings usinng commas
            for (int i = 0; i < lines.Length; i++)
            {
                csvJgdArray[i] = SeparateLines(lines[i], ',');
            }

            return csvJgdArray;
        }

        /// <summary>
        /// converts an array of comma-separated strings ta a jgd array
        /// , the comma determinig the columns
        /// </summary>
        /// <param name="lineData"></param>
        /// <returns></returns>
        public string[][] StringArraytoJGD(string[] lineData)
        {

            //initialise the jagged array[row][column]. the number of rows is the number of elements in the lines array
            string[][] csvJgdArray = new string[lineData.Length][];

            //in each line, separtae the strings usinng commas
            for (int i = 0; i < lineData.Length; i++)
            {
                csvJgdArray[i] = SeparateLines(lineData[i], ',');
            }

            return csvJgdArray;
        }



    }

}
