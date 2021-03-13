using StandardCCStatement.Classes;
using System;
using System.Collections.Generic;

namespace StandardCCStatement.Abstraction
{
    interface IReadWriteCsv
    {
        SortedDictionary<DateTime, List<StandardCCBillOutput>> ReadCSVFile();

        void WriteCSVFile(string outputPath, SortedDictionary<DateTime, List<StandardCCBillOutput>> outputData);
    }
}