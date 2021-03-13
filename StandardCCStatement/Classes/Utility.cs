using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace StandardCCStatement.Classes
{
    public static class Utility
    {
        /// <summary>
        /// Method to check user input
        /// </summary>
        /// <param name="userInput">input provided by user</param>
        /// <returns>bool</returns>
        public static bool CheckUserInput(string userInput)
        {
            bool userEnteredInput = !string.IsNullOrEmpty(userInput);
            return userEnteredInput;
        }

        /// <summary>
        /// Method to get all csv files present in the input directory
        /// </summary>
        /// <param name="dirPath">input directory path</param>
        /// <returns>array of string</returns>
        public static string[] GetAllCsvFiles(string dirPath)
        {
            string[] allCsvFiles = Directory.GetFiles(dirPath, "*.csv");
            return allCsvFiles;            
        }

        /// <summary>
        /// Method to get input statement as per bank case number
        /// </summary>
        /// <param name="allInputCsvFiles">array of string contains csv files</param>
        /// <param name="bankName">bank name provided</param>
        /// <returns>Dictionary of key and values as string</returns>
        public static Dictionary<string, string> GetInputStatmentAsPerBankCaseType(string[] allInputCsvFiles, string bankName)
        {
            Dictionary<string, string> bankInputStatementType = new Dictionary<string, string>();            
            foreach (string file in allInputCsvFiles)
            {
                string[] filePathSplit = file.Split('\\');
                if (filePathSplit.Length <= 0) continue;
                string[] fileNameSplit = filePathSplit[^1].Split('-');
                if (!fileNameSplit[0].ToLower().Equals(bankName)) continue;
                
                string billCaseType = fileNameSplit[^1][0..^4].ToLower();
                //string billCaseType = fileNameSplit[^1].Substring(0, fileNameSplit[^1].Length - 4);
                if (!bankInputStatementType.ContainsKey(bankName + "_" + billCaseType))
                    bankInputStatementType[bankName + "_" + billCaseType] = filePathSplit[^1];
                //bankInputStatementType[bankName + "_" + billCaseType] = filePathSplit[filePathSplit.Length - 1];
            }
            return bankInputStatementType;
        }

        /// <summary>
        /// This Method will return True if List is Not Null and it's items count > 0       
        /// </summary>
        /// <param name="items">IEnumerable of string</param>
        /// <returns>Bool</returns>
        public static bool HasValue(IEnumerable<string> items)
        {
            if (items != null)
            {
                if (items.Count() > 0)
                {
                    foreach(var item in items)
                    {
                        if (string.IsNullOrEmpty(item)) continue;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// This Method will return count of items having values not empty       
        /// </summary>
        /// <param name="items">IEnumerable of string</param>
        /// <returns>Int</returns>
        public static int GetCustomerNameFromRow(IEnumerable<string> items)
        {
            int name = 0;
            if (items != null)
            {
                if (items.Count() > 0)
                {
                    foreach (var item in items)
                    {
                        if (string.IsNullOrEmpty(item) || item.Equals("Domestic Transactions") || item.Equals("International Transactions") 
                            || item.Equals("Domestic Transaction") || item.Equals("International Transaction")) continue;
                        name++;
                    }
                }
            }
            return name;
        }

        /// <summary>
        /// Method to convert object data type to datetime
        /// </summary>
        /// <param name="value">object data type</param>
        /// <returns>datetime value</returns>
        public static DateTime ConvertToDateTime(object value)
        {
            bool parseToDt = false;
            DateTime val;
            parseToDt = DateTime.TryParseExact(value.ToString(),
                       "dd-MM-yyyy",
                       CultureInfo.InvariantCulture,
                       DateTimeStyles.None,
                       out val);
            if (parseToDt) return val.Date;
            else return DateTime.Now.Date;
        }

        /// <summary>
        /// Method to convert object data type to decimal
        /// </summary>
        /// <param name="value">object data type</param>
        /// <returns>decimal value</returns>
        public static decimal ConvertToDecimal(object value)
        {
            bool parseToDec = false;
            decimal val;
            parseToDec = decimal.TryParse(value.ToString(), out val);
            if (parseToDec) return val;
            else return 0;
        }

        /// <summary>
        /// Method to convert object data type to string
        /// </summary>
        /// <param name="value">object data type</param>
        /// <returns>string value</returns>
        public static string ConvertToString(object value)
        {
            return value.ToString();
        }

        /// <summary>
        /// Method to check output file is opened or not
        /// </summary>
        /// <param name="file">file info</param>
        /// <returns>bool</returns>
        public static bool IsFileOpened(FileInfo file)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                Console.WriteLine("Output file is opened, please close it and press any key to continue.");
                Console.ReadLine();
                return IsFileOpened(file);
            }

            //file is not opened
            return false;
        }

        public enum TxnTypeLoc
        {
            D = 0,
            I = 1
        }
    }
}