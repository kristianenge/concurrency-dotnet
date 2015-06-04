using System;
using System.Configuration;

namespace ConcurrencyTester
{
    
    internal class Program
    {
        
        public static void Main()
        {
            var numberOfRequests = 100;
            var threadsActive = 4;
            var processingType = ProcessingType.Async;

            Console.WriteLine("Starting program ...");
            //webServer(numberOfRequests, connectionPool, processingType);
            Digipost(numberOfRequests, threadsActive, processingType);

            Console.ReadKey();
        }

        private static void Digipost(int numberOfRequests, int connectionPool, ProcessingType processingType)
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