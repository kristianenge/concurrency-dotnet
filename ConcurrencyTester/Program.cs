using System;

namespace ConcurrencyTester
{
    
    internal class Program
    {
        
        public static void Main()
        {
            var numberOfRequests = 20;
            var connectionPool = 4;
            var processingType = ProcessingType.Paralell;


            //webServer(numberOfRequests, connectionPool, processingType);
            digipost(numberOfRequests, connectionPool, processingType);

            Console.ReadKey();
        }

        private static void digipost(int numberOfRequests, int connectionPool, ProcessingType processingType)
        {
            var da = new DigipostAsync(numberOfRequests, connectionPool);
            var dp = new DigipostParalell(numberOfRequests,connectionPool);
            switch (processingType)
            {
                case ProcessingType.Paralell:
                    dp.TestParallel();
                    break;
                case ProcessingType.Async:
                    da.TestAsync();
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