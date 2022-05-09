using System.Linq.Expressions;
using System.Reflection;

namespace BuildingBlocks.Web.Extensions;

public static class ModelExtensions
{
    public static T Bind<T>(this T model, Expression<Func<T, object>> expression, object value)
        => model.Bind<T, object>(expression, value);

    private static TModel Bind<TModel, TProperty>(
        this TModel model,
        Expression<Func<TModel, TProperty>> expression,
        object value)
    {
        var memberExpression = expression.Body as MemberExpression ??
                               ((UnaryExpression) expression.Body).Operand as MemberExpression;
        if (memberExpression is null)
        {
            return model;
        }

        var propertyName = memberExpression.Member.Name.ToLowerInvariant();
        var modelType = model.GetType();
        var field = modelType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .SingleOrDefault(x => x.Name.ToLowerInvariant().StartsWith($"<{propertyName}>", StringComparison.Ordinal));
        if (field is null)
        {
            return model;
        }

        field.SetValue(model, value);

        return model;
    }
}
