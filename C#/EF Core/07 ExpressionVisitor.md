# `ExpressionVisitor` — Walking and Rewriting Trees

---

## What It Is

`ExpressionVisitor` is a base class in .NET that implements the **Visitor pattern** over an expression tree. It walks every node in the tree and lets you override what happens when each node type is visited. This is how EF Core's query translator works internally — it visits every node and emits SQL for each one.

You use it for two things: **reading** a tree (inspecting, extracting information) or **rewriting** a tree (replacing nodes, transforming the structure).

---

## The Default Behaviour — Pass-Through

By default, `ExpressionVisitor` walks the entire tree and returns it **unchanged**. Every `Visit*` method calls `base.Visit*(node)` which just returns the same node. You override only the node types you care about.

```csharp
public class MyVisitor : ExpressionVisitor
{
    // Override only what you want to intercept
    protected override Expression VisitMember(MemberExpression node)
    {
        Console.WriteLine($"Member accessed: {node.Member.Name}");
        return base.VisitMember(node);  // ← continue walking, return unchanged
    }
}
```

```csharp
Expression<Func<Product, bool>> expr = p => p.Price > 50 && !p.IsDiscontinued;

var visitor = new MyVisitor();
visitor.Visit(expr);
// Output:
// Member accessed: Price
// Member accessed: IsDiscontinued
```

The visitor walked the whole tree and printed every `MemberExpression` it found — no changes made.

---

## Reading a Tree — Extracting Column Names

A practical read-only use: find every property accessed in a query to build a list of touched columns for audit purposes.

```csharp
public class MemberCollector : ExpressionVisitor
{
    public List<string> Members { get; } = new();

    protected override Expression VisitMember(MemberExpression node)
    {
        Members.Add(node.Member.Name);
        return base.VisitMember(node);
    }
}
```

```csharp
Expression<Func<Product, bool>> expr =
    p => p.Price > 50 && p.Name.Contains("bike") && !p.IsDiscontinued;

var collector = new MemberCollector();
collector.Visit(expr);

Console.WriteLine(string.Join(", ", collector.Members));
// → Price, Name, IsDiscontinued
```

---

## Rewriting a Tree — The Real Power

To **rewrite** a node, override its `Visit*` method and return a **different** expression instead of the original. The visitor rebuilds the tree with your replacement in place.

---

### Example: Replace a Parameter — The Foundation of PredicateBuilder

The most common rewrite — swap one `ParameterExpression` for another. This is the core of `ParameterReplacer` used in combining expressions:

```csharp
public class ParameterReplacer : ExpressionVisitor
{
    private readonly ParameterExpression _from;
    private readonly ParameterExpression _to;

    public ParameterReplacer(ParameterExpression from, ParameterExpression to)
        => (_from, _to) = (from, to);

    protected override Expression VisitParameter(ParameterExpression node)
        => node == _from ? _to : base.VisitParameter(node);
        // ↑ if this is the parameter we're replacing, return the new one
        // ↑ otherwise, leave it alone
}
```

When the visitor hits a `ParameterExpression` matching `_from`, it returns `_to` instead. The rest of the tree rebuilds around the replacement automatically.

---

### Example: Swap a Constant Value at Runtime

Replace every occurrence of a specific constant — useful for query template reuse:

```csharp
public class ConstantReplacer : ExpressionVisitor
{
    private readonly object _oldValue;
    private readonly object _newValue;

    public ConstantReplacer(object oldValue, object newValue)
        => (_oldValue, _newValue) = (oldValue, newValue);

    protected override Expression VisitConstant(ConstantExpression node)
    {
        if (Equals(node.Value, _oldValue))
            return Expression.Constant(_newValue, node.Type);  // ← return new node

        return base.VisitConstant(node);  // ← leave other constants alone
    }
}
```

```csharp
Expression<Func<Product, bool>> template = p => p.Price > 50m;

// Rewrite: replace 50 with 100
var rewriter  = new ConstantReplacer(50m, 100m);
var rewritten = (Expression<Func<Product, bool>>)rewriter.Visit(template);

Console.WriteLine(rewritten);  // → p => p.Price > 100
```

