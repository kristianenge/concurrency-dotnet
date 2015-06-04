using System;
using System.Diagnostics;
using System.Resources;
using System.Security.Cryptography;
using System.Threading;
using ApiClientShared;
using Digipost.Api.Client;
using Digipost.Api.Client.Api;
using Digipost.Api.Client.Domain;
using Digipost.Api.Client.Domain.Exceptions;

namespace ConcurrencyTester
{
    abstract class DigipostRunner
    {

        private readonly Lazy<DigipostClient> _client;
        public Stopwatch Stopwatch;
        private ResourceUtility _resourceManager;

        private int _failedCalls;
        private int _successfulCalls;
        private int _itemsLeft;


        protected DigipostRunner(ClientConfig clientConfig, string thumbprint, int numOfRuns)
        {
            _client = new Lazy<DigipostClient>(() => new DigipostClient(clientConfig, thumbprint));
            _resourceManager = new ResourceUtility("ConcurrencyTester.Resources");
            Stopwatch = new Stopwatch();
            _itemsLeft = numOfRuns;
        }

        public int RunsLeft()
        {
            return Interlocked.Decrement(ref _itemsLeft);
        }

        public abstract void Run();

        public DigipostClient Client
        {
            get { return _client.Value; }
        }

        private Message _message;
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

        private byte[] _documentBytes;
        private byte[] GetDocumentBytes()
        {
            return _documentBytes 
                    ?? (_documentBytes = _resourceManager.ReadAllBytes(true, "Hoveddokument.txt"));
            
        }

        private readonly object _syncLock = new object();
        public async void SendMessageToPerson(DigipostClient api)
        {
            var message = GetMessage();

            try
            {
                await api.SendMessageAsync(message);
                Interlocked.Increment(ref _successfulCalls);

            }
            catch (ClientResponseException e)
            {
                Interlocked.Increment(ref _failedCalls);
                var errorMessage = e.Error;
            }
            catch (Exception e)
            {
                Interlocked.Increment(ref _failedCalls);
            }

            Console.Write(".");
        }

        protected void DisplayTestResults()
        {
            var performanceAllWork = _successfulCalls / (Stopwatch.ElapsedMilliseconds / 1000d);

            Console.WriteLine(
                "Success:" + _successfulCalls + ", " +
                "Failed:" + _failedCalls + ", " +
                "Duration:" + Stopwatch.ElapsedMilliseconds + ", " +
                "Performance full run:" + performanceAllWork.ToString("#.###") + " req/sec");
        }
    }
}
