using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ApiClientShared;
using Digipost.Api.Client;
using Digipost.Api.Client.Api;
using Digipost.Api.Client.Domain;
using Digipost.Api.Client.Domain.Enums;
using Digipost.Api.Client.Domain.Exceptions;
using Digipost.Api.Client.Domain.Print;

namespace ConcurrencyTester
{
    internal class DigipostAsync
    {
        private const string SenderId = "106799002"; //"779052"; 
        //private static readonly string Thumbprint = "84e492a972b7edc197a32d9e9c94ea27bd5ac4d9".ToUpper();
        private static readonly string Thumbprint = "7d fc c9 8b 88 55 16 4d 03 a3 64 a4 90 98 26 9d 23 31 4d 0f";

        private readonly object _syncLock = new object();
        private readonly int _defaultConnectionLimit;
        private readonly int _numberOfRequests;
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private int _failedCalls;
        private int _itemsLeft;
        private int _successfulCalls;
        private long _sumActualSendTime;
        private ResourceUtility _resourceManager;

        public DigipostAsync(int numberOfRequests, int defaultConnectionLimit)
        {
            _resourceManager = new ResourceUtility("ConcurrencyTester.Resources");
            _itemsLeft = numberOfRequests;
            _numberOfRequests = numberOfRequests;
            _defaultConnectionLimit = defaultConnectionLimit;
        }

        public void TestAsync()
        {
            var config = new ClientConfig(SenderId) {ApiUrl = new Uri("https://qa2.api.digipost.no")};
            var api = new DigipostClient(config, Thumbprint);
            ServicePointManager.DefaultConnectionLimit = _defaultConnectionLimit;

            for (var i = 0; i < _numberOfRequests; i++)
            {
                SendMessageToPerson(api);
                
            }
        }

        private void DisplayTestResults()
        {
            _stopwatch.Stop();

            double performanceAllWork = _successfulCalls / (_stopwatch.ElapsedMilliseconds/1000d);
            double performanceRequests = _successfulCalls/(_sumActualSendTime/1000d);

            Console.WriteLine(
                "Success:" + _successfulCalls + ", " +
                "Failed:" + _failedCalls + ", " +
                "Duration:" + _stopwatch.ElapsedMilliseconds + ", " +
                "Performance full run:" + performanceAllWork.ToString("#.###") + " req/sec, " + " Performance request:" + performanceRequests.ToString("#.###") + " req/sec");
        }

        private async void SendMessageToPerson(DigipostClient api)
        {
            Console.WriteLine("Sending to person");
            var perRequestTotal = Stopwatch.StartNew();
            
            var message = GetMessage();
            var afterGetMessage = perRequestTotal.ElapsedMilliseconds;
            var actualSendtime = Stopwatch.StartNew();
            
            try
            {
                await api.SendMessageAsync(message);
                Interlocked.Increment(ref _successfulCalls);
                Interlocked.Add(ref _sumActualSendTime, actualSendtime.ElapsedMilliseconds);
                WriteToConsoleWithColor("> Alt gikk fint! GetMessage MS:" + afterGetMessage + " Send Ms:" + (int)actualSendtime.ElapsedMilliseconds, false);
            }
            catch (ClientResponseException e)
            {
                Interlocked.Increment(ref _failedCalls);
                var errorMessage = e.Error;
                WriteToConsoleWithColor("> Error." + errorMessage + ", GetMessage MS:" + afterGetMessage + " Send Ms:" + (int)actualSendtime.ElapsedMilliseconds, true);
            }
            catch (Exception e)
            {
                Interlocked.Increment(ref _failedCalls);
                WriteToConsoleWithColor("> Oh snap... " + e.Message+ ", GetMessage MS:" +afterGetMessage + " Send Ms:" + (int)actualSendtime.ElapsedMilliseconds, true);
            }
            lock (_syncLock)
            {
                _itemsLeft--;
                if (_itemsLeft == 0)
                {
                    DisplayTestResults();
                }
            }
        }

        private static async void IdentifyPerson(DigipostClient api)
        {
            var identification = new Identification(IdentificationChoice.PersonalidentificationNumber, "31108446911");

            try
            {
                await api.IdentifyAsync(identification);
                WriteToConsoleWithColor("> Personen ble identifisert!", false);
            }
            catch (ClientResponseException e)
            {
                var errorMessage = e.Error;
                WriteToConsoleWithColor("> Feilet." + errorMessage, true);
            }
            catch (Exception e)
            {
                WriteToConsoleWithColor("> Oh snap... " + e.Message, true);
            }
        }

        private Message GetMessage()
        {
            //primary document

            var primaryDocument = new Document( "document subject","txt", _resourceManager.ReadAllBytes(true,"Hoveddokument.txt"));
            //attachment
            //var attachment = new Document("Attachment", "pdf", path: @"\\vmware-host\Shared Folders\Development\Vedlegg.pdf");

            //printdetails for fallback to print (physical mail)
            //var printDetails =
            //    new PrintDetails(
            //        new PrintRecipient("Kristian Sæther Enge", new NorwegianAddress("0460", "Oslo", "Colletts gate 68")),
            //        new PrintReturnAddress("Kristian Sæther Enge",
            //            new NorwegianAddress("0460", "Oslo", "Colletts gate 68"))
            //        );


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

      

        private static void WriteToConsoleWithColor(string message, bool isError = false)
        {
            Console.ForegroundColor = isError ? ConsoleColor.Red : ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}