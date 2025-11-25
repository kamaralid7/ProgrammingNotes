A **generic constraint** in C# is a rule you place on a type parameter to **limit what kinds of types are allowed** when someone uses your generic class, method, or interface.

It ensures that the type used meets specific requirements (like having a constructor, being a class, implementing an interface, etc.).

---

# 🔍 Why Constraints Exist

Without constraints, the compiler knows **nothing** about a generic type `T`.
So these operations are *illegal*:

```csharp
public void DoSomething<T>(T item)
{
    // item.SomeMethod(); // ❌ Compiler doesn't know T has this method
}
```

Constraints tell the compiler what it can expect.

---

# 🧱 Generic Constraint Syntax

```csharp
where T : constraint
```

Example:

```csharp
public class Repository<T> where T : IEntity
{
}
```

---

# 📌 Types of Generic Constraints

## 1. **Base Class Constraint**

Restrict T to a specific base class.

```csharp
where T : Person
```

T must be Person or a derived class.

---

## 2. **Interface Constraint**

Restrict T to types that implement an interface.

```csharp
where T : IDisposable
```

---

## 3. **Reference Type Constraint**

Only classes allowed.

```csharp
where T : class
```

---

## 4. **Value Type Constraint**

Only structs allowed (including nullable value types).

```csharp
where T : struct
```

---

## 5. **Constructor Constraint**

Requires a **public parameterless constructor**.

```csharp
where T : new()
```

Lets you do:

```csharp
T obj = new T();
```

---

## 6. **Unmanaged Constraint**

Only **unmanaged types** allowed (primitive or containing only unmanaged fields).

```csharp
where T : unmanaged
```

Useful for interop and memory-efficient operations.

---

## 7. **Notnull Constraint (C# 8+)**

T must be a non-nullable reference or value type.

```csharp
where T : notnull
```

---

## 8. **Multiple Constraints**

You can stack them:

```csharp
where T : class, IShape, new()
```

Order rule:

* Base class first
* Then interfaces
* Then `new()` last

---

# ✔ Full Example

### A generic repository class using multiple constraints:

```csharp
public class Repository<T> 
    where T : Entity, IValidatable, new()
{
    public T Create()
    {
        T item = new T();
        item.Validate();
        return item;
    }
}
```

This says:

* `T` must inherit from `Entity`
* `T` must implement `IValidatable`
* `T` must have a parameterless constructor

---

# 🎯 Why Use Generic Constraints?

### ✓ Allows safe operations

Compiler knows what T can do.

### ✓ Allows object creation (`new()`)

### ✓ Prevents invalid type usage

Prevents someone from doing:

```csharp
Repository<int> repo = new Repository<int>(); // ❌
```

### ✓ Improves performance

Value-type constraints avoid boxing.

---

# 📝 Summary Table

| Constraint                 | Meaning                             |
| -------------------------- | ----------------------------------- |
| `where T : class`          | Must be a reference type            |
| `where T : struct`         | Must be a value type                |
| `where T : new()`          | Must have parameterless constructor |
| `where T : SomeBaseClass`  | Must inherit base class             |
| `where T : ISomeInterface` | Must implement interface            |
| `where T : unmanaged`      | Must be an unmanaged type           |
| `where T : notnull`        | Cannot be nullable                  |


