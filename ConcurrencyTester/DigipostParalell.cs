using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using ApiClientShared;
using Digipost.Api.Client;
using Digipost.Api.Client.Api;
using Digipost.Api.Client.Domain;

namespace ConcurrencyTester
{
    internal class DigipostParalell : DigipostRunner
    {
        private readonly int _defaultConnectionLimit;
        private readonly int _degreeOfParallelism;
       
        public DigipostParalell(int numberOfRequests, int defaultConnectionLimit,int degreeOfParallelism, ClientConfig clientConfig, string thumbprint) :
            base(clientConfig, thumbprint, numberOfRequests)
        {
            
            _defaultConnectionLimit = defaultConnectionLimit;
            _degreeOfParallelism = degreeOfParallelism;
        }
        
        public override void Run()
        {
            Stopwatch.Start();
            ServicePointManager.DefaultConnectionLimit = _defaultConnectionLimit;
            
            List<Message> messages = new List<Message>();
            while (RunsLeft() > 0)
            {
                messages.Add(GetMessage());
            }

            var options = new ParallelOptions {MaxDegreeOfParallelism = _degreeOfParallelism};
            Parallel.ForEach(messages, options, (message) => AleksanderParallelHelper());
            
            Stopwatch.Stop();
            DisplayTestResults();
        }

        private void AleksanderParallelHelper()
        {
            try
            {
                SendMessageToPerson(Client);
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception:" + e.Message + "InnerEx:" + e.InnerException.Message);
            }
            finally
            {
                Console.Write(".");
            }
        }
    }
}

