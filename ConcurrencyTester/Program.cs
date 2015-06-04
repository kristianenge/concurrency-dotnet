﻿using System;
using Digipost.Api.Client;

namespace ConcurrencyTester
{
    
    internal class Program
    {
        private static string httpsQa2ApiDigipostNo = "https://qa2.api.digipost.no";
        private static string thumbprint = "7d fc c9 8b 88 55 16 4d 03 a3 64 a4 90 98 26 9d 23 31 4d 0f";
        private static string SenderId = "106799002"; //"779052"; 
        private static int degreeOfParallelism = 4;
        private static int numberOfRequests = 100;
        private static int threadsActive = 4;

        const ProcessingType processingType = ProcessingType.Paralell;

        public static void Main()
        {
            Console.WriteLine("Starting program ...");
            
            Digipost(numberOfRequests, threadsActive, processingType);

            Console.ReadKey();
        }

        private static void Digipost(int numberOfRequests, int connectionLimit, ProcessingType processingType)
        {
            Console.WriteLine("Starting to send digipost: {0}, with requests: {1}, poolcount: {2}", processingType, numberOfRequests, connectionLimit);

            var clientConfig = new ClientConfig(SenderId) {ApiUrl = new Uri(httpsQa2ApiDigipostNo)};
            
            switch (processingType)
            {
                case ProcessingType.Paralell:
                    new DigipostParalell(numberOfRequests,connectionLimit, degreeOfParallelism, clientConfig, thumbprint).Run();
                    break;
                case ProcessingType.Async:
                    new DigipostAsync(numberOfRequests, connectionLimit, clientConfig, thumbprint).Run();
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