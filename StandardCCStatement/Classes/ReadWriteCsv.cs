using StandardCCStatement.Abstraction;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StandardCCStatement.Classes
{
    internal sealed class ReadWriteCsv : IReadWriteCsv
    {
        #region Members
        private readonly Dictionary<string, Dictionary<string, List<CreditCardTxnDetails>>> _txnTypeWiseUserTxnDetails = new Dictionary<string, Dictionary<string, List<CreditCardTxnDetails>>>();
        private static readonly object _lock = new object();
        private static ReadWriteCsv _readWriteCsvInstance = null;
        private readonly string _inputFilePath = string.Empty;
        private readonly string _bankName = string.Empty;
        #endregion

        private ReadWriteCsv(string inputFilePath)
        {
            _inputFilePath = inputFilePath;
            List<string> splitIpFilePath = inputFilePath.Split("\\").Select(s => s.Trim()).ToList();
            List<string> fileNameSplit = splitIpFilePath[splitIpFilePath.Count - 1].Split('-').Select(s => s.Trim()).ToList();
            _bankName = fileNameSplit[0].ToLower();
        }

        public static ReadWriteCsv GetInstance(string inputFilePath)
        {
            lock (_lock)
            {
                if (_readWriteCsvInstance == null)
                {
                    _readWriteCsvInstance = new ReadWriteCsv(inputFilePath);
                }
                return _readWriteCsvInstance;
            }
        }

        /// <summary>
        /// Method to read csv input credit card statement
        /// </summary>
        /// <returns>SortedDictionary<DateTime, List<StandardCCBillOutput>></returns>
        public SortedDictionary<DateTime, List<StandardCCBillOutput>> ReadCSVFile()
        {
            Task.Factory.StartNew(() => ReadFile()).Wait();
            IProcessCCStatements processBill = ProcessStatement.GetInstance();
            SortedDictionary<DateTime, List<StandardCCBillOutput>> outputStatement = processBill.ProcessCreditCardBill(_txnTypeWiseUserTxnDetails);
            return outputStatement;
        }

        /// <summary>
        /// Method to write output standardized credit card statement
        /// </summary>
        /// <param name="outputPath">output file path</param>
        /// <param name="outputData">output data as SortedDictionary<DateTime, List<StandardCCBillOutput>></param>
        public void WriteCSVFile(string outputPath, SortedDictionary<DateTime, List<StandardCCBillOutput>> outputData)
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
            using (StreamWriter file = new StreamWriter(outputPath))
            {
                file.WriteLine("Date, Transaction Description, Debit, Credit, Currency, CardName, Transaction, Location");
                foreach(var record in outputData)
                {
                    foreach(var data in record.Value)
                    {
                        string rowOutput = data.TxnDate + "," + data.TxnDesc + ","
                        + data.Debit + "," + data.Credit + "," + data.Currency
                        + "," + data.CardName + "," + data.TxnType + "," + data.TxnLocation;
                        file.WriteLine(rowOutput);
                    }
                }
            }
            Console.WriteLine("Standardized credit card statement saved to " + outputPath);
        }

        /// <summary>
        /// Method to read input csv credit crad statement
        /// </summary>
        private void ReadFile()
        {
            int domesticOrInternational = -1; //-1: Nothing, 0: Domestic, 1: International
            string customerName = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(_inputFilePath))
                {
                    string[] rows = File.ReadAllLines(_inputFilePath);
                    foreach (var row in rows)
                    {
                        var columns = row.Split(',').Select(s => s.Trim()).ToList();
                        bool rowHasVal = Utility.HasValue(columns);
                        if (!rowHasVal)
                            continue;

                        if (Utility.GetCustomerNameFromRow(columns) == 1)
                        {
                            customerName = ExtractCustomerName(columns);
                            switch (domesticOrInternational)
                            {
                                case 0:
                                    if (!_txnTypeWiseUserTxnDetails[Utility.ConvertToString(Utility.TxnTypeLoc.D)].ContainsKey(customerName))
                                        _txnTypeWiseUserTxnDetails[Utility.ConvertToString(Utility.TxnTypeLoc.D)][customerName] = new List<CreditCardTxnDetails>();
                                    break;
                                case 1:
                                    if (!_txnTypeWiseUserTxnDetails[Utility.ConvertToString(Utility.TxnTypeLoc.I)].ContainsKey(customerName))
                                        _txnTypeWiseUserTxnDetails[Utility.ConvertToString(Utility.TxnTypeLoc.I)][customerName] = new List<CreditCardTxnDetails>();
                                    break;
                            }
                            continue;
                        }

                        if (columns.Contains("Domestic Transactions") || columns.Contains("Domestic Transaction"))
                        {
                            domesticOrInternational = 0;
                            customerName = string.Empty;
                            if (!_txnTypeWiseUserTxnDetails.ContainsKey(Utility.ConvertToString(Utility.TxnTypeLoc.D)))
                                _txnTypeWiseUserTxnDetails[Utility.ConvertToString(Utility.TxnTypeLoc.D)] = new Dictionary<string, List<CreditCardTxnDetails>>();
                            continue;
                        }
                        else if (columns.Contains("International Transactions") || columns.Contains("International Transaction"))
                        {
                            domesticOrInternational = 1;
                            customerName = string.Empty;
                            if (!_txnTypeWiseUserTxnDetails.ContainsKey(Utility.ConvertToString(Utility.TxnTypeLoc.I)))
                                _txnTypeWiseUserTxnDetails[Utility.ConvertToString(Utility.TxnTypeLoc.I)] = new Dictionary<string, List<CreditCardTxnDetails>>();
                            continue;
                        }
                        else if (columns.Contains("Date"))
                        {
                            customerName = string.Empty;
                            continue;
                        }

                        if (!string.IsNullOrEmpty(customerName))
                        {
                            CreditCardTxnDetails ccTxnDetails = new CreditCardTxnDetails();
                            //Need to merge these conditions to make it generic
                            switch (_bankName)
                            {
                                case "axis":
                                    ccTxnDetails.TxnDate = columns[0];
                                    ccTxnDetails.DebitAmt = columns[1];
                                    ccTxnDetails.CreditAmt = columns[2];
                                    ccTxnDetails.TxnDesc = Regex.Replace(columns[3], @"\s+", " ");
                                    break;
                                case "hdfc":
                                    ccTxnDetails.TxnDate = columns[0];
                                    if (!columns[2].ToLower().Contains("cr"))
                                        ccTxnDetails.DebitAmt = columns[2];
                                    else
                                        ccTxnDetails.CreditAmt = columns[2].Substring(0, columns[2].Length - 2).Trim();
                                    ccTxnDetails.TxnDesc = Regex.Replace(columns[1], @"\s+", " ");
                                    break;
                                case "icici":
                                    ccTxnDetails.TxnDate = columns[0];
                                    ccTxnDetails.DebitAmt = columns[2];
                                    ccTxnDetails.CreditAmt = columns[3];
                                    ccTxnDetails.TxnDesc = Regex.Replace(columns[1], @"\s+", " ");
                                    break;
                                case "idfc":
                                    ccTxnDetails.TxnDate = columns[1];
                                    if (!columns[2].ToLower().Contains("cr"))
                                        ccTxnDetails.DebitAmt = columns[2];
                                    else
                                        ccTxnDetails.CreditAmt = columns[2].Substring(0, columns[2].Length - 2).Trim();
                                    ccTxnDetails.TxnDesc = Regex.Replace(columns[0], @"\s+", " ");
                                    break;
                            }
                            switch (domesticOrInternational)
                            {
                                case 0:
                                    _txnTypeWiseUserTxnDetails[Utility.ConvertToString(Utility.TxnTypeLoc.D)][customerName].Add(ccTxnDetails);
                                    break;
                                case 1:
                                    _txnTypeWiseUserTxnDetails[Utility.ConvertToString(Utility.TxnTypeLoc.I)][customerName].Add(ccTxnDetails);
                                    break;
                            }
                            continue;
                        }                       
                    }
                }
                else
                    Console.WriteLine("File doesn't exist. Please try again.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Method to extract customer name from csv file
        /// </summary>
        /// <param name="columns">List of string from csv row</param>
        /// <returns>string</returns>
        private string ExtractCustomerName(List<string> columns)
        {
            string customerName = string.Empty;
            foreach (string col in columns)
            {
                if (string.IsNullOrEmpty(col)) continue;
                customerName = col;
                break;                
            }
            return customerName;
        }
    }
}