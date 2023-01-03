using System.Net;
using System.Net.Http.Json;
using ZeroDocSigner.Models;

namespace ZeroDocSigner.Cli
{
    public class Client : IDisposable
    {
        private readonly Uri baseUri;
        private readonly HttpClient client = new();

        public Client(string baseUri)
        {
            this.baseUri = new Uri(baseUri);
        }

        public async Task<byte[]> SignAsync(SignModel signModel,
            CancellationToken cancellationToken = default)
        {
            var response = await client.PostAsJsonAsync(
                new Uri(baseUri, "/sign"),
                signModel,
                cancellationToken);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new HttpRequestException(
                    await response.Content.ReadAsStringAsync(cancellationToken));
            }

            var data = await response.Content.ReadFromJsonAsync<byte[]>(
                cancellationToken: cancellationToken);

            return data!;
        }

        public async Task<bool> VerifyAsync(DataModel dataModel,
            CancellationToken cancellationToken = default)
        {
            var response = await client.PostAsJsonAsync(
                new Uri(baseUri, "/verify"),
                dataModel,
                cancellationToken);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new HttpRequestException(
                    await response.Content.ReadAsStringAsync(cancellationToken));
            }

            return await response.Content.ReadFromJsonAsync<bool>(
                cancellationToken: cancellationToken);
        }

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    client.Dispose();
                }

                disposed = true;
            }
        }
    }
}
