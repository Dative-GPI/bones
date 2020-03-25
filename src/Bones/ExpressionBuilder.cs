using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Bones
{
    public static class ExpressionBuilder
    {

        public static Expression<Func<TSource, bool>> In<TSource, TElement>(IEnumerable<TElement> items, Expression<Func<TSource, TElement>> comparatorAccessor)
        {
            Expression exp = null;
            ParameterExpression pe = Expression.Parameter(typeof(TSource), "e");

            Expression replaced = new SwapVisitor(comparatorAccessor.Parameters[0], pe).Visit(comparatorAccessor.Body);

            foreach (var item in items)
            {
                ConstantExpression ce = Expression.Constant(item);
                BinaryExpression be = Expression.Equal(replaced, ce);

                exp = exp == null ? be : Expression.OrElse(exp, be);
            }

            var result = Expression.Lambda<Func<TSource, bool>>(exp, new[] { pe });
            return result;
        }

        public static Expression<Func<TSource, bool>> Or<TSource, TElement>(IEnumerable<TElement> items, Expression<Func<TSource, TElement, bool>> comparison)
        {
            Expression exp = null;
            ParameterExpression pe = Expression.Parameter(typeof(TSource), "e");

            Expression replaced = new SwapVisitor(comparison.Parameters[0], pe).Visit(comparison.Body);

            foreach (var item in items)
            {
                ConstantExpression ce = Expression.Constant(item);
                Expression inserted = new SwapVisitor(comparison.Parameters[1], ce).Visit(replaced);

                exp = exp == null ? inserted : Expression.OrElse(exp, inserted);
            }

            var result = Expression.Lambda<Func<TSource, bool>>(exp, new[] { pe });
            return result;
        }

        public static Expression<Func<TSource, bool>> AndAlso<TSource>(this Expression<Func<TSource, bool>> expr1, Expression<Func<TSource, bool>> expr2)
        {
            var parameter = Expression.Parameter(typeof(TSource), "d");

            var leftVisitor = new SwapVisitor(expr1.Parameters[0], parameter);
            var left = leftVisitor.Visit(expr1.Body);

            var rightVisitor = new SwapVisitor(expr2.Parameters[0], parameter);
            var right = rightVisitor.Visit(expr2.Body);

            return Expression.Lambda<Func<TSource, bool>>(
                Expression.AndAlso(left, right), parameter);
        }

    }

    internal class SwapVisitor : ExpressionVisitor
    {
        private readonly Expression _source, _replacement;

        public SwapVisitor(Expression source, Expression replacement)
        {
            _source = source;
            _replacement = replacement;
        }

        public override Expression Visit(Expression node)
        {
            return node == _source ? _replacement : base.Visit(node);
        }
    }

}
