using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
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
        private const string SenderId = "106768801"; //"779052"; 
        //private static readonly string Thumbprint = "84e492a972b7edc197a32d9e9c94ea27bd5ac4d9".ToUpper();
        private static readonly string Thumbprint = "f7 de 9c 38 4e e6 d0 a8 1d ad 7e 8e 60 bd 37 76 fa 5d e9 f4";

        private readonly object _syncLock = new object();
        private readonly int _defaultConnectionLimit;
        private readonly int _numberOfRequests;
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private int _failedCalls;
        private int _itemsLeft;
        private int _successfulCalls;

        public DigipostAsync(int numberOfRequests, int defaultConnectionLimit)
        {
            _itemsLeft = numberOfRequests;
            _numberOfRequests = numberOfRequests;
            _defaultConnectionLimit = defaultConnectionLimit;
        }

        public void TestAsync()
        {
            var config = new ClientConfig(SenderId) {ApiUrl = new Uri("https://qa.api.digipost.no")};
            

            Logging.Initialize(config);
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

            Console.WriteLine("Success:" + _successfulCalls + " , Failed:" + _failedCalls + ", Duration:" +
                              _stopwatch.ElapsedMilliseconds);
        }

        private void SendMessageToPerson(DigipostClient api)
        {
            Console.WriteLine("======================================");
            Console.WriteLine("Sending message:");
            Console.WriteLine("======================================");
            var message = GetMessage();
            try
            {
                Console.WriteLine("> Starter å sende melding");
                var messageDeliveryResult = api.SendMessage(message);
                Interlocked.Increment(ref _successfulCalls);
                Logging.Log(TraceEventType.Information, "" + messageDeliveryResult);
                WriteToConsoleWithColor("> Alt gikk fint!", false);
            }
            catch (ClientResponseException e)
            {
                Interlocked.Increment(ref _failedCalls);

                var errorMessage = e.Error;
                WriteToConsoleWithColor("> Error." + errorMessage, true);
            }
            catch (Exception e)
            {
                WriteToConsoleWithColor("> Oh snap... " + e.Message, true);
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

        private static void IdentifyPerson(DigipostClient api)
        {
            Console.WriteLine("======================================");
            Console.WriteLine("Identifiserer person:");
            Console.WriteLine("======================================");

            var identification = new Identification(IdentificationChoice.PersonalidentificationNumber, "31108446911");

            try
            {
                var identificationResponse = api.Identify(identification);
                Logging.Log(TraceEventType.Information, "Identification resp: \n" + identificationResponse);
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

        private static Message GetMessage()
        {
            //primary document

            var primaryDocument = new Document(subject: "document subject", mimeType: "pdf", path: @"\\vmware-host\Shared Folders\Development\Hoveddokument.pdf");
            //attachment
            var attachment = new Document("Attachment", "pdf", path: @"\\vmware-host\Shared Folders\Development\Vedlegg.pdf");

            //printdetails for fallback to print (physical mail)
            var printDetails =
                new PrintDetails(
                    new PrintRecipient("Kristian Sæther Enge", new NorwegianAddress("0460", "Oslo", "Colletts gate 68")),
                    new PrintReturnAddress("Kristian Sæther Enge",
                        new NorwegianAddress("0460", "Oslo", "Colletts gate 68"))
                    );


            //recipientIdentifier for digital mail
            var recipientByNameAndAddress = new RecipientByNameAndAddress("Kristian Sæther Enge", "0460",
                "Oslo", "Collettsgate 68");

            //recipient
            var digitalRecipientWithFallbackPrint = new Recipient(recipientByNameAndAddress);

            //message
            var message = new Message(digitalRecipientWithFallbackPrint, primaryDocument);
            message.Attachments.Add(attachment);

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