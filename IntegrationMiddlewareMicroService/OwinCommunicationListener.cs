using System;
using System.Fabric;
using System.Fabric.Description;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace IntegrationMiddlewareMicroService
{
    public class OwinCommunicationListener : ICommunicationListener
    {
        private readonly string _appRoot;
        private readonly IOwinAppBuilder _startup;
        private readonly StatelessServiceContext _context;

        private IDisposable _serverHandle;
        private string _listeningAddress;

        public OwinCommunicationListener(string appRoot, IOwinAppBuilder startup, StatelessServiceContext context)
        {
            _appRoot = appRoot;
            _startup = startup;
            _context = context;
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            var serviceEndpoint = _context.CodePackageActivationContext.GetEndpoint("ServiceEndpoint");
            var port = serviceEndpoint.Port;

            _listeningAddress = string.Format(
                CultureInfo.InvariantCulture,
                "http://+:{0}/{1}",
                port,
                string.IsNullOrWhiteSpace(_appRoot)
                    ? string.Empty
                    : _appRoot.TrimEnd('/') + '/');

            _serverHandle = WebApp.Start(_listeningAddress, appBuilder => _startup.Configuration(appBuilder));
            var publishAddress = _listeningAddress.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);

            ServiceEventSource.Current.Message("Listening on {0}", publishAddress);

            return Task.FromResult(publishAddress);
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            ServiceEventSource.Current.Message("Close");
            StopWebServer();
            return Task.FromResult(true);
        }

        public void Abort()
        {
            ServiceEventSource.Current.Message("Abort");
            StopWebServer();
        }

        private void StopWebServer()
        {
            if (_serverHandle != null)
            {
                try
                {
                    _serverHandle.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // no-op
                }
            }
        }
    }
}
