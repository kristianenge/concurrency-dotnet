using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
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

        public DigipostParalell(int numberOfRequests, int defaultConnectionLimit)
        {
            _itemsLeft = numberOfRequests;
            _numberOfRequests = numberOfRequests;
            _defaultConnectionLimit = defaultConnectionLimit;
        }

        public void TestParallel()
        {
            
            ServicePointManager.DefaultConnectionLimit = _defaultConnectionLimit;
            var config = new ClientConfig(SenderId) { ApiUrl = new Uri("https://qa2.api.digipost.no") };
            var api = new DigipostClient(config, Thumbprint);
            for (var i = 0; i < _numberOfRequests; i++)
            {
                Task.Run(() =>
                {

                    try
                    {
                        //PerformWebRequestGet();
                        SendMessageToPerson(api);
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
                        Console.WriteLine("fin: "+_utcEndTime);
                        if (_itemsLeft != 0) return;
                        _utcEndTime = DateTime.UtcNow;
                        Console.WriteLine("avg:" + (_successfulCalls / _stopwatch.Elapsed.Seconds));
       
                    }
                });
            }
        }

        private void PerformWebRequestGet()
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;

            try
            {
                request = (HttpWebRequest)WebRequest.Create("http://10.0.49.54:3000/");
                request.Method = "GET";
                request.KeepAlive = true;
                response = (HttpWebResponse)request.GetResponse();
            }
            finally
            {
                if (response != null) response.Close();
            }
        }

       

        private void SendMessageToPerson(DigipostClient api)
        {
            var message = GetMessage();
            
            api.SendMessageAsync(message);
         
        }


        private Message GetMessage()
        {
            //primary document

            var primaryDocument = new Document( "document subject","txt", File.ReadAllBytes(@"\\vmware-host\Shared Folders\Development\Hoveddokument.txt"));
        
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

