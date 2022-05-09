using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using BuildingBlocks.Core.Utils;

namespace BuildingBlocks.Core.Extensions;

public static class ReflectionExtensions
{
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
        var method = GetGenericMethod(
            instanceObject.GetType(),
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

    // Ref: https://stackoverflow.com/a/588596/581476
    public static MethodInfo? GetGenericMethod(
        this Type t,
        string name,
        Type[] genericArgTypes,
        Type[] argTypes,
        Type? returnType = null)
    {
        MethodInfo? res = (from m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                where m.Name == name &&
                      m.GetGenericArguments().Length == genericArgTypes.Length &&
                      m.GetParameters().Select(pi => pi.ParameterType)
                          .All(d => argTypes.Any(a => a.IsAssignableTo(d))) &&
                      (m.ReturnType == returnType || returnType == null)
                select m)
            .FirstOrDefault();

        return res;
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

    // https://riptutorial.com/csharp/example/15938/creating-an-instance-of-a-type
    public static bool IsHaveAttribute(this PropertyInfo propertyInfo, Type attribute)
    {
        return propertyInfo.GetCustomAttributes(attribute, true).Any();
    }

    public static T[] GetFlatObjectsListWithInterface<T>(this object obj, IList<T> resultList = null)
    {
        var retVal = new List<T>();

        resultList ??= new List<T>();

        // Ignore cycling references
        if (!resultList.Any(x => ReferenceEquals(x, obj)))
        {
            var objectType = obj.GetType();

            if (objectType.GetInterface(typeof(T).Name) != null)
            {
                retVal.Add((T)obj);
                resultList.Add((T)obj);
            }

            var properties = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var objects = properties.Where(x => x.PropertyType.GetInterface(typeof(T).Name) != null)
                .Select(x => (T)x.GetValue(obj)).ToList();

            // Recursive call for single properties
            retVal.AddRange(objects.Where(x => x != null)
                .SelectMany(x => x.GetFlatObjectsListWithInterface(resultList)));

            // Handle collection and arrays
            var collections = properties.Where(p => p.GetIndexParameters().Length == 0)
                .Select(x => x.GetValue(obj, null))
                .Where(x => x is IEnumerable && !(x is string))
                .Cast<IEnumerable>();

            foreach (var collection in collections)
            {
                foreach (var collectionObject in collection)
                {
                    if (collectionObject is T)
                    {
                        retVal.AddRange(collectionObject.GetFlatObjectsListWithInterface<T>(resultList));
                    }
                }
            }
        }

        return retVal.ToArray();
    }

    public static T CastTo<T>(this object o) => (T)o;

    // https://stackoverflow.com/a/55852845/581476
    public static dynamic CastToReflected(this object o, Type type)
    {
        var methodInfo =
            typeof(ReflectionUtilities).GetMethod(nameof(CastTo), BindingFlags.Static | BindingFlags.Public);
        var genericArguments = new[] { type };
        var genericMethodInfo = methodInfo?.MakeGenericMethod(genericArguments);
        return genericMethodInfo?.Invoke(null, new[] { o });
    }

    private static bool GenericParametersMatch(
        IReadOnlyList<Type> parameters,
        IReadOnlyList<Type> interfaceArguments)
    {
        if (parameters.Count != interfaceArguments.Count)
        {
            return false;
        }

        for (var i = 0; i < parameters.Count; i++)
        {
            if (parameters[i] != interfaceArguments[i])
            {
                return false;
            }
        }

        return true;
    }

    public static string GetModuleName(this object value)
        => value?.GetType().GetModuleName() ?? string.Empty;

    /// <summary>
    /// Iterates recursively over each public property of object to gather member values.
    /// </summary>
    public static IEnumerable<KeyValuePair<string, object>> TraverseObjectGraph(this object original)
    {
        foreach (var result in original.TraverseObjectGraphRecursively(new List<object>(), original.GetType().Name))
        {
            yield return result;
        }
    }

    private static IEnumerable<KeyValuePair<string, object>> TraverseObjectGraphRecursively(
        this object obj,
        List<object> visited,
        string memberPath)
    {
        yield return new KeyValuePair<string, object>(memberPath, obj);
        if (obj != null)
        {
            var typeOfOriginal = obj.GetType();
            if (!IsPrimitive(typeOfOriginal) && !visited.Any(x => ReferenceEquals(obj, x)))
            {
                visited.Add(obj);
                if (obj is IEnumerable objEnum)
                {
                    var originalEnumerator = objEnum.GetEnumerator();
                    var iIdx = 0;
                    while (originalEnumerator.MoveNext())
                    {
                        foreach (var result in originalEnumerator.Current.TraverseObjectGraphRecursively(
                                     visited,
                                     $@"{memberPath}[{iIdx++}]"))
                        {
                            yield return result;
                        }
                    }
                }
                else
                {
                    foreach (var propInfo in typeOfOriginal.GetProperties(BindingFlags.Instance |
                                                                          BindingFlags.Public))
                    {
                        foreach (var result in propInfo.GetValue(obj)
                                     .TraverseObjectGraphRecursively(visited, $@"{memberPath}.{propInfo.Name}"))
                        {
                            yield return result;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Check if type is a value-type, primitive type  or string
    /// </summary>
    public static bool IsPrimitive(this object obj)
    {
        return obj == null || obj.GetType().IsPrimitive();
    }

    /// <summary>
    /// Handles correct upcast. If no upcast was needed, then this could be exchanged to an <c>Expression.Call</c>
    /// and an <c>Expression.Lambda</c>.
    /// </summary>
    public static TResult CompileMethodInvocation<TResult>(this MethodInfo methodInfo)
    {
        var genericArguments = typeof(TResult).GetTypeInfo().GetGenericArguments();
        var methodArgumentList = methodInfo.GetParameters().Select(p => p.ParameterType).ToList();
        var funcArgumentList = genericArguments.Skip(1).Take(methodArgumentList.Count).ToList();

        if (funcArgumentList.Count != methodArgumentList.Count)
        {
            throw new ArgumentException("Incorrect number of arguments");
        }

        var instanceArgument = Expression.Parameter(genericArguments[0]);

        var argumentPairs = funcArgumentList.Zip(methodArgumentList, (s, d) => new { Source = s, Destination = d })
            .ToList();
        if (argumentPairs.All(a => a.Source == a.Destination))
        {
            // No need to do anything fancy, the types are the same
            var parameters = funcArgumentList.Select(Expression.Parameter).ToList();
            return Expression.Lambda<TResult>(Expression.Call(instanceArgument, methodInfo, parameters),
                new[] { instanceArgument }.Concat(parameters)).Compile();
        }

        var lambdaArgument = new List<ParameterExpression> { instanceArgument, };

        var type = methodInfo.DeclaringType;
        var instanceVariable = Expression.Variable(type);
        var blockVariables = new List<ParameterExpression> { instanceVariable, };
        var blockExpressions = new List<Expression>
        {
            Expression.Assign(instanceVariable, Expression.ConvertChecked(instanceArgument, type))
        };
        var callArguments = new List<ParameterExpression>();

        foreach (var a in argumentPairs)
        {
            if (a.Source == a.Destination)
            {
                var sourceParameter = Expression.Parameter(a.Source);
                lambdaArgument.Add(sourceParameter);
                callArguments.Add(sourceParameter);
            }
            else
            {
                var sourceParameter = Expression.Parameter(a.Source);
                var destinationVariable = Expression.Variable(a.Destination);
                var assignToDestination = Expression.Assign(destinationVariable,
                    Expression.Convert(sourceParameter, a.Destination));

                lambdaArgument.Add(sourceParameter);
                callArguments.Add(destinationVariable);
                blockVariables.Add(destinationVariable);
                blockExpressions.Add(assignToDestination);
            }
        }

        var callExpression = Expression.Call(instanceVariable, methodInfo, callArguments);
        blockExpressions.Add(callExpression);

        var block = Expression.Block(blockVariables, blockExpressions);

        var lambdaExpression = Expression.Lambda<TResult>(block, lambdaArgument);

        return lambdaExpression.Compile();
    }
}
