# Lambda Expression Utils

## Javascript Transpiler

Converts a C# Lambda Expression into a Javascript function, using ExpressionVisitor class

```
var jsTranspiler = new JsTranspiler(wh => wh.WarehouseId > 1 && wh.WarehouseId < 10);

var js = jsTranspiler.GetJs();
// => function(wh) { return ((wh.WarehouseId > 1) && (wh.WarehouseId < 10)); }

var parameters = jsTranspiler.Parameters;
// => [wh]

var body = jsTranspiler.Body;	        
// => ((wh.WarehouseId > 1) && (wh.WarehouseId < 10)) }
```
