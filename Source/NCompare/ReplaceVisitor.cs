using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace NCompare
{
  internal sealed class ReplaceVisitor : ExpressionVisitor
  {
    private ReplaceVisitor(Expression what, Expression to) {
      What = what ?? throw new ArgumentNullException(nameof(what));
      To = to ?? throw new ArgumentNullException(nameof(to));
    }

    private ReplaceVisitor(Expression what, Expression to, Expression secondWhat, Expression secondTo) : this(what, to) {
      SecondWhat = secondWhat ?? throw new ArgumentNullException(nameof(secondWhat));
      SecondTo = secondTo ?? throw new ArgumentNullException(nameof(secondTo));
    }

    public Expression What { get; }
    public Expression To { get; }

    public Expression? SecondWhat { get; }
    public Expression? SecondTo { get; }

    public static Expression ReplaceParameters(LambdaExpression expression, Expression first) => ReplaceParametersCore(expression, first);

    public static Expression ReplaceParameters(LambdaExpression expression, Expression first, Expression second) => ReplaceParametersCore(expression, first, second);

    private static Expression ReplaceParametersCore(LambdaExpression expression, Expression first, Expression? second = null) {
      if(expression is null) {
        throw new ArgumentNullException(nameof(expression));
      } else if(expression.Parameters.Count != (second is null ? 1 : 2)) {
        throw new ArgumentException($"{nameof(expression)}.Parameters.Count != {(second is null ? 1 : 2)}", nameof(expression));
      }//if

      var replace = second is null
        ? new ReplaceVisitor(expression.Parameters[0], first)
        : new ReplaceVisitor(expression.Parameters[0], first, expression.Parameters[1], second);
      return replace.Visit(expression.Body);
    }

    public override Expression Visit(Expression node) {
      if(node == What) {
        return To;
      } else if(node is object && node == SecondWhat) {
        Debug.Assert(SecondTo is object, $"{nameof(SecondTo)} is object");
        return SecondTo!;
      } else {
        return base.Visit(node);
      }//if
    }
  }
}
