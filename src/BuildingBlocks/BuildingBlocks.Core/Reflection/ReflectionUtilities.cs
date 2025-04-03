using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace BuildingBlocks.Core.Reflection;

public static class ReflectionUtilities
{
    public static dynamic CreateGenericType(Type genericType, Type[] typeArguments, params object?[] constructorArgs)
    {
        var type = genericType.MakeGenericType(typeArguments);
        return Activator.CreateInstance(type, constructorArgs);
    }

    public static dynamic CreateGenericType<TGenericType>(Type[] typeArguments, params object?[] constructorArgs)
    {
        return CreateGenericType(typeof(TGenericType), typeArguments, constructorArgs);
    }

    public static IEnumerable<Type> GetAllTypesImplementingInterface<TInterface>(params Assembly[] assemblies)
    {
        var inputAssemblies = assemblies.Any() ? assemblies : AppDomain.CurrentDomain.GetAssemblies();
        return inputAssemblies.SelectMany(GetAllTypesImplementingInterface<TInterface>);
    }

    private static IEnumerable<Type> GetAllTypesImplementingInterface<TInterface>(Assembly? assembly = null)
    {
        var inputAssembly = assembly ?? Assembly.GetExecutingAssembly();
        return inputAssembly
            .GetTypes()
            .Where(type =>
                typeof(TInterface).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract && type.IsClass
            );
    }

    public static IEnumerable<string?> GetPropertyNames<T>(params Expression<Func<T, object>>[] propertyExpressions)
    {
        var retVal = new List<string?>();
        foreach (var propertyExpression in propertyExpressions)
        {
            retVal.Add(GetPropertyName(propertyExpression));
        }

        return retVal;
    }

    public static string? GetPropertyName<T>(Expression<Func<T, object>> propertyExpression)
    {
        string? retVal = null;
        if (propertyExpression != null)
        {
            var lambda = (LambdaExpression)propertyExpression;
            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression unaryExpression)
            {
                memberExpression = (MemberExpression)unaryExpression.Operand;
            }
            else
            {
                memberExpression = (MemberExpression)lambda.Body;
            }

            retVal = memberExpression.Member.Name;
        }

        return retVal;
    }

    public static Type? GetTypeFromAnyReferencingAssembly(string typeName)
    {
        var referencedAssemblies = Assembly.GetEntryAssembly()?.GetReferencedAssemblies().Select(a => a.FullName);

        if (referencedAssemblies == null)
            return null;

        return AppDomain
            .CurrentDomain.GetAssemblies()
            .Where(a => referencedAssemblies.Contains(a.FullName))
            .SelectMany(a => a.GetTypes().Where(x => x.FullName == typeName || x.Name == typeName))
            .FirstOrDefault();
    }

    public static Type? GetFirstMatchingTypeFromCurrentDomainAssemblies(string typeName)
    {
        return AppDomain
            .CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes().Where(x => x.FullName == typeName || x.Name == typeName))
            .FirstOrDefault();
    }

    /// <summary>
    /// Handles correct upcast. If no upcast was needed, then this could be exchanged to an <c>Expression.Call</c>
    /// and an <c>Expression.Lambda</c>.
    /// </summary>
    public static TResult CompileMethodInvocation<TResult>(MethodInfo methodInfo)
    {
        var genericArguments = typeof(TResult).GetTypeInfo().GetGenericArguments();
        var methodArgumentList = methodInfo.GetParameters().Select(p => p.ParameterType).ToList();
        var funcArgumentList = genericArguments.Skip(1).Take(methodArgumentList.Count).ToList();

        if (funcArgumentList.Count != methodArgumentList.Count)
        {
            throw new ArgumentException("Incorrect number of arguments");
        }

        var instanceArgument = Expression.Parameter(genericArguments[0]);

        var argumentPairs = funcArgumentList
            .Zip(methodArgumentList, (s, d) => new { Source = s, Destination = d })
            .ToList();
        if (argumentPairs.All(a => a.Source == a.Destination))
        {
            // No need to do anything fancy, the types are the same
            var parameters = funcArgumentList.Select(Expression.Parameter).ToList();
            return Expression
                .Lambda<TResult>(
                    Expression.Call(instanceArgument, methodInfo, parameters),
                    new[] { instanceArgument }.Concat(parameters)
                )
                .Compile();
        }

        var lambdaArgument = new List<ParameterExpression> { instanceArgument };

        var type = methodInfo.DeclaringType;
        var instanceVariable = Expression.Variable(type);
        var blockVariables = new List<ParameterExpression> { instanceVariable };
        var blockExpressions = new List<Expression>
        {
            Expression.Assign(instanceVariable, Expression.ConvertChecked(instanceArgument, type)),
        };
        var callArguments = new List<ParameterExpression>();

        foreach (var a in CollectionsMarshal.AsSpan(argumentPairs))
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
                var assignToDestination = Expression.Assign(
                    destinationVariable,
                    Expression.Convert(sourceParameter, a.Destination)
                );

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

    /// <summary>
    /// Getting all assemblies containing application part
    /// Ref: https://learn.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-7.0 , https://stackoverflow.com/questions/71571852/how-can-i-get-applicationpartattribute-within-net5-class-library.
    /// </summary>
    /// <param name="rootAssembly"></param>
    /// <returns></returns>
    public static IReadOnlyList<Assembly> GetApplicationPartAssemblies(Assembly rootAssembly)
    {
        var rootNamespace = rootAssembly.GetName().Name!.Split('.').First();
        var list = rootAssembly!
            .GetCustomAttributes<ApplicationPartAttribute>()
            .Where(x => x.AssemblyName.StartsWith(rootNamespace, StringComparison.InvariantCulture))
            .Select(name => Assembly.Load(name.AssemblyName))
            .Distinct();

        return list.ToList().AsReadOnly();
    }

    /// <summary>
    /// Get and load, all assemblies from bin folder.
    /// Ref: https://dotnetcoretutorials.com/2020/07/03/getting-assemblies-is-harder-than-you-think-in-c/.
    /// </summary>
    /// <returns></returns>
    public static IReadOnlyList<Assembly> GetBinDirectoryAssemblies()
    {
        var assemblies = Directory
            .GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll")
            .Select(x => Assembly.Load(AssemblyName.GetAssemblyName(x)))
            .Distinct();

        return assemblies.ToList().AsReadOnly();
    }
}
