using System.Linq.Expressions;
using System.Reflection;

namespace Backynet.Abstraction;

public static class JobDescriptorFactory
{
    public static IJobDescriptor Create(Expression<Func<Task>> expression)
    {
        return Create((Expression)expression);
    }

    public static IJobDescriptor Create(Expression<Action> expression)
    {
        return Create((Expression)expression);
    }

    public static IJobDescriptor Create(Expression expression)
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

        arguments = DeleteCancellationToken(arguments);

        return new JobDescriptor(method, arguments);
    }

    private static IArgument[] DeleteCancellationToken(IArgument[] arguments)
    {
        var output = new List<IArgument>(arguments.Length);

        for (var i = 0; i < arguments.Length; i++)
        {
            if (arguments[i].Value is not CancellationToken)
            {
                output.Add(arguments[i]);
            }
        }

        return output.ToArray();
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
        if (expression is MemberExpression memberExpression)
        {
            return GetValueFromExpression(memberExpression);
        }

        if (expression is UnaryExpression unaryExpression)
        {
            var unaryExpressionOperand = (MemberExpression)unaryExpression.Operand;
            return GetValueFromExpression(unaryExpressionOperand);
        }

        if (expression is ConstantExpression constantExpression)
        {
            return constantExpression.Value;
        }

        if (expression is MemberInitExpression memberInitExpression)
        {
            var d = memberInitExpression.Bindings[0];
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