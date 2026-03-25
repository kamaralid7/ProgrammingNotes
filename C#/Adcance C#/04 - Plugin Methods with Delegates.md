# Advanced C# — 04: Plugin Methods with Delegates

---

## The Core Idea

Instead of hardcoding behavior inside a class, you expose **delegate "slots"** that callers can plug methods into. This makes your class extensible without ever modifying it.

---

## The Problem Without Delegates

```csharp
// BAD — you have to modify this class every time you add a new format
public class DataExporter
{
    public void Export(List<string> data, string format)
    {
        if (format == "csv")
            ExportAsCsv(data);
        else if (format == "json")
            ExportAsJson(data);
        else if (format == "xml")   // keeps growing forever...
            ExportAsXml(data);
    }
}
```

---

## The Fix — Inject Behavior via Delegate

```csharp
public class DataExporter
{
    // The "plugin slot" — caller decides how to format data
    public void Export(List<string> data, Action<List<string>> formatter)
    {
        Console.WriteLine("Preparing data...");
        formatter(data);    // call whatever was plugged in
        Console.WriteLine("Done.");
    }
}

var data = new List<string> { "Alice", "Bob", "Charlie" };
var exporter = new DataExporter();

// Plug in CSV format
exporter.Export(data, items =>
    Console.WriteLine(string.Join(",", items)));

// Plug in JSON format
exporter.Export(data, items =>
    Console.WriteLine("[" + string.Join(", ", items.Select(i => $"\"{i}\"")) + "]"));
```

`DataExporter` never needs to change — you plug in new behavior from outside.

---

## Building a Real Plugin Pipeline

A list of delegate plugins, each transforming data and passing it to the next.

```csharp
public class TextProcessor
{
    private readonly List<Func<string, string>> _plugins = new();

    // Register a plugin — returns 'this' for method chaining
    public TextProcessor Use(Func<string, string> plugin)
    {
        _plugins.Add(plugin);
        return this;
    }

    // Run text through all plugins in order
    public string Process(string input)
    {
        string result = input;
        foreach (var plugin in _plugins)
            result = plugin(result); // each plugin feeds into the next
        return result;
    }
}

// Usage — chain plugins
var processor = new TextProcessor()
    .Use(text => text.Trim())           // Plugin 1: remove whitespace
    .Use(text => text.ToUpper())        // Plugin 2: uppercase
    .Use(text => text.Replace(" ", "_")) // Plugin 3: replace spaces
    .Use(text => $"[{text}]");          // Plugin 4: wrap in brackets

string result = processor.Process("  hello world  ");
Console.WriteLine(result); // [HELLO_WORLD]
```

---

## Plugin with Context and Short-Circuiting

When plugins need shared state and the ability to stop the pipeline early.

```csharp
public class RequestContext
{
    public string Input   { get; set; } = "";
    public string Output  { get; set; } = "";
    public bool Handled   { get; set; } = false; // short-circuit flag
}

public class MiddlewarePipeline
{
    private readonly List<Action<RequestContext>> _middlewares = new();

    public MiddlewarePipeline Use(Action<RequestContext> middleware)
    {
        _middlewares.Add(middleware);
        return this;
    }

    public RequestContext Run(string input)
    {
        var ctx = new RequestContext { Input = input };
        foreach (var middleware in _middlewares)
        {
            if (ctx.Handled) break; // stop if a plugin has handled it
            middleware(ctx);
        }
        return ctx;
    }
}

// Wire up plugins
var pipeline = new MiddlewarePipeline()
    .Use(ctx =>
    {
        Console.WriteLine($"Logging: {ctx.Input}");
    })
    .Use(ctx =>
    {
        if (ctx.Input.Contains("banned"))
        {
            ctx.Output  = "Rejected!";
            ctx.Handled = true; // short-circuit — skip remaining plugins
        }
    })
    .Use(ctx =>
    {
        ctx.Output = ctx.Input.ToUpper(); // only runs if not handled
    });

var r1 = pipeline.Run("hello world");
Console.WriteLine(r1.Output); // HELLO WORLD

var r2 = pipeline.Run("banned word here");
Console.WriteLine(r2.Output); // Rejected!
```

This is the same model ASP.NET Core middleware uses internally.

