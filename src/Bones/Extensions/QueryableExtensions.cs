
using System.Linq.Expressions;
using System.Reflection;


namespace System.Linq
{
    public static class IQueryableExtensions
    {
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName)
        {
            var property = GetProperty<T>(propertyName);

            if (property == null)
                throw new ArgumentException($"Property {propertyName} does not exist on type {typeof(T)}");

            return source.OrderBy(ToLambda<T>(property.Name));
        }

        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string propertyName)
        {
            var property = GetProperty<T>(propertyName);

            if (property == null)
                throw new ArgumentException($"Property {propertyName} does not exist on type {typeof(T)}");

            return source.OrderByDescending(ToLambda<T>(property.Name));
        }

        private static Expression<Func<T, object>> ToLambda<T>(string propertyName)
        {
            var parameter = Expression.Parameter(typeof(T));
            var property = Expression.Property(parameter, propertyName);

            return Expression.Lambda<Func<T, object>>(property, parameter);
        }

        private static PropertyInfo GetProperty<T>(string propertyName)
        {
            return typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        }
    }
}