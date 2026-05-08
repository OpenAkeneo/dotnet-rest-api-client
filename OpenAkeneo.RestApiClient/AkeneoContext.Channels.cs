using OpenAkeneo.RestApiClient.Models;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace OpenAkeneo.RestApiClient
{
    public partial class AkeneoContext
    {

        #region Channel

        /// <summary>Streams all channels, following HAL pagination automatically.</summary>
        /// <param name="withCount">Include total item count in API response.</param>
        /// <param name="ct">Cancellation token.</param>
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

        /// <summary>Returns all channels as a materialised list.</summary>
        /// <param name="withCount">Include total item count in API response.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A list of all <see cref="Channel"/> objects.</returns>
        public async Task<List<Channel>> GetChannelListFullAsync(bool withCount = false, CancellationToken ct = default)
        {
            var list = new List<Channel>();
            await foreach (var item in StreamChannelsAsync(withCount, ct))
                list.Add(item);
            return list;
        }

        /// <summary>Returns a single page of channels.</summary>
        /// <param name="page">1-based page number.</param>
        /// <param name="limit">Items per page (1–100).</param>
        /// <param name="withCount">Include total item count in API response.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="ChannelList"/> with HAL navigation links.</returns>
        public async Task<ChannelList> GetChannelListAsync(int page = 1, int limit = 100, bool withCount = false, CancellationToken ct = default)
        {
            AkeneoContextHelpers.ValidatePagination(page, limit);

            var queryParameters = new Dictionary<string, string>();

            queryParameters.Add("page", page.ToString());
            queryParameters.Add("limit", limit.ToString());
            queryParameters.Add("with_count", withCount ? "true" : "false");

            return await GetChannelListAsync(queryParameters, ct).ConfigureAwait(false);
        }

        /// <summary>Returns a channel page using an arbitrary set of pre-built query parameters.</summary>
        /// <param name="queryParameters">Raw query-string key/value pairs sent to the Akeneo API.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="ChannelList"/> with HAL navigation links.</returns>
        public async Task<ChannelList> GetChannelListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/channels";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<Channel>(responseString, url);

            return new ChannelList { Links = links, Channels = items };
        }

        /// <summary>Returns a single channel by its code.</summary>
        /// <param name="channelCode">The channel code.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The matching <see cref="Channel"/>.</returns>
        public async Task<Channel> GetChannelAsync(string channelCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/channels/{Uri.EscapeDataString(channelCode)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<Channel>(responseString, url);
        }

        /// <summary>Creates or updates a channel via HTTP PATCH then returns the refreshed entity.</summary>
        /// <param name="channel">The channel to create or update. <see cref="Channel.Code"/> must be set.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The updated <see cref="Channel"/> as returned by the API.</returns>
        public async Task<Channel> CreateOrUpdateChannelAsync(Channel channel, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/channels/{Uri.EscapeDataString(channel.Code)}";
            var body = JsonSerializer.Serialize(channel);

            return await PatchAndFetchAsync(url, body, () => GetChannelAsync(channel.Code, ct), ct).ConfigureAwait(false);
        }

        #endregion

        #region Locale

        /// <summary>Streams all locales, following HAL pagination automatically.</summary>
        /// <param name="withCount">Include total item count in API response.</param>
        /// <param name="ct">Cancellation token.</param>
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

        /// <summary>Returns all locales as a materialised list.</summary>
        /// <param name="withCount">Include total item count in API response.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A list of all <see cref="Locale"/> objects.</returns>
        public async Task<List<Locale>> GetLocaleListFullAsync(bool withCount = false, CancellationToken ct = default)
        {
            var list = new List<Locale>();
            await foreach (var item in StreamLocalesAsync(withCount, ct))
                list.Add(item);
            return list;
        }

        /// <summary>Returns a single page of locales.</summary>
        /// <param name="page">1-based page number.</param>
        /// <param name="limit">Items per page (1–100).</param>
        /// <param name="withCount">Include total item count in API response.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="LocaleList"/> with HAL navigation links.</returns>
        public async Task<LocaleList> GetLocaleListAsync(int page = 1, int limit = 100, bool withCount = false, CancellationToken ct = default)
        {
            AkeneoContextHelpers.ValidatePagination(page, limit);

            var queryParameters = new Dictionary<string, string>();

            queryParameters.Add("page", page.ToString());
            queryParameters.Add("limit", limit.ToString());
            queryParameters.Add("with_count", withCount ? "true" : "false");

            return await GetLocaleListAsync(queryParameters, ct).ConfigureAwait(false);
        }

        /// <summary>Returns a locale page using an arbitrary set of pre-built query parameters.</summary>
        /// <param name="queryParameters">Raw query-string key/value pairs sent to the Akeneo API.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="LocaleList"/> with HAL navigation links.</returns>
        public async Task<LocaleList> GetLocaleListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/locales";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<Locale>(responseString, url);

            return new LocaleList { Links = links, Locales = items };
        }

        /// <summary>Returns a single locale by its code.</summary>
        /// <param name="localeCode">The locale code (e.g. <c>en_US</c>).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The matching <see cref="Locale"/>.</returns>
        public async Task<Locale> GetLocaleAsync(string localeCode, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/locales/{Uri.EscapeDataString(localeCode)}";

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);

            return AkeneoContextHelpers.DeserializeOrThrow<Locale>(responseString, url);
        }

        #endregion

        #region Currency

        /// <summary>Streams all currencies, following HAL pagination automatically.</summary>
        /// <param name="withCount">Include total item count in API response.</param>
        /// <param name="ct">Cancellation token.</param>
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

        /// <summary>Returns all currencies as a materialised list.</summary>
        /// <param name="withCount">Include total item count in API response.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A list of all <see cref="Currency"/> objects.</returns>
        public async Task<List<Currency>> GetCurrencyListFullAsync(bool withCount = false, CancellationToken ct = default)
        {
            var list = new List<Currency>();
            await foreach (var item in StreamCurrenciesAsync(withCount, ct))
                list.Add(item);
            return list;
        }

        /// <summary>Returns a single page of currencies.</summary>
        /// <param name="page">1-based page number.</param>
        /// <param name="limit">Items per page (1–100).</param>
        /// <param name="withCount">Include total item count in API response.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="CurrencyList"/> with HAL navigation links.</returns>
        public async Task<CurrencyList> GetCurrencyListAsync(int page = 1, int limit = 100, bool withCount = false, CancellationToken ct = default)
        {
            AkeneoContextHelpers.ValidatePagination(page, limit);

            var queryParameters = new Dictionary<string, string>();

            queryParameters.Add("page", page.ToString());
            queryParameters.Add("limit", limit.ToString());
            queryParameters.Add("with_count", withCount ? "true" : "false");

            return await GetCurrencyListAsync(queryParameters, ct).ConfigureAwait(false);
        }

        /// <summary>Returns a currency page using an arbitrary set of pre-built query parameters.</summary>
        /// <param name="queryParameters">Raw query-string key/value pairs sent to the Akeneo API.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A paginated <see cref="CurrencyList"/> with HAL navigation links.</returns>
        public async Task<CurrencyList> GetCurrencyListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
        {
            var url = $"/api/rest/v1/currencies";
            url += AkeneoContextHelpers.BuildQueryString(queryParameters);

            var responseString = await _service.HttpGetAsync(url, ct).ConfigureAwait(false);
            var (links, items) = AkeneoContextHelpers.ParseHalResponse<Currency>(responseString, url);

            return new CurrencyList { Links = links, Currencies = items };
        }

        /// <summary>Returns a single currency by its code.</summary>
        /// <param name="currencyCode">The ISO 4217 currency code (e.g. <c>EUR</c>).</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The matching <see cref="Currency"/>.</returns>
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
