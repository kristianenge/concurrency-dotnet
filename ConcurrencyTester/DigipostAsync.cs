using System;
using System.Net;
using ApiClientShared;
using Digipost.Api.Client;
using Digipost.Api.Client.Api;
using Digipost.Api.Client.Domain;
using Digipost.Api.Client.Domain.Enums;
using Digipost.Api.Client.Domain.Exceptions;

namespace ConcurrencyTester
{
    internal class DigipostAsync : DigipostRunner
    {
        
        private readonly int _defaultConnectionLimit;
        private readonly int _numberOfRequests;
        
        private long _sumActualSendTime;
        private ResourceUtility _resourceManager;

        public DigipostAsync(int numberOfRequests, int defaultConnectionLimit, ClientConfig clientconfig, string thumbprint) : 
            base(clientconfig, thumbprint, numberOfRequests)
        {
            _resourceManager = new ResourceUtility("ConcurrencyTester.Resources");
            
            _numberOfRequests = numberOfRequests;
            _defaultConnectionLimit = defaultConnectionLimit;
        }

        public override void Run()
        {
            Stopwatch.Start();
            ServicePointManager.DefaultConnectionLimit = _defaultConnectionLimit;

            while(RunsLeft() > 0)
            {
                SendMessageToPerson(Client);
            }

            Stopwatch.Stop();
            DisplayTestResults();
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

        private static void WriteToConsoleWithColor(string message, bool isError = false)
        {
            Console.ForegroundColor = isError ? ConsoleColor.Red : ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}