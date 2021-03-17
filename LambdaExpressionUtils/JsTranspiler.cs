using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LambdaExpressionUtils
{
    public class JsTranspiler : ExpressionVisitor
    {
        private static readonly Dictionary<ExpressionType, string> _logicalOperators = new Dictionary<ExpressionType, string> {
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
            [ExpressionType.Add] = "+"
        };

        private static readonly Dictionary<Type, Func<object, string>> _typeConverters = new Dictionary<Type, Func<object, string>> {
            [typeof(string)] = value => $"'{value}'",
            [typeof(DateTime)] = value => $"datetime'{((DateTime)value).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}'",
            [typeof(bool)] = value => value.ToString().ToLower()
        };

        private static readonly Dictionary<string, string> _memberConverters = new Dictionary<string, string> {
            [$"{typeof(DateTime).Name}.{nameof(DateTime.Now)}"] = "new Date()",
            [$"{typeof(DateTime).Name}.{nameof(DateTime.Today)}"] = "(function() { var today = new Date(); today.setHours(0, 0, 0, 0); return today; })()",
        };


        private StringBuilder _jsBuilder = new StringBuilder();

        public List<string> Parameters { get; private set; }
        public string Body { get; private set; }

        public JsTranspiler(LambdaExpression lambdaExpression) {
            Parameters = lambdaExpression.Parameters.Select(p => p.Name).ToList();

            Visit(lambdaExpression.Body);
            Body = _jsBuilder.ToString();
        }

        public string GetJs() => $"function({string.Join(", ", Parameters)}) {{ return {Body}; }}";

        protected override Expression VisitUnary(UnaryExpression node) {
            if (_logicalOperators.ContainsKey(node.NodeType)) {
                _jsBuilder.Append($"{_logicalOperators[node.NodeType]}");
                Visit(node.Operand);
                return node;
            }
            return base.Visit(node);
        }

        protected override Expression VisitBinary(BinaryExpression node) {
            _jsBuilder.Append("(");
            Visit(node.Left);
            _jsBuilder.Append($" {_logicalOperators[node.NodeType]} ");
            Visit(node.Right);
            _jsBuilder.Append(")");
            return node;
        }

        protected override Expression VisitMember(MemberExpression node) {
            if (node.Expression != null) {
                var propertyName = node.Member.Name;
                var parameterName = (node.Expression as ParameterExpression)?.Name;
                MemberExpression runningExpression = node.Expression as MemberExpression;
                while (runningExpression != null) {
                    propertyName = $"{runningExpression.Member.Name}.{propertyName}";
                    parameterName = (runningExpression.Expression as ParameterExpression)?.Name;
                    runningExpression = runningExpression.Expression as MemberExpression;
                }
                _jsBuilder.Append($"{parameterName}.{propertyName}");
                return node;
            }
            else if (_memberConverters.ContainsKey($"{node.Member.DeclaringType.Name}.{node.Member.Name}")) {
                _jsBuilder.Append(_memberConverters[$"{node.Member.DeclaringType.Name}.{node.Member.Name}"]);
                return node;
            }
            return base.VisitMember(node);
        }

        protected override Expression VisitConstant(ConstantExpression node) {
            string value;
            if (_typeConverters.ContainsKey(node.Value.GetType())) {
                value = _typeConverters[node.Value.GetType()](node.Value);
            }
            else {
                value = node.Value.ToString();
            }
            _jsBuilder.Append(value);
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node) {
            if (node.Type == typeof(string) && node.Method.Name == nameof(string.Format)) {
                return visitStringFormatCall(node);
            }
            return base.VisitMethodCall(node);
        }

        private Expression visitStringFormatCall(MethodCallExpression node) {
            // string.Format("{0} - {1}", obj1, obj2) or $"{obj1} - {obj1}" => (obj1 + ' - ' + obj2)
            // ... Arguments[0] = "{0} - {1}"   or  Arguments[0] = "{0} - {1}"
            // ... Arguments[1] = obj1              Arguments[1].Arguments[0] = obj1
            // ... Arguments[2] = obj2              Arguments[1].Arguments[1] = obj2

            List<Expression> arguments;
            if (node.Arguments.Count == 2 && (node.Arguments[1] is NewArrayExpression)) {
                arguments = (node.Arguments[1] as NewArrayExpression).Expressions.ToList();
            }
            else {
                arguments = node.Arguments.Skip(1).ToList();
            }

            var template = (node.Arguments.First() as ConstantExpression).Value.ToString();
            var segments = template.GetSegments();

            _jsBuilder.Append("(");
            foreach (var segment in segments) {
                if (segment != segments.First()) {
                    _jsBuilder.Append(" + ");
                }
                if (!segment.IsMatch) {
                    _jsBuilder.Append($"'{segment.Value}'");
                }
                else {
                    var index = int.Parse(segment.Value.Substring(1, segment.Value.Length - 2));
                    Visit(arguments[index]);
                }
            }
            _jsBuilder.Append(")");

            return node;
        }
    }
}
