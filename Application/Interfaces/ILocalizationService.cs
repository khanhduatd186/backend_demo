namespace backend.Application.Interfaces;

/// <summary>
/// Service interface để quản lý localization/ngôn ngữ
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Lấy chuỗi đã được dịch theo key
    /// </summary>
    string GetString(string key, params object[] args);
    
    /// <summary>
    /// Lấy ngôn ngữ hiện tại
    /// </summary>
    string GetCurrentLanguage();
    
    /// <summary>
    /// Set ngôn ngữ cho request hiện tại
    /// </summary>
    void SetLanguage(string languageCode);
    
    /// <summary>
    /// Lấy danh sách ngôn ngữ được hỗ trợ
    /// </summary>
    IEnumerable<string> GetSupportedLanguages();
    
    /// <summary>
    /// Lấy danh sách ngôn ngữ được hỗ trợ (async)
    /// </summary>
    Task<IEnumerable<string>> GetSupportedLanguagesAsync();
}
