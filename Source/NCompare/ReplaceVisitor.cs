using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NCompare
{
  internal sealed class ReplaceVisitor : ExpressionVisitor
  {
    private ReplaceVisitor(IEnumerable<(Expression What, Expression To)> items) => Replaces = items?.ToDictionary(item => item.What, item => item.To) ?? throw new ArgumentNullException(nameof(items));

    private IReadOnlyDictionary<Expression, Expression> Replaces { get; }

    public static Expression ReplaceParameters(LambdaExpression expression, params Expression[] parameters) {
      if(expression is null) {
        throw new ArgumentNullException(nameof(expression));
      } else if(parameters is null) {
        throw new ArgumentNullException(nameof(parameters));
      } else if(parameters.Length == 0) {
        throw new ArgumentException("Should not be empty array.", nameof(parameters));
      } else if(expression.Parameters.Count != parameters.Length) {
        throw new ArgumentException("Number of parameters in expression and in the array not equal.", nameof(expression));
      }//if

      var replaces = expression.Parameters.Select((item, index) => ((Expression)item, parameters[index]));
      var visitor = new ReplaceVisitor(replaces);
      return visitor.Visit(expression.Body);
    }

    public override Expression Visit(Expression node) => node is not null && Replaces.TryGetValue(node, out var replace) ? replace : base.Visit(node);
  }
}
