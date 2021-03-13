using System.Collections.Generic;

namespace StandardCCStatement.Abstraction
{
    interface IStatmentCCReader
    {
        void GetBankNameFromUser(string userInput);
        string GetCaseTypeFromUser(List<string> caseTypeList);
    }
}