---

### Example: Replace a Property Access — Column Renaming

Rename a property access — useful when mapping DTOs to entities with different property names:

```csharp
public class PropertyRenamer : ExpressionVisitor
{
    private readonly string _from;
    private readonly string _to;

    public PropertyRenamer(string from, string to)
        => (_from, _to) = (from, to);

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Member.Name == _from)
        {
            var newMember = node.Expression.Type.GetProperty(_to);
            return Expression.Property(node.Expression, newMember);
        }
        return base.VisitMember(node);
    }
}
```

---

## How EF Core Uses `ExpressionVisitor` Internally

EF Core's query pipeline is a chain of visitors. When you call `.ToList()`, roughly this happens:

```
Your LINQ expression tree
        ↓
NavigationExpandingExpressionVisitor   — expands Include() and navigation paths
        ↓
QueryTranslationPreprocessor           — normalises the tree
        ↓
RelationalQueryTranslatingVisitor      — walks nodes, emits SQL fragments
        ↓
SelectExpression                       — assembled SQL SELECT statement
        ↓
QuerySqlGenerator                      — renders SQL string with parameters
        ↓
Database execution
```

Every stage is an `ExpressionVisitor` subclass reading or rewriting the tree. When you write `.Where(p => p.Price > 50)`, the `RelationalQueryTranslatingVisitor` visits the `BinaryExpression (GreaterThan)`, sees a `MemberExpression (Price)` on the left and a `ConstantExpression (50)` on the right, and emits `[Price] > @p0`.

---

## The `Visit()` Entry Point — How Dispatch Works

The public `Visit(Expression node)` method is the entry point — it dispatches to the correct `Visit*` override based on the node's `NodeType`:

```csharp
public virtual Expression Visit(Expression node)
{
    return node?.NodeType switch
    {
        ExpressionType.Parameter    => VisitParameter((ParameterExpression)node),
        ExpressionType.MemberAccess => VisitMember((MemberExpression)node),
        ExpressionType.Constant     => VisitConstant((ConstantExpression)node),
        ExpressionType.AndAlso      => VisitBinary((BinaryExpression)node),
        ExpressionType.Lambda       => VisitLambda((LambdaExpression)node),
        // ... all other node types
        _ => node
    };
}
```

You only override what you need. The base class handles everything else.

---

## The Key Rule

```csharp
// ✅ Always call base when you don't modify — ensures the rest of the tree is walked
protected override Expression VisitMember(MemberExpression node)
{
    // do your reading here
    return base.VisitMember(node);  // ← walks the children
}

// ✅ Return a new node to rewrite — the visitor rebuilds the tree around it
protected override Expression VisitConstant(ConstantExpression node)
{
    if (ShouldReplace(node))
        return Expression.Constant(newValue, node.Type);  // ← replacement

    return base.VisitConstant(node);  // ← leave unchanged
}

// ❌ Never return null — breaks the tree reconstruction
protected override Expression VisitMember(MemberExpression node)
{
    return null;  // ← don't do this
}
```

---

## Summary

```
ExpressionVisitor   → base class for walking any expression tree
                    → override Visit*() methods to intercept specific node types
                    → return same node  = read only (no change)
                    → return new node   = rewrite the tree at that point

Common overrides:
  VisitParameter()  → swap parameters (PredicateBuilder combining)
  VisitMember()     → intercept property access (audit, column mapping)
  VisitConstant()   → replace values (query templates)
  VisitBinary()     → intercept operators (custom translations)

How EF Core uses it internally:
  → chain of visitors translate LINQ tree to SQL step by step
  → NavigationExpanding → QueryTranslation → SqlGeneration
  → every WHERE clause, JOIN, ORDER BY comes from a visitor emitting SQL

Key rule: always call base.Visit*(node) for nodes you don't modify
          → ensures the rest of the tree is still walked and rebuilt correctly
```
