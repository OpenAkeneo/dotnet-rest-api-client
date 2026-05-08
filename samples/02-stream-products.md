# Streaming Products with a Search Filter

`StreamProductUuidsAsync` returns an `IAsyncEnumerable<T>` that fetches pages from the Akeneo API on demand. This is the most memory-efficient way to iterate large catalogs — only one page is held in memory at a time.

## Stream all products

```csharp
await foreach (var product in context.StreamProductUuidsAsync())
{
    Console.WriteLine($"{product.Uuid} — family: {product.Family}");
}
```

## Stream with a search filter

Filters use the [Akeneo search syntax](https://api.akeneo.com/documentation/filter.html) encoded as a JSON string:

```csharp
var search = """{"enabled":[{"operator":"=","value":true}]}""";

await foreach (var product in context.StreamProductUuidsAsync(search: search))
{
    Console.WriteLine($"{product.Uuid} — family: {product.Family}");
}
```

## Other access patterns

When you don't need streaming, two alternatives are available:

```csharp
// Single page
var page = await context.GetProductUuidListAsync(page: 1, limit: 100);

// All pages buffered into a list (use with caution on large catalogs)
var all = await context.GetProductUuidListFullAsync();
```
