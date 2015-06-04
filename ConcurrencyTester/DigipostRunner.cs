﻿using System;
using System.Diagnostics;
using System.Resources;
using System.Security.Cryptography;
using System.Threading;
using ApiClientShared;
using ConcurrencyTester.Enums;
using Digipost.Api.Client;
using Digipost.Api.Client.Api;
using Digipost.Api.Client.Domain;
using Digipost.Api.Client.Domain.Enums;
using Digipost.Api.Client.Domain.Exceptions;

namespace ConcurrencyTester
{
    abstract class DigipostRunner
    {
        private readonly Lazy<DigipostClient> _client;
        private readonly ResourceUtility _resourceManager;
        private int _failedCalls;
        private int _successfulCalls;
        private int _itemsLeft;
        private byte[] _documentBytes;
        private Message _message;
        private Identification _identification;

        protected DigipostRunner(ClientConfig clientConfig, string thumbprint, int numOfRuns)
        {
            _client = new Lazy<DigipostClient>(() => new DigipostClient(clientConfig, thumbprint));
            _resourceManager = new ResourceUtility("ConcurrencyTester.Resources");
            Stopwatch = new Stopwatch();
            _itemsLeft = numOfRuns + 1; //Fordi vi decrementer teller før return
        }

        public Stopwatch Stopwatch;

        public int RunsLeft()
        {
           return Interlocked.Decrement(ref _itemsLeft);
        }

        public abstract void Run(RequestType requestType);

        public DigipostClient Client
        {
            get { return _client.Value; }
        }

        private readonly Object _lock = new Object();

        public Message GetMessage()
        {
            lock (_lock)
            {
                if (_message != null) return _message;

                var primaryDocument = new Document("document subject", "txt", GetDocumentBytes());
                var recipientByNameAndAddress = new RecipientByNameAndAddress("Kristian Sæther Enge", "0460",
                    "Oslo", "Collettsgate 68");

                var digitalRecipientWithFallbackPrint = new Recipient(recipientByNameAndAddress);
                _message = new Message(digitalRecipientWithFallbackPrint, primaryDocument);
            }

            return _message;
        }

        public Identification GetIdentification()
        {
            lock (_lock)
            {
                if(_identification != null) return _identification;
                _identification = new Identification(IdentificationChoice.PersonalidentificationNumber, "31108446911");
            }

            return _identification;
        }

        private byte[] GetDocumentBytes()
        {
            return _documentBytes 
                    ?? (_documentBytes = _resourceManager.ReadAllBytes(true, "Hoveddokument.txt"));
            
        }

        public async void Send(DigipostClient digipostClient, RequestType requestType)
        {
            try
            {
                switch (requestType)
                {
                    case RequestType.Message:
                        await digipostClient.SendMessageAsync(GetMessage());
                        break;
                    case RequestType.Identify:
                        await digipostClient.IdentifyAsync(GetIdentification());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("requestType", requestType, null);
                }
                
                Interlocked.Increment(ref _successfulCalls);

            }
            catch (Exception e)
            {
                Interlocked.Increment(ref _failedCalls);
                Console.WriteLine("Request failed. Are you connected to VPN? Reason{0}. Inner: {1}", e.Message, e.InnerException.Message);
                Console.WriteLine(e.InnerException.InnerException);
            }

            Console.Write(".");
        }

        protected void DisplayTestResults()
        {
            var performanceAllWork = _successfulCalls / (Stopwatch.ElapsedMilliseconds / 1000d);

            Console.WriteLine();
            Console.WriteLine(
                "Success:" + _successfulCalls + ", \n" +
                "Failed:" + _failedCalls + " \n" +
                "Duration:" + Stopwatch.ElapsedMilliseconds + " \n" +
                "Performance full run:" + performanceAllWork.ToString("#.###") + " req/sec");
        }
    }
}
