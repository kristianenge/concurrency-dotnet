using System;
using System.Configuration;
using Digipost.Api.Client;

namespace ConcurrencyTester
{
    
    internal class Program
    {
        private static string thumbprint = "7d fc c9 8b 88 55 16 4d 03 a3 64 a4 90 98 26 9d 23 31 4d 0f";
        private const string SenderId = "106799002"; //"779052"; 


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

            var clientConfig = new ClientConfig(SenderId) {ApiUrl = new Uri("https://qa2.api.digipost.no")};
            
            switch (processingType)
            {
                case ProcessingType.Paralell:
                    new DigipostParalell(numberOfRequests,connectionPool).AleksanderParallel();
                    break;
                case ProcessingType.Async:
                    new DigipostAsync(numberOfRequests, connectionPool, clientConfig, thumbprint).Run();
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