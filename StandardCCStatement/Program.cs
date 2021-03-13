using StandardCCStatement.Abstraction;
using StandardCCStatement.Classes;
using System;
using System.Threading;

namespace StandardCCStatement
{
    class Program
    {
        static void Main()
        {
            const string appName = "Standardize Statement";
            Mutex mutex = new Mutex(true, appName, out bool createdNew);
            //Allow to open Single Instance only
            if (createdNew)
                GetStandardizeStatement();
        }

        /// <summary>
        /// Method to run Standardize Statement app
        /// </summary>
        private static void GetStandardizeStatement()
        {            
            Console.WriteLine("Standardize Statement App Starts!");
            Console.WriteLine("Please enter your bank name (Axis, HDFC, ICICI or IDFC) and press any key to continue.");
            string userInput = Console.ReadLine();
            IStatmentCCReader readInputBills = ReadInputCCBills.GetInstance();
            readInputBills.GetBankNameFromUser(userInput);            
            Console.ReadLine();
        }        
    }
}