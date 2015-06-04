using System;
using Digipost.Api.Client;
using Digipost.Api.Client.Api;

namespace ConcurrencyTester
{
    abstract class DigipostRunner
    {

        private readonly Lazy<DigipostClient> _client;

        protected DigipostRunner(ClientConfig clientConfig, string thumbprint)
        {
            _client = new Lazy<DigipostClient>(() => new DigipostClient(clientConfig, thumbprint));
        }

        public abstract void Run();

        public DigipostClient Client
        {
            get { return _client.Value; }
        }
    }
}
