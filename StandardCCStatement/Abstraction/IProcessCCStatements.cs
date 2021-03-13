using StandardCCStatement.Classes;
using System;
using System.Collections.Generic;

namespace StandardCCStatement.Abstraction
{
    interface IProcessCCStatements
    {
        SortedDictionary<DateTime, List<StandardCCBillOutput>> ProcessCreditCardBill(Dictionary<string, Dictionary<string, List<CreditCardTxnDetails>>> _txnTypeWiseUserTxnDetails);
    }
}