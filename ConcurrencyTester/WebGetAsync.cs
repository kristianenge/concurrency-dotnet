using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;
using Digipost.Api.Client.Api;

namespace ConcurrencyTester
{
    class WebGetAsync
    {
        private int _successfulCalls;
        private int _failedCalls;
        private readonly object _syncLock = new object();
        private int _itemsLeft;
        private readonly int _numberOfRequests;
        private readonly int _defaultConnectionLimit;
        

        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();


        public WebGetAsync(int numberOfRequests,int defaultConnectionLimit)
        {
            _defaultConnectionLimit = defaultConnectionLimit;
            _itemsLeft = numberOfRequests;
            _numberOfRequests = numberOfRequests;
        }

        private void DisplayTestResults()
        {
            _stopwatch.Stop();


            Console.WriteLine("Success:" + _successfulCalls + " , Failed:" + _failedCalls + ", Duration:" +
                _stopwatch.ElapsedMilliseconds + " Avg:" + (_stopwatch.Elapsed.Seconds==0?_successfulCalls:(_successfulCalls / _stopwatch.Elapsed.Seconds)) + " req/sec");
        }

      

        public async void TestAsync( )
        {
            ServicePointManager.DefaultConnectionLimit = _defaultConnectionLimit;
            HttpClient httpClient = new HttpClient();
            
            
            for (int i = 0; i < _numberOfRequests; i++)
            {
                ProcessUrlAsync(httpClient);
            }
        }


        private async void ProcessUrlAsync(HttpClient httpClient)
        {
            HttpResponseMessage httpResponse = null;

            try
            {
                //string URL = "http://vg.no";
                string URL = "http://10.0.49.54:3000/";
                System.Console.WriteLine("AsyncGet to "+URL);
                Task<HttpResponseMessage> getTask = httpClient.GetAsync(URL);
                httpResponse = await getTask;

                Interlocked.Increment(ref _successfulCalls);
                System.Console.WriteLine("Success");
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _failedCalls);
                System.Console.WriteLine("Failed");
            }
            finally
            {
                if (httpResponse != null) httpResponse.Dispose();
            }


            lock (_syncLock)
            {
                _itemsLeft--;
                if (_itemsLeft == 0)
                {
                    
                    this.DisplayTestResults();
                }
            }
        }
    }

}
