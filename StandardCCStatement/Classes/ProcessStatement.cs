using StandardCCStatement.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StandardCCStatement.Classes
{
    internal sealed class ProcessStatement : IProcessCCStatements
    {
        #region Members
        private static readonly object _lock = new object();
        private static ProcessStatement _processStatement = null;
        #endregion

        private ProcessStatement()
        {

        }

        public static ProcessStatement GetInstance()
        {
            lock (_lock)
            {
                if (_processStatement == null)
                {
                    _processStatement = new ProcessStatement();
                }
                return _processStatement;
            }
        }

        /// <summary>
        /// Method to process input credit card statement as per bank name and case number provided by user
        /// </summary>
        /// <param name="txnTypeWiseUserTxnDetails">Dictionary of string as key and value as Dictionary of string as key and List of CreditCardTxnDetails as value</param>
        /// <returns>SortedDictionary<DateTime, List<StandardCCBillOutput>></returns>
        public SortedDictionary<DateTime, List<StandardCCBillOutput>> ProcessCreditCardBill(Dictionary<string, Dictionary<string, List<CreditCardTxnDetails>>> txnTypeWiseUserTxnDetails)
        {
            SortedDictionary<DateTime, List<StandardCCBillOutput>> outputStatement =
                new SortedDictionary<DateTime, List<StandardCCBillOutput>>();

            Task<SortedDictionary<DateTime, List<StandardCCBillOutput>>> task = 
                Task<SortedDictionary<DateTime, List<StandardCCBillOutput>>>.Factory.StartNew(() =>
            {
                return ProcessBill(txnTypeWiseUserTxnDetails);
            });
            task.Wait();

            return outputStatement = task.Result;
        }

        /// <summary>
        /// Method to process the input statement
        /// </summary>
        /// <param name="txnTypeWiseUserTxnDetails">Dictionary of string as key and value as Dictionary of string as key and List of CreditCardTxnDetails as value</param>
        /// <returns>SortedDictionary<DateTime, List<StandardCCBillOutput>></returns>
        private SortedDictionary<DateTime, List<StandardCCBillOutput>> ProcessBill(Dictionary<string, Dictionary<string, List<CreditCardTxnDetails>>> txnTypeWiseUserTxnDetails)
        {
            SortedDictionary<DateTime, List<StandardCCBillOutput>> outputStatement = 
                new SortedDictionary<DateTime, List<StandardCCBillOutput>>();
            //Process received data here as per output format.
            foreach (var txnLoc in txnTypeWiseUserTxnDetails)
            {
                foreach (var userTxn in txnLoc.Value)
                {
                    foreach (var txn in userTxn.Value)
                    {
                        DateTime dt = Utility.ConvertToDateTime(txn.TxnDate);
                        if (!outputStatement.ContainsKey(dt)) outputStatement[dt] = new List<StandardCCBillOutput>();
                        StandardCCBillOutput outputBill = new StandardCCBillOutput();
                        List<string> splitTxnDesc = txn.TxnDesc.Split(' ').Select(s => s.Trim()).ToList();
                        outputBill.CardName = userTxn.Key;
                        outputBill.TxnDate = txn.TxnDate;
                        outputBill.TxnDesc = txn.TxnDesc;
                        if (!string.IsNullOrEmpty(txn.DebitAmt))
                            outputBill.Debit = Utility.ConvertToDecimal(txn.DebitAmt);
                        if (!string.IsNullOrEmpty(txn.CreditAmt))
                            outputBill.Credit = Utility.ConvertToDecimal(txn.CreditAmt);
                        if (txnLoc.Key == Utility.ConvertToString(Utility.TxnTypeLoc.D))
                        {
                            outputBill.Currency = "INR";
                            outputBill.TxnType = "Domestic";
                            if(!txn.TxnDesc.ToLower().Contains("cash back"))
                                outputBill.TxnLocation = splitTxnDesc[^1].ToLower();
                        }
                        else if (txnLoc.Key == Utility.ConvertToString(Utility.TxnTypeLoc.I))
                        {
                            outputBill.TxnType = "International";
                            outputBill.Currency = splitTxnDesc[^1];
                            if (!txn.TxnDesc.ToLower().Contains("cash back"))
                                outputBill.TxnLocation = splitTxnDesc[^2].ToLower();
                        }
                        outputStatement[dt].Add(outputBill);
                    }
                }
            }
            return outputStatement;
        }
    }
}