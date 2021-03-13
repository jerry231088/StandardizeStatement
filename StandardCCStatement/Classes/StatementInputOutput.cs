namespace StandardCCStatement.Classes
{
    public class CreditCardTxnDetails
    {
        public string TxnDate;
        public string DebitAmt;
        public string CreditAmt;
        public string TxnDesc;
    }

    public class StandardCCBillOutput
    {
        public string TxnDate;
        public string TxnDesc;
        public decimal Debit;
        public decimal Credit;
        public string Currency;
        public string CardName;
        public string TxnType;
        public string TxnLocation;
    }
}