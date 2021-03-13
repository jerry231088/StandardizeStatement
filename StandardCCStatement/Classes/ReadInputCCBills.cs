using System;
using System.Collections.Generic;
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
                switch (bkName)
                {
                    case "axis":
                    case "hdfc":
                    case "icici":
                    case "idfc":
                        string getDirectoryPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\InputBills\";
                        string[] inputFiles = Utility.GetAllCsvFiles(getDirectoryPath);
                        Dictionary<string, string> statementCaseType = Utility.GetInputStatmentAsPerBankCaseType(inputFiles, bkName);
                        if (statementCaseType.Count <= 0)
                        {
                            Console.WriteLine("No Statement present. Please select another bank and press any key to continue.");
                            UserInputBankName();
                            break;
                        }
                        List<string> caseTypeList = GetCaseTypeList(statementCaseType.Keys);                        
                        Console.WriteLine("Select the input statement case from the case types given below and press any key to continue.");
                        PrintCaseTypeFound(caseTypeList);
                        string userCaseType = GetCaseTypeFromUser(caseTypeList);
                        Stopwatch sw = Stopwatch.StartNew();
                        var ioPath = InputOutputStatementPaths(statementCaseType, bkName, userCaseType, getDirectoryPath);
                        string inputStatmentPath = ioPath.Item1;
                        string stdStatementPath = ioPath.Item2;
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
                        break;
                    default:
                        Console.WriteLine("Incorrect bank name provided, please enter again.");
                        Console.WriteLine("Please enter your bank name (Axis, HDFC, ICICI or IDFC) and press any key to continue.");
                        UserInputBankName();
                        break;
                }
            }
            else
            {
                Console.WriteLine("No bank name provided, please enter again.");
                Console.WriteLine("Please enter your bank name (Axis, HDFC, ICICI or IDFC) and press any key to continue.");
                UserInputBankName();
            }
        }

        /// <summary>
        /// Method to get input file case number from user
        /// </summary>
        /// <param name="caseTypeList">List of string</param>
        /// <returns>string</returns>
        public string GetCaseTypeFromUser(List<string> caseTypeList)
        {
            string caseTypeSlctd = Console.ReadLine();
            bool inputProvided = Utility.CheckUserInput(caseTypeSlctd);
            if (!inputProvided)
            {
                Console.WriteLine("No case type provided, please enter again.");
                return GetCaseTypeFromUser(caseTypeList);
            }
            else
            {
                if (!caseTypeList.Contains(caseTypeSlctd.Trim().ToLower()))
                {
                    Console.WriteLine("Incorrect case type provided, please enter again.");
                    return GetCaseTypeFromUser(caseTypeList);
                }
                else return caseTypeSlctd.Trim().ToLower();
            }
        }

        /// <summary>
        /// Method to Get case type list from input statement, if multple statements for a bank present
        /// </summary>
        /// <param name="keys">case numbers</param>
        /// <returns>List<string></returns>
        private List<string> GetCaseTypeList(Dictionary<string, string>.KeyCollection keys)
        {
            List<string> caseTypeList = new List<string>();
            foreach (string caseTypeKey in keys)
            {
                string[] splitCaseType = caseTypeKey.Split('_');
                caseTypeList.Add(splitCaseType[^1]);
            }
            return caseTypeList;
        }

        /// <summary>
        /// Method to print case numbers for bank statements present.
        /// </summary>
        /// <param name="caseTypeList">List of string</param>
        private void PrintCaseTypeFound(List<string> caseTypeList)
        {
            foreach (string caseType in caseTypeList)
            {
                Console.WriteLine("\n" + caseType);
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
        private Tuple<string, string> InputOutputStatementPaths(Dictionary<string, string> statementCaseType, string bkName, string userCaseType, string getDirectoryPath)
        {
            string slctdFileName = statementCaseType[bkName + "_" + userCaseType];
            string inputStatmentPath = getDirectoryPath + "\\" + slctdFileName;
            string outputDirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\StandardizeStatements\";
            if (!Directory.Exists(outputDirPath)) Directory.CreateDirectory(outputDirPath);
            string[] splitSlctdStatementName = slctdFileName.Split('-');
            string stdStatementPath = outputDirPath + splitSlctdStatementName[0] + "-Output-" + splitSlctdStatementName[2];
            return new Tuple<string, string>(inputStatmentPath, stdStatementPath);
        }
    }
}