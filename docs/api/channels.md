# Channels — OpenAkeneo.RestApiClient

Methods on `AkeneoContext` for the Channels domain. All methods are async and
accept an optional trailing `CancellationToken ct`. All throw `AkeneoApiException` on
non-success responses. Generated from the compiled v0.9.0 surface — do not edit by hand.

**Domain notes:**
- This page covers channels, locales, currencies, and measurement families.
- Locales and currencies are **read-only** in the API. Channels are not deletable.
- Creating a channel requires existing `CategoryTree`, `Currencies`, and `Locales` codes —
  copy them from an existing channel when unsure.
- `CreateOrUpdateMeasurementFamiliesAsync` is inherently batch: it takes a list and returns the
  raw per-item status JSON (`[{"code":...,"status_code":201|204|4xx,...}]`) — parse it; it does
  not throw per item.

## `StreamChannelsAsync`

```csharp
IAsyncEnumerable<Channel> StreamChannelsAsync(bool withCount = false, CancellationToken ct = default)
```

Streams all channels, following HAL pagination automatically.

- `withCount` — Include total item count in API response.

## `GetChannelListFullAsync`

```csharp
Task<List<Channel>> GetChannelListFullAsync(bool withCount = false, CancellationToken ct = default)
```

Returns all channels as a materialised list.

- `withCount` — Include total item count in API response.

Returns: A list of all `Channel` objects.

## `GetChannelListAsync`

```csharp
Task<ChannelList> GetChannelListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
```

Returns a channel page using an arbitrary set of pre-built query parameters.

- `queryParameters` — Raw query-string key/value pairs sent to the Akeneo API.

Returns: A paginated `ChannelList` with HAL navigation links.

## `GetChannelListAsync`

```csharp
Task<ChannelList> GetChannelListAsync(int page = 1, int limit = 100, bool withCount = false, CancellationToken ct = default)
```

Returns a single page of channels.

- `page` — 1-based page number.
- `limit` — Items per page (1–100).
- `withCount` — Include total item count in API response.

Returns: A paginated `ChannelList` with HAL navigation links.

## `GetChannelAsync`

```csharp
Task<Channel> GetChannelAsync(string channelCode, CancellationToken ct = default)
```

Returns a single channel by its code.

- `channelCode` — The channel code.

Returns: The matching `Channel`.

## `CreateOrUpdateChannelAsync`

```csharp
Task<Channel> CreateOrUpdateChannelAsync(Channel channel, CancellationToken ct = default)
```

Creates or updates a channel via HTTP PATCH then returns the refreshed entity.

- `channel` — The channel to create or update. `Code` must be set.

Returns: The updated `Channel` as returned by the API.

## `CreateChannelAsync`

```csharp
Task<Channel> CreateChannelAsync(Channel channel, CancellationToken ct = default)
```

Creates a new channel via HTTP POST and returns the created entity.

- `channel` — The channel to create.

Returns: The created `Channel`.

## `StreamLocalesAsync`

```csharp
IAsyncEnumerable<Locale> StreamLocalesAsync(bool withCount = false, CancellationToken ct = default)
```

Streams all locales, following HAL pagination automatically.

- `withCount` — Include total item count in API response.

## `GetLocaleListFullAsync`

```csharp
Task<List<Locale>> GetLocaleListFullAsync(bool withCount = false, CancellationToken ct = default)
```

Returns all locales as a materialised list.

- `withCount` — Include total item count in API response.

Returns: A list of all `Locale` objects.

## `GetLocaleListAsync`

```csharp
Task<LocaleList> GetLocaleListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
```

Returns a locale page using an arbitrary set of pre-built query parameters.

- `queryParameters` — Raw query-string key/value pairs sent to the Akeneo API.

Returns: A paginated `LocaleList` with HAL navigation links.

## `GetLocaleListAsync`

```csharp
Task<LocaleList> GetLocaleListAsync(int page = 1, int limit = 100, bool withCount = false, CancellationToken ct = default)
```

Returns a single page of locales.

- `page` — 1-based page number.
- `limit` — Items per page (1–100).
- `withCount` — Include total item count in API response.

Returns: A paginated `LocaleList` with HAL navigation links.

## `GetLocaleAsync`

```csharp
Task<Locale> GetLocaleAsync(string localeCode, CancellationToken ct = default)
```

Returns a single locale by its code.

- `localeCode` — The locale code (e.g. `en_US`).

Returns: The matching `Locale`.

## `StreamCurrenciesAsync`

```csharp
IAsyncEnumerable<Currency> StreamCurrenciesAsync(bool withCount = false, CancellationToken ct = default)
```

Streams all currencies, following HAL pagination automatically.

- `withCount` — Include total item count in API response.

## `GetCurrencyListFullAsync`

```csharp
Task<List<Currency>> GetCurrencyListFullAsync(bool withCount = false, CancellationToken ct = default)
```

Returns all currencies as a materialised list.

- `withCount` — Include total item count in API response.

Returns: A list of all `Currency` objects.

## `GetCurrencyListAsync`

```csharp
Task<CurrencyList> GetCurrencyListAsync(Dictionary<string, string> queryParameters, CancellationToken ct = default)
```

Returns a currency page using an arbitrary set of pre-built query parameters.

- `queryParameters` — Raw query-string key/value pairs sent to the Akeneo API.

Returns: A paginated `CurrencyList` with HAL navigation links.

## `GetCurrencyListAsync`

```csharp
Task<CurrencyList> GetCurrencyListAsync(int page = 1, int limit = 100, bool withCount = false, CancellationToken ct = default)
```

Returns a single page of currencies.

- `page` — 1-based page number.
- `limit` — Items per page (1–100).
- `withCount` — Include total item count in API response.

Returns: A paginated `CurrencyList` with HAL navigation links.

## `GetCurrencyAsync`

```csharp
Task<Currency> GetCurrencyAsync(string currencyCode, CancellationToken ct = default)
```

Returns a single currency by its code.

- `currencyCode` — The ISO 4217 currency code (e.g. `EUR`).

Returns: The matching `Currency`.

## `GetMeasurementFamilyListAsync`

```csharp
Task<List<MeasurementFamily>> GetMeasurementFamilyListAsync(CancellationToken ct = default)
```

Returns the full list of measurement families. The Akeneo API returns all families in a single non-paginated array, so no paging is required.

Returns: All `MeasurementFamily` objects defined in the catalog.

## `CreateOrUpdateMeasurementFamiliesAsync`

```csharp
Task<string> CreateOrUpdateMeasurementFamiliesAsync(List<MeasurementFamily> measurementFamilies, CancellationToken ct = default)
```

Creates or updates measurement families in bulk via HTTP PATCH. The Akeneo API accepts an array of measurement family objects and returns an array of per-item status results.

- `measurementFamilies` — The list of measurement families to create or update.

Returns: Response body string (JSON array with per-item status codes and errors).

