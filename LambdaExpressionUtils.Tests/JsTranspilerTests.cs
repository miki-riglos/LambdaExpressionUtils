using LambdaExpressionUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LambdaExpressionsTests
{
    public enum Color
    {
        Red,
        Blue
    }

    public class BusinessUnit
    {
        public int BusinessUnitId { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class Warehouse
    {
        public int WarehouseId { get; set; }
        public string Name { get; set; }
        public BusinessUnit BusinessUnit { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
        public Color Color { get; set; }
    }

    [TestClass]
    public class JsTranspilerTests
    {
        [TestMethod]
        public void PredicatesTest() {
            var predicates = new List<Expression<Func<Warehouse, bool>>>() {
                wh => wh.WarehouseId == 1,
                wh => wh.WarehouseId != 1,
                wh => wh.WarehouseId > 1 && wh.WarehouseId < 10,
                wh => wh.WarehouseId >= 1 && wh.WarehouseId <= 10,
                wh => wh.BusinessUnit.Name == "Main",
                wh => wh.WarehouseId == 1 && wh.BusinessUnit.Name == "Main",
                wh => (wh.WarehouseId == 1 || wh.BusinessUnit.Name == "Main") && wh.Name == "Main",
                wh => wh.Active == true,
                wh => wh.Active == false,
                wh => wh.Active,
                wh => !wh.Active,
                wh => !wh.BusinessUnit.Active
            };
            var expectedJsList = new List<string>() {
                "function(wh) { return (wh.WarehouseId === 1); }",
                "function(wh) { return (wh.WarehouseId !== 1); }",
                "function(wh) { return ((wh.WarehouseId > 1) && (wh.WarehouseId < 10)); }",
                "function(wh) { return ((wh.WarehouseId >= 1) && (wh.WarehouseId <= 10)); }",
                "function(wh) { return (wh.BusinessUnit.Name === 'Main'); }",
                "function(wh) { return ((wh.WarehouseId === 1) && (wh.BusinessUnit.Name === 'Main')); }",
                "function(wh) { return (((wh.WarehouseId === 1) || (wh.BusinessUnit.Name === 'Main')) && (wh.Name === 'Main')); }",
                "function(wh) { return (wh.Active === true); }",
                "function(wh) { return (wh.Active === false); }",
                "function(wh) { return wh.Active; }",
                "function(wh) { return !wh.Active; }",
                "function(wh) { return !wh.BusinessUnit.Active; }"
            };

            foreach (var pair in predicates.Zip(expectedJsList, (predicate, expectedJs) => new { Predicate = predicate, ExpectedJs = expectedJs })) {
                var jsTranspiler = new JsTranspiler(pair.Predicate);
                var actualJs = jsTranspiler.GetJs();
                Assert.AreEqual(pair.ExpectedJs, actualJs);
            }
        }

        [TestMethod]
        public void ReturningStringTest() {
            var returnStringLambdas = new List<Expression<Func<Warehouse, string>>>() {
                wh => wh.Name,
                wh => wh.BusinessUnit.Name,
                wh => wh.Name + "-",
                wh => wh.Name + "-" + wh.BusinessUnit.Name,
                wh => string.Format("{0}-{1}", wh.Name, wh.BusinessUnit.Name),
                wh => string.Format(">{0}{1}<", wh.Name, wh.BusinessUnit.Name),
                wh => string.Format(">{0}{1}<", wh.Name, wh.BusinessUnit.Name) + "|",
                wh => $"{wh.Name}-{wh.BusinessUnit.Name}",
                wh => $"{wh.Name}-{wh.BusinessUnit.BusinessUnitId}-{wh.CreatedAt}-{wh.WarehouseId}",
                wh => $"{wh.Name}-{wh.CreatedAt}-{wh.WarehouseId}",
                wh => string.Format("{0}-{1}-{2}-{3}", wh.Name, wh.BusinessUnit.Name, wh.CreatedAt, wh.WarehouseId),
                wh => string.Format("{0}-{0}-{1}-{2}", wh.Name, wh.BusinessUnit.Name, wh.CreatedAt),
                wh => wh.WarehouseId > 1 ? wh.Name : wh.BusinessUnit.Name
            };
            var expectedJsList = new List<string>() {
                "function(wh) { return wh.Name; }",
                "function(wh) { return wh.BusinessUnit.Name; }",
                "function(wh) { return (wh.Name + '-'); }",
                "function(wh) { return ((wh.Name + '-') + wh.BusinessUnit.Name); }",
                "function(wh) { return (wh.Name + '-' + wh.BusinessUnit.Name); }",
                "function(wh) { return ('>' + wh.Name + wh.BusinessUnit.Name + '<'); }",
                "function(wh) { return (('>' + wh.Name + wh.BusinessUnit.Name + '<') + '|'); }",
                "function(wh) { return (wh.Name + '-' + wh.BusinessUnit.Name); }",
                "function(wh) { return (wh.Name + '-' + wh.BusinessUnit.BusinessUnitId + '-' + wh.CreatedAt + '-' + wh.WarehouseId); }",
                "function(wh) { return (wh.Name + '-' + wh.CreatedAt + '-' + wh.WarehouseId); }",
                "function(wh) { return (wh.Name + '-' + wh.BusinessUnit.Name + '-' + wh.CreatedAt + '-' + wh.WarehouseId); }",
                "function(wh) { return (wh.Name + '-' + wh.Name + '-' + wh.BusinessUnit.Name + '-' + wh.CreatedAt); }",
                "function(wh) { return (wh.WarehouseId > 1) ? wh.Name : wh.BusinessUnit.Name; }"
            };

            foreach (var pair in returnStringLambdas.Zip(expectedJsList, (lambda, expectedJs) => new { Lambda = lambda, ExpectedJs = expectedJs })) {
                var jsTranspiler = new JsTranspiler(pair.Lambda);
                var actualJs = jsTranspiler.GetJs();
                Assert.AreEqual(pair.ExpectedJs, actualJs);
            }
        }

        [TestMethod]
        public void LambdaExpresionsTest() {
            var expressions = new List<Expression<Func<Warehouse, object>>>() {
                wh => wh.BusinessUnit.Name,
                wh => wh.CreatedAt,
                wh => true,
                wh => false,
                wh => 1,
                wh => DateTime.Now,
                wh => DateTime.Today,
                wh => wh.CreatedAt > DateTime.Now,
                wh => wh.CreatedAt > DateTime.Today,
                wh => wh.Color == Color.Red,
                wh => wh.Color == Color.Blue,
                wh => wh == null || wh.Color == Color.Blue
            };
            var expectedJsList = new List<string>() {
                "function(wh) { return wh.BusinessUnit.Name; }",
                "function(wh) { return wh.CreatedAt; }",
                "function(wh) { return true; }",
                "function(wh) { return false; }",
                "function(wh) { return 1; }",
                "function(wh) { return new Date(); }",
                "function(wh) { return (function() { var today = new Date(); today.setHours(0, 0, 0, 0); return today; })(); }",
                "function(wh) { return (wh.CreatedAt > new Date()); }",
                "function(wh) { return (wh.CreatedAt > (function() { var today = new Date(); today.setHours(0, 0, 0, 0); return today; })()); }",
                "function(wh) { return (wh.Color === 0); }",
                "function(wh) { return (wh.Color === 1); }",
                "function(wh) { return ((wh === null) || (wh.Color === 1)); }"
            };

            foreach (var pair in expressions.Zip(expectedJsList, (expression, expectedJs) => new { Expression = expression, ExpectedJs = expectedJs })) {
                var jsTranspiler = new JsTranspiler(pair.Expression);
                var actualJs = jsTranspiler.GetJs();
                Assert.AreEqual(pair.ExpectedJs, actualJs);
            }
        }
    }
}
