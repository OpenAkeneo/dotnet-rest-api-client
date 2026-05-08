using OpenAkeneo.RestApiClient.Models;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace OpenAkeneo.RestApiClient
{
    public partial class AkeneoContext
    {

        #region Channel

        public async IAsyncEnumerable<Channel> StreamChannelsAsync(bool withCount = false, [EnumeratorCancellation] CancellationToken ct = default)
        {
            for (int page = 1; ; page++)
            {
                var partial = await GetChannelListAsync(page, 100, withCount, ct).ConfigureAwait(false);
                foreach (var item in partial.Channels)
                    yield return item;

                if (string.IsNullOrEmpty(partial.Links?.Next?.Href))
                    yield break;
            }
        }

        public async Task<List<Channel>> GetChannelListFullAsync(bool withCount = false, CancellationToken ct = default)
        {
            var list = new List<Channel>();
            await foreach (var item in StreamChannelsAsync(withCount, ct))
                list.Add(item);
            return list;
        }

        public async Task<ChannelList> GetChannelListAsync(int page = 1, int limit = 100, bool withCount = false, CancellationToken ct = default)
        {
            AkeneoContextHelpers.ValidatePagination(page, limit);

            var queryParameters = new Dictionary<string, string>();

            queryParameters.Add("page", page.ToString());
            queryParameters.Add("limit", limit.ToString());
            queryParameters.Add("with_count", withCount ? "true" : "false");

            return await GetChannelListAsync(queryParameters, ct).ConfigureAwait(false);
        }

        public async Task<ChannelList> GetChannelListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/channels";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<Channel>(responseString, url);

            return new ChannelList { Links = links, Channels = items };
        }

        public async Task<Channel> GetChannelAsync(string channelCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/channels/{Uri.EscapeDataString(channelCode)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<Channel>(responseString, url);
        }

        public async Task<Channel> CreateOrUpdateChannelAsync(Channel channel, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/channels/{Uri.EscapeDataString(channel.Code)}";
            var body = JsonSerializer.Serialize(channel);

            return await PatchAndFetchAsync(url, body, () => GetChannelAsync(channel.Code, ct), ct).ConfigureAwait(false);
        }

        #endregion

        #region Locale

        public async IAsyncEnumerable<Locale> StreamLocalesAsync(bool withCount = false, [EnumeratorCancellation] CancellationToken ct = default)
        {
            for (int page = 1; ; page++)
            {
                var partial = await GetLocaleListAsync(page, 100, withCount, ct).ConfigureAwait(false);
                foreach (var item in partial.Locales)
                    yield return item;

                if (string.IsNullOrEmpty(partial.Links?.Next?.Href))
                    yield break;
            }
        }

        public async Task<List<Locale>> GetLocaleListFullAsync(bool withCount = false, CancellationToken ct = default)
        {
            var list = new List<Locale>();
            await foreach (var item in StreamLocalesAsync(withCount, ct))
                list.Add(item);
            return list;
        }

        public async Task<LocaleList> GetLocaleListAsync(int page = 1, int limit = 100, bool withCount = false, CancellationToken ct = default)
        {
            AkeneoContextHelpers.ValidatePagination(page, limit);

            var queryParameters = new Dictionary<string, string>();

            queryParameters.Add("page", page.ToString());
            queryParameters.Add("limit", limit.ToString());
            queryParameters.Add("with_count", withCount ? "true" : "false");

            return await GetLocaleListAsync(queryParameters, ct).ConfigureAwait(false);
        }

        public async Task<LocaleList> GetLocaleListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/locales";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<Locale>(responseString, url);

            return new LocaleList { Links = links, Locales = items };
        }

        public async Task<Locale> GetLocaleAsync(string localeCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/locales/{Uri.EscapeDataString(localeCode)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<Locale>(responseString, url);
        }

        #endregion

        #region Currency

        public async IAsyncEnumerable<Currency> StreamCurrenciesAsync(bool withCount = false, [EnumeratorCancellation] CancellationToken ct = default)
        {
            for (int page = 1; ; page++)
            {
                var partial = await GetCurrencyListAsync(page, 100, withCount, ct).ConfigureAwait(false);
                foreach (var item in partial.Currencies)
                    yield return item;

                if (string.IsNullOrEmpty(partial.Links?.Next?.Href))
                    yield break;
            }
        }

        public async Task<List<Currency>> GetCurrencyListFullAsync(bool withCount = false, CancellationToken ct = default)
        {
            var list = new List<Currency>();
            await foreach (var item in StreamCurrenciesAsync(withCount, ct))
                list.Add(item);
            return list;
        }

        public async Task<CurrencyList> GetCurrencyListAsync(int page = 1, int limit = 100, bool withCount = false, CancellationToken ct = default)
        {
            AkeneoContextHelpers.ValidatePagination(page, limit);

            var queryParameters = new Dictionary<string, string>();

            queryParameters.Add("page", page.ToString());
            queryParameters.Add("limit", limit.ToString());
            queryParameters.Add("with_count", withCount ? "true" : "false");

            return await GetCurrencyListAsync(queryParameters, ct).ConfigureAwait(false);
        }

        public async Task<CurrencyList> GetCurrencyListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/currencies";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<Currency>(responseString, url);

            return new CurrencyList { Links = links, Currencies = items };
        }

        public async Task<Currency> GetCurrencyAsync(string currencyCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/currencies/{Uri.EscapeDataString(currencyCode)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<Currency>(responseString, url);
        }

        #endregion

        #region Measurement family

        /// <summary>
        /// Returns the full list of measurement families.
        /// The Akeneo API returns all families in a single non-paginated array, so no paging is required.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>All <see cref="MeasurementFamily"/> objects defined in the catalog.</returns>
        public async Task<List<MeasurementFamily>> GetMeasurementFamilyListAsync(CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/measurement-families";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<List<MeasurementFamily>>(responseString, url);
        }

        #endregion

    }
}
