Below is a **clear, accurate, well-structured explanation** of the concepts you asked about in C#:
**abstract methods, virtual methods, overriding, the `new` keyword (method shadowing), and method-invocation behavior when base and derived classes are involved.**

---

# ⭐ 1. **Abstract Methods**

An **abstract method** is a method with **no implementation** in the base class.
It **must** be implemented (overridden) in a non-abstract derived class.

### 🔹 Rules:

* Can only exist inside an `abstract` class.
* Cannot have a body.
* Must be overridden using `override` in derived classes.

### ✔️ Example:

```csharp
public abstract class Animal
{
    public abstract void Speak(); // No body → must be implemented later
}

public class Dog : Animal
{
    public override void Speak() => Console.WriteLine("Dog barks");
}
```

---

# ⭐ 2. **Virtual Methods**

A **virtual method** has a *default implementation* in the base class, but derived classes **may override** it.

### ✔️ Example:

```csharp
public class Animal
{
    public virtual void Speak() => Console.WriteLine("Animal speaks");
}

public class Dog : Animal
{
    public override void Speak() => Console.WriteLine("Dog barks");
}
```

You use `virtual` when:

* The base version works,
  but
* Derived classes may offer a more specialized behavior.

---

# ⭐ 3. **Overriding**

**Overriding** means a derived class supplies its own version of a virtual/abstract method.

### Requirements:

* Base method must be `virtual`, `abstract`, or `override`.
* Derived method must use `override`.

### ✔️ Example:

```csharp
public class Animal
{
    public virtual void Speak() => Console.WriteLine("Animal speaks");
}

public class Cat : Animal
{
    public override void Speak() => Console.WriteLine("Cat meows");
}
```

When overriding occurs:

* Behavior depends on *runtime type* of the object
  (not the variable type).

---

# ⭐ 4. **`new` Keyword (Method Shadowing)**

The `new` keyword **hides** a base class method instead of overriding it.

This is also called:

* **method shadowing**
* **method hiding**

### ✔️ Example:

```csharp
public class Animal
{
    public void Speak() => Console.WriteLine("Animal speaks");
}

public class Dog : Animal
{
    public new void Speak() => Console.WriteLine("Dog barks");
}
```

### 🧠 Behavior:

* If the object is referenced as **derived type**, the new method runs.
* If referenced as **base type**, the base method runs (because no polymorphism is used).

---

# ⭐ 5. **Behavior When Calling Through Base vs Derived Reference**

### Case A — **Overriding (virtual/override)**

```csharp
Animal a = new Dog();
a.Speak();
```

Output:

```
Dog barks
```

🔹 Because overriding supports **runtime polymorphism**.

### Case B — **Shadowing with `new`**

```csharp
Animal a = new Dog();
a.Speak();
```

Output:

```
Animal speaks
```

🔹 Because the base version is chosen when the reference type is `Animal`.

```csharp
Dog d = new Dog();
d.Speak();  // Dog barks
```

### Summary:

| Mechanism | Base Ref Calls Base? | Base Ref Calls Derived? | Polymorphic? |
| --------- | -------------------- | ----------------------- | ------------ |
| override  | ❌ No                 | ✔️ Yes                  | ✔️ Yes       |
| new       | ✔️ Yes               | ❌ No                    | ❌ No         |

---

# ⭐ 6. **Multiple Inheritance and Method Invocation in C#**

C# **does NOT support multiple inheritance of classes**, but it **does** support multiple interface inheritance.

### ❗ If two interfaces contain methods with the same signature:

The implementing class must resolve the conflict.

Example:

```csharp
interface IA { void Speak(); }
interface IB { void Speak(); }

class C : IA, IB
{
    void IA.Speak() => Console.WriteLine("IA speak");
    void IB.Speak() => Console.WriteLine("IB speak");
}
```

### Calling:

```csharp
IA a = new C();
a.Speak(); // IA speak

IB b = new C();
b.Speak(); // IB speak
```

👉 Behavior depends entirely on **which interface reference** is used.

---

# ⭐ Final Cheat Sheet

| Concept                                 | Meaning                               | Key Notes                    |
| --------------------------------------- | ------------------------------------- | ---------------------------- |
| **abstract method**                     | No implementation; must be overridden | Requires abstract class      |
| **virtual method**                      | Has implementation; can be overridden | Supports polymorphism        |
| **override**                            | Replace base implementation           | Must match signature exactly |
| **new (shadowing)**                     | Hides base method                     | No polymorphism              |
| **Base reference to overridden method** | Calls derived method                  | Runtime binding              |
| **Base reference to hidden method**     | Calls base method                     | Compile-time binding         |
| **Multiple inheritance**                | Not supported for classes             | Interfaces only              |
