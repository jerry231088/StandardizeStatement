using System;
using System.Diagnostics;
using System.IO;
using StandardCCStatement.Abstraction;

namespace StandardCCStatement.Classes
{
    internal sealed class ReadInputCCBills : IStatmentCCReader
    {
        #region Members
        private static readonly object _lock = new object();
        private static ReadInputCCBills _readInputCCBillsInstance = null;
        #endregion

        private ReadInputCCBills() 
        {
            
        }

        public static ReadInputCCBills GetInstance()
        {
            lock (_lock)
            {
                if (_readInputCCBillsInstance == null)
                {
                    _readInputCCBillsInstance = new ReadInputCCBills();
                }
                return _readInputCCBillsInstance;
            }
        }

        /// <summary>
        /// Method to get bank name from user
        /// </summary>
        /// <param name="userInput">input provided by user</param>
        public void GetBankNameFromUser(string userInput)
        {
            bool inputProvided = Utility.CheckUserInput(userInput);
            if (inputProvided)
            {
                string bkName = userInput.Trim().ToLower();
                string getDirectoryPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\InputBills\";
                string[] inputFiles = Utility.GetAllCsvFiles(getDirectoryPath);
                var bankStatementExists = Utility.CheckInputStatmentExists(inputFiles, bkName);
                if (bankStatementExists.Item1)
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    string inputStatmentPath = bankStatementExists.Item2;
                    string stdStatementPath = CreateOutputStatementPath(inputStatmentPath);
                    //Check if output file is already opened, if exists.
                    if (File.Exists(stdStatementPath))
                    {
                        FileInfo fi = new FileInfo(stdStatementPath);
                        bool fileOpened = Utility.IsFileOpened(fi);
                    }
                    IStandardizeBill genStandardizeStatement = GenerateStandardCCBills.GetInstance();
                    genStandardizeStatement.StandardizeStatement(inputStatmentPath, stdStatementPath);
                    sw.Stop();
                    Console.WriteLine("Time taken by app to complete the process: " + sw.ElapsedMilliseconds + " msec.");
                    Console.ReadLine();                    
                }
                else
                {
                    Console.WriteLine("No Statement present or incorrect bankname provided. Please enter another bankname and press any key to continue.");
                    UserInputBankName();
                }
            }
            else
            {
                Console.WriteLine("No bank name provided, please enter again.");
                Console.WriteLine("Please enter your bank name like Axis, HDFC, ICICI, etc and press any key to continue.");
                UserInputBankName();
            }
        }

        /// <summary>
        /// Method to get bank name from user
        /// </summary>
        private void UserInputBankName()
        {
            string userInput = Console.ReadLine();
            GetBankNameFromUser(userInput);
        }

        /// <summary>
        /// Method to get Input and Output statements path
        /// </summary>
        /// <param name="statementCaseType">Dictionary<string, string></param>
        /// <param name="bkName">bankname as string</param>
        /// <param name="userCaseType">case number as string</param>
        /// <param name="getDirectoryPath">directory path as string</param>
        /// <returns>Tuple of items as string</returns>
        private string CreateOutputStatementPath(string inputStatmentPath)
        {
            string outputDirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\StandardStatements\";
            if (!Directory.Exists(outputDirPath)) Directory.CreateDirectory(outputDirPath);
            string[] inputFilePathSplit = inputStatmentPath.Split('\\');
            string[] splitSlctdStatementName = inputFilePathSplit[^1].Split('-');
            string stdStatementPath = outputDirPath + splitSlctdStatementName[0] + "-Output-" + splitSlctdStatementName[2];
            return stdStatementPath;
        }
    }
}