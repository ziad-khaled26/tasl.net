using SkiaSharp;
namespace CinemaBooking.Services;
public class ImageService(IWebHostEnvironment environment)
{
    public async Task<string?> Save(IFormFile? file)
    {
        if (file is null || file.Length == 0) return null;
        if (file.Length > 3 * 1024 * 1024) throw new InvalidDataException("The image must be 3 MB or smaller.");
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        SKEncodedImageFormat? expected = extension switch {
            ".jpg" or ".jpeg" => SKEncodedImageFormat.Jpeg,
            ".png" => SKEncodedImageFormat.Png,
            ".webp" => SKEncodedImageFormat.Webp,
            _ => null
        };
        if (expected is null) throw new InvalidDataException("Only JPEG, PNG and WebP raster images are allowed.");
        await using var input = file.OpenReadStream();
        using var memory = new MemoryStream();
        await input.CopyToAsync(memory);
        using var data = SKData.CreateCopy(memory.ToArray());
        using var codec = SKCodec.Create(data);
        if (codec is null || codec.EncodedFormat != expected) throw new InvalidDataException("The image signature does not match its extension or the image is invalid.");
        var info = codec.Info;
        if (info.Width < 1 || info.Height < 1 || info.Width > 6000 || info.Height > 6000 || (long)info.Width * info.Height > 16000000)
            throw new InvalidDataException("Use an image under 16 megapixels and 6000 pixels per side.");
        using var bitmap = new SKBitmap(info.Width, info.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
        if (codec.GetPixels(bitmap.Info, bitmap.GetPixels()) != SKCodecResult.Success) throw new InvalidDataException("The image is corrupt or incomplete.");
        using var image = SKImage.FromBitmap(bitmap);
        using var encoded = image.Encode(SKEncodedImageFormat.Jpeg, 90);
        if (encoded is null) throw new InvalidDataException("The image could not be processed.");
        var directory = Path.Combine(environment.WebRootPath, "uploads");
        Directory.CreateDirectory(directory);
        var filename = $"{Guid.NewGuid():N}.jpg";
        await using var output = File.Create(Path.Combine(directory, filename));
        encoded.SaveTo(output);
        return $"/uploads/{filename}";
    }
}
