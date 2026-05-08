# Downloading a Media File

Media file codes are returned as part of product attribute values. Use them to fetch metadata or download the binary content.

## Get file metadata

```csharp
var file = await context.GetProductMediaFileAsync("f/i/l/e/your-media-file-code.jpg");

Console.WriteLine($"Code:      {file.Code}");
Console.WriteLine($"MIME type: {file.MimeType}");
Console.WriteLine($"Size:      {file.Size} bytes");
```

## Download binary content

```csharp
byte[] bytes = await context.DownloadProductMediaFileAsync("f/i/l/e/your-media-file-code.jpg");
await File.WriteAllBytesAsync("output.jpg", bytes);

Console.WriteLine($"Saved {bytes.Length} bytes to output.jpg");
```

## Other media download methods

The same pattern applies to other resource types:

```csharp
// Category media
byte[] banner = await context.DownloadCategoryMediaFileAsync("c/7/3/c/your-banner.png");

// Reference entity media
byte[] logo = await context.DownloadReferenceEntityMediaFileAsync("f/f/c/f/your-logo.png");

// Asset media
byte[] asset = await context.DownloadAssetMediaFileAsync("path/to/your-asset.jpg");
```