---

## Named Plugin Registry

Register plugins by name — load behavior from config or user input.

```csharp
public class PluginRegistry
{
    private readonly Dictionary<string, Func<string, string>> _plugins = new();

    public void Register(string name, Func<string, string> plugin)
    {
        _plugins[name] = plugin;
    }

    public string Apply(string pluginName, string input)
    {
        if (!_plugins.TryGetValue(pluginName, out var plugin))
            throw new KeyNotFoundException($"Plugin '{pluginName}' not found.");
        return plugin(input);
    }

    public IEnumerable<string> AvailablePlugins => _plugins.Keys;
}

// Setup
var registry = new PluginRegistry();
registry.Register("upper",   text => text.ToUpper());
registry.Register("lower",   text => text.ToLower());
registry.Register("reverse", text => new string(text.Reverse().ToArray()));
registry.Register("trim",    text => text.Trim());

// Use by name — e.g., driven by config or user selection
Console.WriteLine(registry.Apply("upper",   "hello"));   // HELLO
Console.WriteLine(registry.Apply("reverse", "hello"));   // olleh
Console.WriteLine(registry.Apply("trim",    "  hi  "));  // hi
```

---

## Event-Based Plugin Hooks

Use events as plugin "hooks" that external code can subscribe to.

```csharp
public class OrderProcessor
{
    // Plugin hooks — external code subscribes to customize behavior
    public event Action<Order>?  BeforeProcess;
    public event Action<Order>?  AfterProcess;
    public event Action<Order, Exception>? OnError;

    public void Process(Order order)
    {
        try
        {
            BeforeProcess?.Invoke(order);   // run all before-hooks
            // ... core processing logic ...
            Console.WriteLine($"Processing order #{order.Id}");
            AfterProcess?.Invoke(order);    // run all after-hooks
        }
        catch (Exception ex)
        {
            OnError?.Invoke(order, ex);     // run error hooks
            throw;
        }
    }
}

// Plug in behavior without changing OrderProcessor
var processor = new OrderProcessor();

processor.BeforeProcess += order => Console.WriteLine($"Validating order #{order.Id}");
processor.BeforeProcess += order => Console.WriteLine($"Checking inventory for #{order.Id}");
processor.AfterProcess  += order => Console.WriteLine($"Sending confirmation email for #{order.Id}");
processor.OnError       += (order, ex) => Console.WriteLine($"Order #{order.Id} failed: {ex.Message}");

processor.Process(new Order { Id = 42 });
```

---

## Combining Plugin Patterns

A complete example combining registry, pipeline, and context:

```csharp
public class ImagePipeline
{
    private readonly List<Func<byte[], byte[]>> _steps = new();

    public ImagePipeline AddStep(string name, Func<byte[], byte[]> step)
    {
        Console.WriteLine($"  → Adding step: {name}");
        _steps.Add(step);
        return this;
    }

    public byte[] Execute(byte[] imageData)
    {
        byte[] result = imageData;
        foreach (var step in _steps)
            result = step(result);
        return result;
    }
}

// Usage
var pipeline = new ImagePipeline()
    .AddStep("Resize",      img => ResizeImage(img, 800, 600))
    .AddStep("Grayscale",   img => ConvertToGrayscale(img))
    .AddStep("Compress",    img => CompressImage(img, quality: 80))
    .AddStep("Watermark",   img => AddWatermark(img, "© 2024"));

byte[] finalImage = pipeline.Execute(originalImageBytes);
```

---

## Summary

| Pattern | When to Use |
|---------|-------------|
| **Delegate slot** (`Action`/`Func` parameter) | Simple single-step injection |
| **Plugin list** (list of `Func<T,T>`) | Sequential transformation pipeline |
| **Context + short-circuit** | Middleware where steps can halt the chain |
| **Named registry** (`Dictionary<string, Func>`) | Config/runtime-driven plugin selection |
| **Event hooks** | Before/after hooks on lifecycle methods |

The key rule: **the host class defines the shape, the caller provides the behavior**.

---

*Previous: [03 - Higher-Order Functions](./03%20-%20Higher-Order%20Functions.md)*
*Next: [05 - Generics & Constraints](./05%20-%20Generics%20%26%20Constraints.md)*
