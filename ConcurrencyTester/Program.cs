using System;

namespace ConcurrencyTester
{
    internal class Program
    {


        public static void Main()
        {
            int numberOfRequests = 10;
            int connectionPool = 10;
            //WebGetAsync wa = new WebGetAsync(numberOfRequests,connectionPool);
            //wa.TestAsync();

            DigipostAsync da = new DigipostAsync(numberOfRequests,connectionPool);
            da.TestAsync();

            Console.ReadKey();
        }


    }
}
