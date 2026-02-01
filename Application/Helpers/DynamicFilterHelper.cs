using System.Linq.Expressions;
using System.Reflection;
using backend.Application.DTOs.Common;
using backend.Domain.Common;

namespace backend.Application.Helpers;

/// <summary>
/// Helper class để áp dụng dynamic filters cho entities
/// </summary>
public static class DynamicFilterHelper
{
    /// <summary>
    /// Áp dụng dynamic filters cho entity dựa trên properties của filter request
    /// </summary>
    public static IQueryable<T> ApplyDynamicFilters<T>(IQueryable<T> query, FilterRequest filter) where T : BaseEntity
    {
        var filterType = filter.GetType();
        var entityType = typeof(T);

        // Lấy tất cả properties của filter request (trừ các properties từ FilterRequest base class)
        var filterProperties = filterType.GetProperties()
            .Where(p => p.DeclaringType != typeof(FilterRequest) && 
                       p.DeclaringType != typeof(PagedRequest) &&
                       p.GetValue(filter) != null);

        foreach (var filterProp in filterProperties)
        {
            var filterValue = filterProp.GetValue(filter);
            if (filterValue == null) continue;

            var propName = filterProp.Name;
            
            // Kiểm tra nếu là foreign key filter (ví dụ: CategoryCode)
            if (propName.EndsWith("Code") && filterValue is string codeValue && !string.IsNullOrEmpty(codeValue))
            {
                // Tìm navigation property (ví dụ: CategoryCode -> Category.Code)
                var navPropertyName = propName.Replace("Code", "");
                var navProperty = entityType.GetProperty(navPropertyName);
                
                if (navProperty != null)
                {
                    // Tạo expression: e.Category.Code.Contains(codeValue)
                    var navParam = Expression.Parameter(entityType, "e");
                    var navPropertyExpr = Expression.Property(navParam, navProperty);
                    var codeProperty = navProperty.PropertyType.GetProperty("Code");
                    
                    if (codeProperty != null)
                    {
                        var codePropertyExpr = Expression.Property(navPropertyExpr, codeProperty);
                        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                        var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
                        var codeToLower = Expression.Call(codePropertyExpr, toLowerMethod!);
                        var navConstant = Expression.Constant(codeValue.ToLower());
                        var navConstantToLower = Expression.Call(navConstant, toLowerMethod!);
                        var navCondition = Expression.Call(codeToLower, containsMethod!, navConstantToLower);
                        var navLambda = Expression.Lambda<Func<T, bool>>(navCondition, navParam);
                        query = query.Where(navLambda);
                        continue;
                    }
                }
            }

            // Tìm property trực tiếp trong entity
            var entityProp = entityType.GetProperty(propName);
            if (entityProp == null) continue;

            var param = Expression.Parameter(entityType, "e");
            var property = Expression.Property(param, entityProp);
            var constant = Expression.Constant(filterValue);
            
            Expression? condition = null;

            // Xử lý theo kiểu dữ liệu
            if (filterValue is string strValue && !string.IsNullOrEmpty(strValue))
            {
                // String: Contains (case-insensitive)
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
                var propertyToLower = Expression.Call(property, toLowerMethod!);
                var constantToLower = Expression.Call(constant, toLowerMethod!);
                condition = Expression.Call(propertyToLower, containsMethod!, constantToLower);
            }
            else if (filterValue is Guid guidValue)
            {
                // Guid: Equals
                condition = Expression.Equal(property, constant);
            }
            else if (filterValue is bool boolValue)
            {
                // Bool: Equals
                condition = Expression.Equal(property, constant);
            }
            else if (filterValue is DateTime dateValue)
            {
                // DateTime: GreaterThanOrEqual hoặc LessThanOrEqual
                if (propName.Contains("From"))
                {
                    condition = Expression.GreaterThanOrEqual(property, constant);
                }
                else if (propName.Contains("To"))
                {
                    condition = Expression.LessThanOrEqual(property, constant);
                }
            }
            else if (filterValue is int || filterValue is long || filterValue is decimal)
            {
                // Numeric: Equals
                condition = Expression.Equal(property, constant);
            }

            if (condition != null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(condition, param);
                query = query.Where(lambda);
            }
        }

        // Apply base filters
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            query = ApplySearchTerm(query, filter.SearchTerm);
        }

        if (filter.IsDeleted.HasValue)
        {
            var param = Expression.Parameter(entityType, "e");
            var property = Expression.Property(param, nameof(BaseEntity.IsDeleted));
            var constant = Expression.Constant(filter.IsDeleted.Value);
            var condition = Expression.Equal(property, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(condition, param);
            query = query.Where(lambda);
        }

        if (filter.CreatedFrom.HasValue)
        {
            var param = Expression.Parameter(entityType, "e");
            var property = Expression.Property(param, nameof(BaseEntity.CreatedAt));
            var constant = Expression.Constant(filter.CreatedFrom.Value);
            var condition = Expression.GreaterThanOrEqual(property, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(condition, param);
            query = query.Where(lambda);
        }

        if (filter.CreatedTo.HasValue)
        {
            var param = Expression.Parameter(entityType, "e");
            var property = Expression.Property(param, nameof(BaseEntity.CreatedAt));
            var constant = Expression.Constant(filter.CreatedTo.Value);
            var condition = Expression.LessThanOrEqual(property, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(condition, param);
            query = query.Where(lambda);
        }

        return query;
    }

    /// <summary>
    /// Áp dụng search term cho tất cả string properties của entity
    /// </summary>
    private static IQueryable<T> ApplySearchTerm<T>(IQueryable<T> query, string searchTerm) where T : BaseEntity
    {
        var entityType = typeof(T);
        var stringProperties = entityType.GetProperties()
            .Where(p => p.PropertyType == typeof(string) && p.CanRead);

        if (!stringProperties.Any()) return query;

        var parameter = Expression.Parameter(entityType, "e");
        var searchTermLower = searchTerm.ToLower();
        var constant = Expression.Constant(searchTermLower);
        var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

        Expression? combinedCondition = null;

        foreach (var prop in stringProperties)
        {
            var property = Expression.Property(parameter, prop);
            var propertyToLower = Expression.Call(property, toLowerMethod!);
            var condition = Expression.Call(propertyToLower, containsMethod!, constant);

            if (combinedCondition == null)
            {
                combinedCondition = condition;
            }
            else
            {
                combinedCondition = Expression.OrElse(combinedCondition, condition);
            }
        }

        if (combinedCondition != null)
        {
            var lambda = Expression.Lambda<Func<T, bool>>(combinedCondition, parameter);
            query = query.Where(lambda);
        }

        return query;
    }
}
