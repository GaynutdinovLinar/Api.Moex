using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Moex
{
    /// <summary>
    /// Класс для осуществления запросов к ISS и удобного получения ответа.
    /// </summary>
    public class Moex : IDisposable
    {
        private readonly bool _internalCreatedHttpClient = false;

        public Moex(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public Moex() : this(new HttpClient())
        {
            _internalCreatedHttpClient = true;
        }

        public HttpClient HttpClient { get; }

        public void Dispose()
        {
            if (_internalCreatedHttpClient) HttpClient.Dispose();
        }

        /// <summary>
        /// Выполнить запрос и получить ответ.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(string requestUri) where T : MoexResponse => await GetAsync<T>(requestUri, CancellationToken.None);

        /// <summary>
        /// Выполнить запрос и получить ответ.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(string requestUri, CancellationToken cancellationToken) where T : MoexResponse
        {
            var response = await HttpClient.GetAsync(requestUri, cancellationToken);
            var content = await response.Content.ReadAsStringAsync();
            return Activator.CreateInstance(typeof(T), args: content) as T;
        }
    }
}