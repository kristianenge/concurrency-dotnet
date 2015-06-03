using System;
using System.Configuration;

namespace ConcurrencyTester
{
    
    internal class Program
    {
        
        public static void Main()
        {
            var numberOfRequests = 1000;
            var connectionPool = 2;
            var processingType = ProcessingType.Paralell;

            Console.WriteLine("Starting program ...");
            //webServer(numberOfRequests, connectionPool, processingType);
            digipost(numberOfRequests, connectionPool, processingType);

            Console.ReadKey();
        }

        private static void digipost(int numberOfRequests, int connectionPool, ProcessingType processingType)
        {
            Console.WriteLine("Starting to send digipost: {0}, with requests: {1}, poolcount: {2}", processingType, numberOfRequests, connectionPool);
            
            switch (processingType)
            {
                case ProcessingType.Paralell:
                    new DigipostParalell(numberOfRequests,connectionPool).AleksanderParallel();
                    break;
                case ProcessingType.Async:
                    new DigipostAsync(numberOfRequests, connectionPool).TestAsync();
                    break;
            }
        }

        private static void webServer(int numberOfRequests, int connectionPool, ProcessingType processingType)
        {
            var wa = new WebGetAsync(numberOfRequests, connectionPool);
            switch (processingType)
            {
                case ProcessingType.Paralell:
                    wa.TestParallel();
                    break;
                case ProcessingType.Async:
                    wa.TestAsync();
                    break;
            }
        }

        
    }
}