using System.Linq.Expressions;
using System.Reflection;

namespace Backynet.Core.Abstraction;

public static class JobDescriptorFactory
{
    public static JobDescriptor Create(Expression<Func<Task>> expression)
    {
        return Create((Expression)expression);
    }

    public static JobDescriptor Create(Expression<Action> expression)
    {
        return Create((Expression)expression);
    }

    public static JobDescriptor Create(Expression expression)
    {
        if (expression is not LambdaExpression lambdaExpression)
        {
            throw new InvalidOperationException("Expression must be LambdaExpression.");
        }

        if (lambdaExpression.Body is not MethodCallExpression methodCallExpression)
        {
            throw new InvalidOperationException("Expression must be MethodCallExpression.");
        }

        var method = GetMethod(methodCallExpression.Method);
        var arguments = GetArguments(methodCallExpression);

        return new JobDescriptor(method, arguments);
    }

    private static IMethod GetMethod(MemberInfo methodInfo)
    {
        var type = methodInfo.DeclaringType ?? throw new InvalidOperationException("Unable to get declaring type.");

        var typeName = type.AssemblyQualifiedName ?? throw new InvalidOperationException("Unable to get assembly qualified name.");
        var methodName = methodInfo.Name;

        return new Method(typeName, methodName);
    }

    private static IArgument[] GetArguments(MethodCallExpression methodCallExpression)
    {
        var values = new IArgument[methodCallExpression.Arguments.Count];

        for (var i = 0; i < methodCallExpression.Arguments.Count; i++)
        {
            var value = GetValueFromExp(methodCallExpression.Arguments[i]);
            var type = value?.GetType();

            var typeName = type is null ? null : type.AssemblyQualifiedName ?? throw new InvalidOperationException("Unable to get assembly qualified name.");
            var argument = new Argument(typeName, value);

            values[i] = argument;
        }

        return values;
    }

    private static object? GetValueFromExp(Expression expression)
    {
        if (expression is MemberExpression mem1)
        {
            return GetValueFromExpression(mem1);
        }

        if (expression is UnaryExpression e)
        {
            MemberExpression s = (MemberExpression)e.Operand;
            return GetValueFromExpression(s);
        }

        if (expression is ConstantExpression c)
        {
            return c.Value;
        }

        throw new NotSupportedException(expression.ToString());
    }

    private static object GetValueFromExpression(MemberExpression expression)
    {
        while (true)
        {
            // expression is ConstantExpression or FieldExpression
            if (expression.Expression is ConstantExpression constantExpression)
            {
                return constantExpression.Value.GetType()
                    .GetField(expression.Member.Name)
                    .GetValue(constantExpression.Value);
            }

            if (expression.Expression is MemberExpression memberExpression)
            {
                expression = memberExpression;
                continue;
            }

            throw new NotSupportedException(expression.ToString());
        }
    }

    internal static class Empty
    {
        public static Task Method()
        {
            return Task.CompletedTask;
        }
    }
}