namespace StaffManagementApp.Services;

public class LocalFileStorageService(IWebHostEnvironment env) : IFileStorageService
{
    private const string AvatarFolder = "uploads/avatars";
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 МБ

    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    // Magic bytes для проверки реального формата файла
    private static readonly byte[] JpegMagic = { 0xFF, 0xD8, 0xFF };
    private static readonly byte[] PngMagic  = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
    private static readonly byte[] RiffMagic = { 0x52, 0x49, 0x46, 0x46 }; // "RIFF"
    private static readonly byte[] WebpMagic = { 0x57, 0x45, 0x42, 0x50 }; // "WEBP" (bytes 8-11)

    public async Task<string> SavePhotoAsync(Stream stream, string fileName, string contentType)
    {
        // 1. Проверка расширения
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new InvalidOperationException(
                "Ошибка: разрешена загрузка только фотографий форматов JPEG, PNG или WEBP.");

        // 2. Проверка MIME-типа
        if (string.IsNullOrEmpty(contentType) ||
            !contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "Ошибка: разрешена загрузка только фотографий форматов JPEG, PNG или WEBP.");

        // 3. Читаем весь поток в MemoryStream (Blazor PipeStream не поддерживает .Length)
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        var data = ms.ToArray();

        // 4. Проверка размера
        if (data.Length > MaxFileSizeBytes)
            throw new InvalidOperationException("Размер файла не должен превышать 5 МБ.");

        // 5. Проверка магических байт (защита от подмены расширения)
        if (!IsValidImageSignature(data))
            throw new InvalidOperationException(
                "Ошибка: разрешена загрузка только фотографий форматов JPEG, PNG или WEBP.");

        // 6. Сохранение файла
        var uniqueName = $"{Guid.NewGuid()}{ext}";
        var folder = Path.Combine(env.WebRootPath, AvatarFolder);
        Directory.CreateDirectory(folder);
        var fullPath = Path.Combine(folder, uniqueName);

        await File.WriteAllBytesAsync(fullPath, data);

        return $"{AvatarFolder}/{uniqueName}";
    }

    private static bool IsValidImageSignature(byte[] data)
    {
        if (data.Length < 3) return false;

        // JPEG: FF D8 FF
        if (StartsWith(data, JpegMagic)) return true;

        // PNG: 89 50 4E 47 0D 0A 1A 0A (минимум 8 байт)
        if (data.Length >= 8 && StartsWith(data, PngMagic)) return true;

        // WebP: RIFF????WEBP (минимум 12 байт)
        if (data.Length >= 12 && StartsWith(data, RiffMagic))
        {
            // Байты 8-11 должны быть "WEBP"
            if (data[8] == WebpMagic[0] && data[9] == WebpMagic[1] &&
                data[10] == WebpMagic[2] && data[11] == WebpMagic[3])
                return true;
        }

        return false;
    }

    private static bool StartsWith(byte[] data, byte[] prefix)
    {
        if (data.Length < prefix.Length) return false;
        for (int i = 0; i < prefix.Length; i++)
            if (data[i] != prefix[i]) return false;
        return true;
    }

    public string GetPhotoUrl(string? photoPath, string gender)
    {
        if (!string.IsNullOrEmpty(photoPath))
            return $"/{photoPath}";
        return gender == "Female" ? "/images/female-avatar.svg" : "/images/male-avatar.svg";
    }

    public Task DeletePhotoAsync(string? photoPath)
    {
        if (!string.IsNullOrEmpty(photoPath))
        {
            var fullPath = Path.Combine(env.WebRootPath, photoPath.TrimStart('/'));
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
        return Task.CompletedTask;
    }
}
