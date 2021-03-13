using StandardCCStatement.Abstraction;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StandardCCStatement.Classes
{
    internal sealed class GenerateStandardCCBills : IStandardizeBill
    {
        #region Members
        private static readonly object _lock = new object();
        private static GenerateStandardCCBills _generateStandardCCBillsInstance = null;
        #endregion

        private GenerateStandardCCBills()
        {

        }

        public static GenerateStandardCCBills GetInstance()
        {
            lock (_lock)
            {
                if (_generateStandardCCBillsInstance == null)
                {
                    _generateStandardCCBillsInstance = new GenerateStandardCCBills();
                }
                return _generateStandardCCBillsInstance;
            }
        }

        /// <summary>
        /// Method to standardize credit card statement
        /// </summary>
        /// <param name="inputFile">input file path</param>
        /// <param name="outputFile">output file path</param>
        public void StandardizeStatement(string inputFile, string outputFile)
        {
            IReadWriteCsv readWriteCsv = ReadWriteCsv.GetInstance(inputFile);
            SortedDictionary<DateTime, List<StandardCCBillOutput>> outputStatement = readWriteCsv.ReadCSVFile();
            Task.Factory.StartNew(() => readWriteCsv.WriteCSVFile(outputFile, outputStatement)).Wait();
        }
    }
}
