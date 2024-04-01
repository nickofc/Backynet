using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Backynet.Core.Abstraction;

public static class JobDescriptorFactory
{
    private static readonly ConcurrentDictionary<Expression, object> Cache
        = new ConcurrentDictionary<Expression, object>();

    public static JobDescriptor Create(Expression expression)
    {
        // todo: cache

        if (expression is not LambdaExpression lambdaExpression)
        {
            throw new InvalidOperationException("Expression must be LambdaExpression.");
        }

        if (lambdaExpression.Body is not MethodCallExpression methodCallExpression)
        {
            throw new InvalidOperationException("Expression must be MethodCallExpression.");
        }

        var method = AsMethod(methodCallExpression.Method);
        var arguments = AsArguments(methodCallExpression);

        return new JobDescriptor(method, arguments);
    }

    private static IMethod AsMethod(MethodInfo methodInfo)
    {
        var type = methodInfo.DeclaringType ?? throw new InvalidOperationException("Unable to get declaring type.");

        var typeName = type.AssemblyQualifiedName ?? throw new InvalidOperationException("Unable to get assembly qualified name.");
        var methodName = type.Name;

        return new Method(typeName, methodName);
    }

    private static IReadOnlyCollection<IArgument> AsArguments(MethodCallExpression methodCallExpression)
    {
    }

    private static IReadOnlyCollection<IArgument> ResolveArgs(MethodCallExpression body)
    {
        var values = new List<IArgument>(body.Arguments.Count);

        for (var i = 0; i < body.Arguments.Count; i++)
        {
            var argument = body.Arguments[i];

            var expression = ResolveMemberExpression(argument);

            var value = GetValue(expression);
            var type = value.GetType();

            var attr = new Argument(type.AssemblyQualifiedName, value);

            values.Add(attr);
        }

        return values;
    }

    private static MemberExpression ResolveMemberExpression(Expression expression)
    {
        if (expression is MemberExpression)
        {
            return (MemberExpression)expression;
        }

        if (expression is UnaryExpression)
        {
            // if casting is involved, Expression is not x => x.FieldName but x => Convert(x.Fieldname)
            return (MemberExpression)((UnaryExpression)expression).Operand;
        }

        throw new NotSupportedException(expression.ToString());
    }

    private static object GetValue(MemberExpression exp)
    {
        while (true)
        {
            // expression is ConstantExpression or FieldExpression
            if (exp.Expression is ConstantExpression)
            {
                return (((ConstantExpression)exp.Expression).Value).GetType()
                    .GetField(exp.Member.Name)
                    .GetValue(((ConstantExpression)exp.Expression).Value);
            }

            if (exp.Expression is MemberExpression)
            {
                exp = (MemberExpression)exp.Expression;
                continue;
            }

            throw new NotImplementedException();
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