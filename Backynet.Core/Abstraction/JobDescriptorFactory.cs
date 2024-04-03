using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Backynet.Core.Abstraction;

public static class JobDescriptorFactory
{
    private static readonly ConcurrentDictionary<Expression, object> Cache
        = new ConcurrentDictionary<Expression, object>();

    public static JobDescriptor Create(Expression<Action> expression)
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
        var methodName = type.Name;

        return new Method(typeName, methodName);
    }

    private static IReadOnlyCollection<IArgument> GetArguments(MethodCallExpression methodCallExpression)
    {
        var values = new List<IArgument>(methodCallExpression.Arguments.Count);

        for (var i = 0; i < methodCallExpression.Arguments.Count; i++)
        {
            var expression = ResolveMemberExpression(methodCallExpression.Arguments[i]);

            var value = GetValueFromExpression(expression);
            var type = value.GetType();

            var typeName = type.AssemblyQualifiedName ?? throw new InvalidOperationException("Unable to get assembly qualified name.");
            var argument = new Argument(typeName, value);

            values.Add(argument);
        }

        return values;
    }

    private static MemberExpression ResolveMemberExpression(Expression expression)
    {
        return expression switch
        {
            MemberExpression memberExpression => memberExpression,
            UnaryExpression unaryExpression => (MemberExpression)unaryExpression.Operand,
            _ => throw new NotSupportedException(expression.ToString())
        };
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