using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Amazon.Common.ExcelExport
{
    public class ExportColumnBuilder<TModel>
    {
        public ExcelColumnInfo Build<TProperty>(Expression<Func<TModel, TProperty>> propertyExpr,
            string title,
            string format,
            int width)
        {
            var column = new ExcelColumnInfo();
            column.Property = GetPropertyInfo(propertyExpr);
            column.Title = title;
            column.Format = format;
            column.Width = width;

            return column;
        }

        public ExcelColumnInfo Build<TProperty>(Expression<Func<TModel, TProperty>> propertyExpr,
            string title)
        {
            return Build(propertyExpr, title, null, ExcelHelper.DEFAULT_WIDTH);
        }

        public ExcelColumnInfo Build<TProperty>(Expression<Func<TModel, TProperty>> propertyExpr,
            string title,
            int width)
        {
            return Build(propertyExpr, title, null, width);
        }

        public ExcelColumnInfo Build<TProperty>(Expression<Func<TModel, TProperty>> propertyExpr,
            string title,
            string format)
        {
            return Build(propertyExpr, title, format, ExcelHelper.DEFAULT_WIDTH);
        }

        public static PropertyInfo GetPropertyInfo<TSource, TProperty>(
            Expression<Func<TSource, TProperty>> propertyLambda)
        {
            Type type = typeof(TSource);

            MemberExpression member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda.ToString()));

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));

            if (type != propInfo.ReflectedType &&
                !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException(string.Format(
                    "Expresion '{0}' refers to a property that is not from type {1}.",
                    propertyLambda.ToString(),
                    type));

            return propInfo;
        }
    }
}
