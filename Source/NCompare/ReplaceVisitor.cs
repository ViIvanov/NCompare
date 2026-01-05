using System.Linq.Expressions;

namespace NCompare;

internal sealed class ReplaceVisitor : ExpressionVisitor
{
  private ReplaceVisitor(IEnumerable<(Expression What, Expression To)> items) => Replaces = items?.ToDictionary(static item => item.What, static item => item.To) ?? throw new ArgumentNullException(nameof(items));

  private Dictionary<Expression, Expression> Replaces { get; }

  public static Expression ReplaceParameters(LambdaExpression expression, params IReadOnlyList<Expression> parameters) {
    ArgumentNullException.ThrowIfNull(expression);
    ArgumentNullException.ThrowIfNull(parameters);

    if(parameters is []) {
      throw new ArgumentException("Should not be empty array.", nameof(parameters));
    } else if(expression.Parameters.Count != parameters.Count) {
      throw new ArgumentException("Number of parameters in expression and in the array not equal.", nameof(expression));
    }//if

    var replaces = expression.Parameters.Select((item, index) => ((Expression)item, parameters[index]));
    var visitor = new ReplaceVisitor(replaces);
    return visitor.Visit(expression.Body);
  }

  public override Expression Visit(Expression node) => node is not null && Replaces.TryGetValue(node, out var replace) ? replace : base.Visit(node);
}
