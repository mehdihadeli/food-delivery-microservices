using System.Reflection;
using System.Web;

namespace BuildingBlocks.Core.Extensions;

public static class ObjectExtensions
{
    public static string GetQueryString(this object obj)
    {
        var properties = from p in obj.GetType().GetProperties()
            where p.GetValue(obj, null) != null
            select p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(obj, null).ToString());

        return string.Join("&", properties.ToArray());
    }

    /// <summary>
    /// Invoke a instance generic method member.
    /// </summary>
    /// <param name="instanceObject"></param>
    /// <param name="methodName"></param>
    /// <param name="genericTypes"></param>
    /// <param name="returnType"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static dynamic? InvokeGenericMethod(
        this object instanceObject,
        string methodName,
        Type[] genericTypes,
        Type? returnType = null,
        params object[] parameters)
    {
        var method = instanceObject.GetType().GetGenericMethod(
            methodName,
            genericTypes,
            parameters.Select(y => y.GetType()).ToArray(),
            returnType);

        if (method == null)
        {
            return null;
        }

        var genericMethod = method.MakeGenericMethod(genericTypes);
        return genericMethod.Invoke(instanceObject, parameters);
    }

    /// Ref: https://stackoverflow.com/a/39679855/581476
    /// <summary>
    /// Invoke a async instance generic method member.
    /// </summary>
    /// <param name="instanceObject"></param>
    /// <param name="methodName"></param>
    /// <param name="genericTypes"></param>
    /// <param name="returnType"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static Task<dynamic>? InvokeGenericMethodAsync(
        this object instanceObject,
        string methodName,
        Type[] genericTypes,
        Type? returnType = null,
        params object[] parameters)
    {
        dynamic? awaitable = InvokeGenericMethod(instanceObject, methodName, genericTypes, returnType, parameters);

        return awaitable;
    }

    /// <summary>
    /// Invoke a instance method member.
    /// </summary>
    /// <param name="instanceObject"></param>
    /// <param name="methodName"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static dynamic InvokeMethod(
        this object instanceObject,
        string methodName,
        params object[] parameters)
    {
        var method = instanceObject
            .GetType()
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.Name == methodName)
            .FirstOrDefault(x =>
                x.GetParameters().Select(p => p.ParameterType).All(parameters.Select(p => p.GetType()).Contains));

        if (method is null)
            return null!;

        return method.Invoke(instanceObject, parameters);
    }

    /// <summary>
    /// Invoke a instance method member.
    /// </summary>
    /// <param name="instanceObject"></param>
    /// <param name="methodName"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static Task<dynamic> InvokeMethodAsync(
        this object instanceObject,
        string methodName,
        params object[] parameters)
    {
        dynamic awaitable = InvokeMethod(instanceObject, methodName, parameters);

        return awaitable;
    }

    /// <summary>
    /// Invoke a instance method member with return type <see cref="Task"/> or without return type.
    /// </summary>
    /// <param name="instanceObject"></param>
    /// <param name="methodName"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static async Task InvokeMethodWithoutResultAsync(
        this object instanceObject,
        string methodName,
        params object[] parameters)
    {
        dynamic awaitable = InvokeMethod(instanceObject, methodName, parameters);

        await awaitable;
    }

    /// <summary>
    /// Check if type is a value-type, primitive type  or string
    /// </summary>
    public static bool IsPrimitiveType(this object obj)
    {
        return obj == null || obj.GetType().IsPrimitiveType();
    }
}
