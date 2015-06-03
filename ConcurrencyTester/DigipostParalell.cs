﻿using System;
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
    internal class DigipostParalell
    {
        private const string SenderId = "106799002"; //"779052"; 
        //private static readonly string Thumbprint = "84e492a972b7edc197a32d9e9c94ea27bd5ac4d9".ToUpper();
        private static readonly string Thumbprint = "7d fc c9 8b 88 55 16 4d 03 a3 64 a4 90 98 26 9d 23 31 4d 0f";

        private readonly int _defaultConnectionLimit;
        private readonly int _numberOfRequests;
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private int _itemsLeft;
        private int _successfulCalls;
        private int _failedCalls;
        private object _syncLock = new object();
        private DateTime _utcEndTime;
        private ResourceUtility _resourceManager;
        private long _sumActualSendTime;

        public DigipostParalell(int numberOfRequests, int defaultConnectionLimit)
        {
            _resourceManager = new ResourceUtility("ConcurrencyTester.Resources");
            _itemsLeft = numberOfRequests;
            _numberOfRequests = numberOfRequests;
            _defaultConnectionLimit = defaultConnectionLimit;
        }

        private void DisplayTestResults()
        {
            _stopwatch.Stop();

            double performanceAllWork = _successfulCalls / (_stopwatch.ElapsedMilliseconds / 1000d);

            Console.WriteLine(
                "Success:" + _successfulCalls + ", " +
                "Failed:" + _failedCalls + ", " +
                "Duration:" + _stopwatch.ElapsedMilliseconds + ", " +
                "Performance full run:" + performanceAllWork.ToString("#.###") + " req/sec, " );
        }

        public void TestParallel()
        {

            ServicePointManager.DefaultConnectionLimit = _defaultConnectionLimit;
            var config = new ClientConfig(SenderId) {ApiUrl = new Uri("https://qa2.api.digipost.no")};
            var api = new DigipostClient(config, Thumbprint);

            
                List<Message> messages = new List<Message>();
                for (var i = 0; i < _numberOfRequests; i++)
                {
                    messages.Add(GetMessage());
                }

            {
                for (var i = 0; i < _numberOfRequests; i++)
                {
                    var i1 = i;
                    Task.Run(() =>
                    {

                        try
                        {
                            //PerformWebRequestGet();
                            SendMessageToPerson(api, messages.ElementAt(i1));
                            Interlocked.Increment(ref _successfulCalls);
                        }
                        catch (Exception ex)
                        {
                            Interlocked.Increment(ref _failedCalls);
                        }

                        lock (_syncLock)
                        {
                            _itemsLeft--;
                            _utcEndTime = DateTime.UtcNow;
                            Console.WriteLine("fin: " + _utcEndTime);
                            if (_itemsLeft != 0) return;
                            _utcEndTime = DateTime.UtcNow;
                            DisplayTestResults();

                        }
                    });
                }
            }
        }

        public void AleksanderParallel()
        {
            Console.WriteLine("Starter aleksanderparallell");
             ServicePointManager.DefaultConnectionLimit = _defaultConnectionLimit;
            var config = new ClientConfig(SenderId) {ApiUrl = new Uri("https://qa2.api.digipost.no")};
            var digipostClient = new DigipostClient(config, Thumbprint);

            List<Message> messages = new List<Message>();
            for (var i = 0; i < _numberOfRequests; i++)
            {
                messages.Add(GetMessage());
            }


            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = _defaultConnectionLimit;

            Parallel.ForEach(messages, message => AleksanderParallelHelper(digipostClient,message));
            Console.WriteLine("Ferdig med aleksanderparallell:"+(_numberOfRequests/(_sumActualSendTime/1000d)+" req/sec"));
        }

        private void AleksanderParallelHelper(DigipostClient client, Message message)
        {
            var actualSendtime = Stopwatch.StartNew();
            Thread.Sleep(200);
            //client.SendMessage(message);
            Interlocked.Add(ref _sumActualSendTime, actualSendtime.ElapsedMilliseconds);
            Console.WriteLine("Sendte en aleksandermelding:"+actualSendtime.ElapsedMilliseconds+" ms");
            actualSendtime.Stop();
        }

        private void PerformWebRequestGet()
        {
            var actualSendtime = Stopwatch.StartNew();
            HttpWebRequest request = null;
            HttpWebResponse response = null;

            try
            {
                request = (HttpWebRequest)WebRequest.Create("http://10.16.0.125:3000/");
                request.Method = "GET";
                request.KeepAlive = true;
                response = (HttpWebResponse)request.GetResponse();
                Interlocked.Add(ref _sumActualSendTime, actualSendtime.ElapsedMilliseconds);
                Console.WriteLine("Sendte en aleksandermelding:" + actualSendtime.ElapsedMilliseconds + " ms");
                actualSendtime.Stop();
                
            }
            finally
            {
                if (response != null) response.Close();
            }
        }

       

        private void SendMessageToPerson(DigipostClient api,Message message)
        {
            //var message = GetMessage();
            
            api.SendMessage(message);
         
        }


        private Message GetMessage()
        {
            //primary document

            var primaryDocument = new Document( "document subject","txt", _resourceManager.ReadAllBytes(true, "Hoveddokument.txt"));
        
            //recipientIdentifier for digital mail
            var recipientByNameAndAddress = new RecipientByNameAndAddress("Kristian Sæther Enge", "0460",
                "Oslo", "Collettsgate 68");

            //recipient
            var digitalRecipientWithFallbackPrint = new Recipient(recipientByNameAndAddress);

            //message
            var message = new Message(digitalRecipientWithFallbackPrint, primaryDocument);
            //message.Attachments.Add(attachment);

            return message;
        }

      
    }
}
