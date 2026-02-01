using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using backend.Application.Interfaces;

namespace backend.Infrastructure.Filters;

/// <summary>
/// Filter để dịch ModelState validation error messages theo ngôn ngữ hiện tại
/// </summary>
public class LocalizedModelStateFilter : IActionFilter
{
    private readonly ILocalizationService _localizationService;

    public LocalizedModelStateFilter(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            // Dịch tất cả validation error messages
            var keysToUpdate = new Dictionary<string, List<string>>();
            
            foreach (var key in context.ModelState.Keys)
            {
                var errors = context.ModelState[key]?.Errors;
                if (errors != null && errors.Any())
                {
                    var translatedErrors = new List<string>();
                    
                    foreach (var error in errors)
                    {
                        var translatedMessage = TranslateErrorMessage(error.ErrorMessage, key, _localizationService);
                        translatedErrors.Add(translatedMessage);
                    }
                    
                    keysToUpdate[key] = translatedErrors;
                }
            }

            // Cập nhật ModelState với messages đã dịch
            foreach (var kvp in keysToUpdate)
            {
                context.ModelState.Remove(kvp.Key);
                foreach (var errorMessage in kvp.Value)
                {
                    context.ModelState.AddModelError(kvp.Key, errorMessage);
                }
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Không cần xử lý gì sau khi action executed
    }

    /// <summary>
    /// Dịch error message dựa trên nội dung và field name
    /// </summary>
    private string TranslateErrorMessage(string errorMessage, string fieldName, ILocalizationService localizationService)
    {
        if (string.IsNullOrEmpty(errorMessage))
            return errorMessage;

        var messageLower = errorMessage.ToLower();
        var fieldLower = fieldName.ToLower();

        // Map các validation messages phổ biến
        if (messageLower.Contains("required") || messageLower.Contains("bắt buộc"))
        {
            // Kiểm tra field cụ thể
            if (fieldLower.Contains("username") || (fieldLower.Contains("email") && fieldLower.Contains("or")))
                return localizationService.GetString("Validation.UsernameOrEmailRequired");
            if (fieldLower.Contains("password") && !fieldLower.Contains("confirm"))
                return localizationService.GetString("Validation.PasswordRequired");
            if (fieldLower.Contains("email"))
                return localizationService.GetString("Validation.EmailRequired");
            if (fieldLower.Contains("confirmpassword") || (fieldLower.Contains("confirm") && fieldLower.Contains("password")))
                return localizationService.GetString("Validation.ConfirmPasswordRequired");
            if (fieldLower.Contains("productcode"))
                return localizationService.GetString("Validation.ProductCodeRequired");
            if (fieldLower.Contains("productname"))
                return localizationService.GetString("Validation.ProductNameRequired");
            if ((fieldLower.Contains("permissionname") || fieldLower.Contains("name")) && !fieldLower.Contains("product"))
                return localizationService.GetString("Validation.PermissionNameRequired");
            if (fieldLower.Contains("resource"))
                return localizationService.GetString("Validation.ResourceRequired");
            if (fieldLower.Contains("action"))
                return localizationService.GetString("Validation.ActionRequired");
            if (fieldLower.Contains("rolename"))
                return localizationService.GetString("Validation.RoleNameRequired");
            if (fieldLower.Contains("permissionids"))
                return localizationService.GetString("Validation.PermissionIdsRequired");
            if (fieldLower.Contains("language"))
                return localizationService.GetString("Validation.LanguageCodeRequired");
            if (fieldLower.Contains("translationkey") || (fieldLower.Contains("key") && fieldLower.Contains("translation")))
                return localizationService.GetString("Validation.TranslationKeyRequired");
            if (fieldLower.Contains("translationvalue") || (fieldLower.Contains("value") && fieldLower.Contains("translation")))
                return localizationService.GetString("Validation.TranslationValueRequired");
            
            return localizationService.GetString("Validation.Required");
        }
        
        if (messageLower.Contains("email") && (messageLower.Contains("invalid") || messageLower.Contains("format")))
            return localizationService.GetString("Validation.InvalidEmail");
        
        if (messageLower.Contains("minlength") || messageLower.Contains("tối thiểu") || messageLower.Contains("at least"))
        {
            if (fieldLower.Contains("password"))
                return localizationService.GetString("Validation.PasswordMinLength");
            return localizationService.GetString("Validation.MinLength");
        }
        
        if (messageLower.Contains("maxlength") || messageLower.Contains("tối đa") || messageLower.Contains("exceed") || messageLower.Contains("not exceed"))
        {
            if (fieldLower.Contains("productcode"))
                return localizationService.GetString("Validation.ProductCodeMaxLength");
            if (fieldLower.Contains("productname"))
                return localizationService.GetString("Validation.ProductNameMaxLength");
            if (fieldLower.Contains("image"))
                return localizationService.GetString("Validation.ImageMaxLength");
            if ((fieldLower.Contains("permissionname") || fieldLower.Contains("name")) && !fieldLower.Contains("product"))
                return localizationService.GetString("Validation.PermissionNameMaxLength");
            if (fieldLower.Contains("description"))
                return localizationService.GetString("Validation.DescriptionMaxLength");
            if (fieldLower.Contains("resource"))
                return localizationService.GetString("Validation.ResourceMaxLength");
            if (fieldLower.Contains("action"))
                return localizationService.GetString("Validation.ActionMaxLength");
            if (fieldLower.Contains("translationkey") || (fieldLower.Contains("key") && fieldLower.Contains("translation")))
                return localizationService.GetString("Validation.TranslationKeyMaxLength");
            return localizationService.GetString("Validation.MaxLength");
        }
        
        if (messageLower.Contains("compare") || messageLower.Contains("match") || messageLower.Contains("không khớp") || messageLower.Contains("do not match"))
            return localizationService.GetString("Validation.PasswordsDoNotMatch");
        
        if (messageLower.Contains("between") || messageLower.Contains("phạm vi") || messageLower.Contains("must be between"))
        {
            if (fieldLower.Contains("language"))
                return localizationService.GetString("Validation.LanguageCodeLength");
            return localizationService.GetString("Validation.Range");
        }

        // Nếu không tìm thấy translation, trả về message gốc
        return errorMessage;
    }
}
