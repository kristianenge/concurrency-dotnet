using System;

namespace ConcurrencyTester
{
    internal class Program
    {


        public static void Main()
        {
            int numberOfRequests = 100;
            int connectionPool = 4;
            
            //webServer(numberOfRequests, connectionPool);
            digipost(numberOfRequests, connectionPool);

            Console.ReadKey();
        }

        private static void digipost(int numberOfRequests, int connectionPool)
        {
            DigipostAsync da = new DigipostAsync(numberOfRequests, connectionPool);
            da.TestAsync();
        }

        private static void webServer(int numberOfRequests, int connectionPool)
        {
            WebGetAsync wa = new WebGetAsync(numberOfRequests, connectionPool);
            wa.TestAsync();
        }
    }
}
