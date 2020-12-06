using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SpotifyLib.Authenticators
{
    internal class AuthenticatedHttpClientHandler : HttpClientHandler
    {
        private readonly Func<string> getToken;

        public AuthenticatedHttpClientHandler(Func<string> getToken)
        {
            this.getToken = getToken ?? throw new ArgumentNullException(nameof(getToken));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = getToken();
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",
                token);
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}