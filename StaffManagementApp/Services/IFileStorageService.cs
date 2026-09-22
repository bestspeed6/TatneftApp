namespace StaffManagementApp.Services;

public interface IFileStorageService
{
    /// <summary>
    /// Сохраняет фотографию. Выбрасывает InvalidOperationException при нарушении ограничений.
    /// </summary>
    Task<string> SavePhotoAsync(Stream stream, string fileName, string contentType);
    string GetPhotoUrl(string? photoPath, string gender);
    Task DeletePhotoAsync(string? photoPath);
}
