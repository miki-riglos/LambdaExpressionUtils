using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LambdaExpressionUtils
{
    public class JsTranspiler : ExpressionVisitor
    {

        private readonly JsTranspilerOptions _options;
        private StringBuilder _jsBuilder = new StringBuilder();

        public List<string> Parameters { get; private set; }
        public string Body { get; private set; }

        public JsTranspiler(LambdaExpression lambdaExpression, JsTranspilerOptions options = null) {
            _options = options ?? JsTranspilerOptions.Default;

            Parameters = lambdaExpression.Parameters.Select(p => p.Name).ToList();

            Visit(lambdaExpression.Body);
            Body = string.Format(_options.BodyFormat, _jsBuilder.ToString());
        }

        public string GetJs() => $"function({string.Join(", ", Parameters)}) {{ return {Body}; }}";

        protected override Expression VisitUnary(UnaryExpression node) {
            if (_options.Operators.ContainsKey(node.NodeType)) {
                _jsBuilder.AppendFormat(_options.UnaryFormat, _options.Operators[node.NodeType]);
                Visit(node.Operand);
                return node;
            }
            return base.Visit(node);
        }

        protected override Expression VisitBinary(BinaryExpression node) {
            _jsBuilder.AppendFormat(_options.BinaryBeforeLeftFormat);
            Visit(node.Left);
            _jsBuilder.AppendFormat(_options.BinaryAfterLeftFormat);

            _jsBuilder.AppendFormat(_options.BinaryOperatorFormat, _options.Operators[node.NodeType]);

            _jsBuilder.AppendFormat(_options.BinaryBeforeRightFormat);
            Visit(node.Right);
            _jsBuilder.AppendFormat(_options.BinaryAfterRightFormat);

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
                _jsBuilder.AppendFormat(_options.MemberFormat, parameterName, propertyName);
                return node;
            }
            else if (_options.NonExpressionMembers.ContainsKey($"{node.Member.DeclaringType.Name}.{node.Member.Name}")) {
                _jsBuilder.Append(_options.NonExpressionMembers[$"{node.Member.DeclaringType.Name}.{node.Member.Name}"]);
                return node;
            }
            return base.VisitMember(node);
        }

        protected override Expression VisitParameter(ParameterExpression node) {
            _jsBuilder.AppendFormat(_options.ParameterFormat, node.Name);
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node) {
            string value;
            if (node.Value == null) {
                value = "null";
            }
            else if (_options.TypeConverters.ContainsKey(node.Value.GetType())) {
                value = _options.TypeConverters[node.Value.GetType()](node.Value);
            }
            else {
                value = node.Value.ToString();
            }
            _jsBuilder.AppendFormat(_options.ConstantFormat, value);
            return node;
        }

        protected override Expression VisitConditional(ConditionalExpression node) {
            _jsBuilder.AppendFormat(_options.ConditionalBeforeTestFormat);
            Visit(node.Test);
            _jsBuilder.AppendFormat(_options.ConditionalAfterTestFormat);
            Visit(node.IfTrue);
            _jsBuilder.AppendFormat(_options.ConditionalAfterIfTrueFormat);
            Visit(node.IfFalse);
            _jsBuilder.AppendFormat(_options.ConditionalAfterIfFalseFormat);
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node) {
            if (node.Method.DeclaringType == typeof(string) && node.Method.Name == nameof(string.Format)) {
                return visitStringFormatCall(node);
            }
            return base.VisitMethodCall(node);
        }

        protected List<Expression> getStringFormatCallArguments(MethodCallExpression node) {
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
            return arguments;
        }

        protected virtual Expression visitStringFormatCall(MethodCallExpression node) {
            List<Expression> arguments = getStringFormatCallArguments(node);
            var template = (node.Arguments.First() as ConstantExpression).Value.ToString();
            var segments = template.GetSegments();

            _jsBuilder.AppendFormat(_options.StringFormatCallBeforeFormat);
            foreach (var segment in segments) {
                if (segment != segments.First()) {
                    _jsBuilder.AppendFormat(_options.StringFormatCallAfter1stLoopFormat);
                }
                if (!segment.IsMatch) {
                    _jsBuilder.AppendFormat(_options.StringFormatCallNoMatchInLoopFormat, segment.Value);
                }
                else {
                    var index = int.Parse(segment.Value.Substring(1, segment.Value.Length - 2));
                    Visit(arguments[index]);
                }
            }
            _jsBuilder.AppendFormat(_options.StringFormatCallAfterFormat);

            return node;
        }

        // static helpers
        static public string GetJs(LambdaExpression lambdaExpression) {
            var jsTranspiler = new JsTranspiler(lambdaExpression);
            return jsTranspiler.GetJs();
        }

        static public string GetTemplate(LambdaExpression lambdaExpression) {
            var jsTranspiler = new JsTranspiler(lambdaExpression, JsTranspilerOptions.Template);
            return jsTranspiler.Body;
        }

        static public string GetJsWithValueGetter(LambdaExpression lambdaExpression) {
            var jsTranspiler = new JsTranspiler(lambdaExpression, JsTranspilerOptions.ValueGetter);
            return jsTranspiler.GetJs();
        }
    }
}
