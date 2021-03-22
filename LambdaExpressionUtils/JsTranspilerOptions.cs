using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LambdaExpressionUtils
{
    public class JsTranspilerOptions
    {
        public Dictionary<ExpressionType, string> Operators { get; protected set; } = new Dictionary<ExpressionType, string>() {
            [ExpressionType.Not] = "!",
            [ExpressionType.Convert] = "",
            [ExpressionType.GreaterThan] = ">",
            [ExpressionType.GreaterThanOrEqual] = ">=",
            [ExpressionType.LessThan] = "<",
            [ExpressionType.LessThanOrEqual] = "<=",
            [ExpressionType.Equal] = "===",
            [ExpressionType.NotEqual] = "!==",
            [ExpressionType.AndAlso] = "&&",
            [ExpressionType.OrElse] = "||",
            [ExpressionType.Add] = "+",
            [ExpressionType.Subtract] = "-",
            [ExpressionType.Multiply] = "*",
            [ExpressionType.Divide] = "/"
        };

        public Dictionary<Type, Func<object, string>> TypeConverters { get; protected set; } = new Dictionary<Type, Func<object, string>> {
            [typeof(string)] = value => $"'{value}'",
            [typeof(DateTime)] = value => $"datetime'{((DateTime)value).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}'",
            [typeof(bool)] = value => value.ToString().ToLower()
        };

        public Dictionary<string, string> NonExpressionMembers { get; protected set; } = new Dictionary<string, string> {
            [$"{typeof(DateTime).Name}.{nameof(DateTime.Now)}"] = "new Date()",
            [$"{typeof(DateTime).Name}.{nameof(DateTime.Today)}"] = "(function() { var today = new Date(); today.setHours(0, 0, 0, 0); return today; })()",
        };

        public string BodyFormat { get; protected set; } = "{0}";               // body

        public string UnaryFormat { get; protected set; } = "{0}";              // operator

        public string BinaryBeforeLeftFormat { get; protected set; } = "(";
        public string BinaryAfterLeftFormat { get; protected set; } = "";
        public string BinaryOperatorFormat { get; protected set; } = " {0} ";   // operator
        public string BinaryBeforeRightFormat { get; protected set; } = "";
        public string BinaryAfterRightFormat { get; protected set; } = ")";

        public string MemberFormat { get; protected set; } = "{0}.{1}";         // parameter name, property name/path

        public string ParameterFormat { get; protected set; } = "{0}";          // parameter name

        public string ConstantFormat { get; protected set; } = "{0}";           // value or converted value

        public string ConditionalBeforeTestFormat { get; protected set; } = "(";
        public string ConditionalAfterTestFormat { get; protected set; } = " ? ";
        public string ConditionalAfterIfTrueFormat { get; protected set; } = " : ";
        public string ConditionalAfterIfFalseFormat { get; protected set; } = ")";

        public string StringFormatCallBeforeFormat { get; protected set; } = "(";
        public string StringFormatCallAfter1stLoopFormat { get; protected set; } = " + ";
        public string StringFormatCallNoMatchInLoopFormat { get; protected set; } = "'{0}'";  // string segment inside format template
        public string StringFormatCallAfterFormat { get; protected set; } = ")";

        // sigletons
        public static JsTranspilerOptions Default = new JsTranspilerOptions();

        public static JsTranspilerOptions Template = new JsTranspilerOptions() {
            BodyFormat = "${{{0}}}",
            MemberFormat = "{1}"
        };

        public static JsTranspilerOptions ValueGetter = new JsTranspilerOptions() {
            MemberFormat = "{0}.{1}.value()"
        };
    }
}
