# Advanced C# — 12: Delegate Compatibility

> *"When is one delegate 'the same' as another? When can you substitute one for another? The answer reveals how the type system thinks."*

---

## Type Compatibility — Structural Matching

C# delegates use **structural compatibility** — if the signature matches, it fits. The delegate *name* doesn't matter.

```csharp
delegate int Transform(int x);
delegate int MathOp(int x);     // different name, same signature

int MyDouble(int x) => x * 2;

Transform t = MyDouble; // ✅
MathOp    m = MyDouble; // ✅ — same method, two different delegate types

// But you CANNOT directly assign one to the other
Transform t2 = m;                  // ❌ compile error — different types
Transform t3 = new Transform(m);   // ✅ explicit wrapping works
```

> Same *shape*, different *names* — compatible with a method, not directly with each other.

---

## Parameter Compatibility — Contravariance

> *"If your delegate can handle a broad type, it can handle a narrower one too."*

A method with a **less derived (broader) parameter** is compatible with a delegate expecting a **more derived (narrower) parameter**:

```csharp
class Animal { public string Name = "Animal"; }
class Dog : Animal { }

void HandleAnimal(Animal a) => Console.WriteLine($"Handling {a.Name}");

// Delegate expects a Dog handler
Action<Dog> dogHandler = HandleAnimal; // ✅ contravariance

dogHandler(new Dog()); // safe — Dog IS-A Animal
```

Why safe? `HandleAnimal` can process *any* Animal, so certainly a Dog.

```
Delegate expects:  Action<Dog>    (narrow)
Method accepts:    Animal         (broad)
Direction:         broad ← narrow  ✅ safe
```

The wrong direction fails:

```csharp
void HandleDog(Dog d) => Console.WriteLine(d.Breed);

Action<Animal> animalHandler = HandleDog; // ❌ compile error
// A Cat is also an Animal — HandleDog would crash on a Cat
```

---

## Return Type Compatibility — Covariance

> *"If your delegate returns a broad type, a method returning something more specific fits fine."*

A method with a **more derived (narrower) return type** is compatible with a delegate that returns a **less derived (broader) type**:

```csharp
class Animal { }
class Dog : Animal { }

Dog GetDog() => new Dog();

Func<Animal> getAnimal = GetDog; // ✅ covariance

Animal a = getAnimal(); // caller gets Animal — it's actually a Dog, that's fine
```

Why safe? The caller expects an `Animal` — getting a `Dog` back is even better.

```
Delegate returns:  Animal   (broad)
Method returns:    Dog      (narrow)
Direction:         narrow → broad  ✅ safe
```

The wrong direction fails:

```csharp
Animal GetAnimal() => new Animal();

Func<Dog> getDog = GetAnimal; // ❌ compile error
// Caller expects a Dog — might get just an Animal — unsafe
```

---

## Both Together — The Full Picture

```csharp
class Animal { }
class Dog : Animal { }

// Parameter: delegate expects Dog,    method takes Animal  → ✅ contravariant
// Return:    delegate returns Animal, method returns Animal → ✅ exact
Animal HandleAndReturn(Dog d) => new Animal();
Func<Dog, Animal> f1 = HandleAndReturn; // ✅

// Parameter: delegate expects Dog,  method takes Animal → ✅ contravariant
// Return:    delegate wants Animal, method returns Dog  → ✅ covariant
Animal HandleBroad(Animal a) => new Dog();
Func<Dog, Animal> f2 = HandleBroad; // ✅

// Memory trick:
// Parameters flow AGAINST the hierarchy  (contra = against)
// Return types flow WITH the hierarchy   (co = with)
```

---

## Generic Delegate Variance — `in` and `out`

When a generic delegate is declared, mark type parameters to unlock variance across the entire type system:

```csharp
// out T = covariant   — T only appears in RETURN positions
delegate TResult Func<out TResult>();

// in T = contravariant — T only appears in PARAMETER positions
delegate void Action<in T>(T arg);
```

This makes the *assignment* rules apply at the delegate *type* level:

```csharp
// Covariance with Func
Func<Dog>    getDog    = () => new Dog();
Func<Animal> getAnimal = getDog; // ✅ Func<Dog> → Func<Animal>

// Contravariance with Action
Action<Animal> handleAnimal = a => Console.WriteLine(a.Name);
Action<Dog>    handleDog    = handleAnimal; // ✅ Action<Animal> → Action<Dog>
```

---

## Declaring Your Own Variant Generic Delegate

```csharp
// COVARIANT — T only in return (output) position
delegate T Producer<out T>();

// CONTRAVARIANT — T only in parameter (input) position
delegate void Consumer<in T>(T input);

// INVARIANT — T in both positions, no modifier allowed
delegate T Transformer<T>(T input);

// Usage
Producer<Dog>    dogProducer    = () => new Dog();
Producer<Animal> animalProducer = dogProducer; // ✅ covariance

Consumer<Animal> animalConsumer = a => Console.WriteLine(a.Name);
Consumer<Dog>    dogConsumer    = animalConsumer; // ✅ contravariance
```

---

## Why `List<T>` Can't Be Covariant

```csharp
// If List<T> were covariant (pretend):
List<Dog>    dogs    = new();
List<Animal> animals = dogs; // imagine this worked...

animals.Add(new Cat()); // CAT going into a List<Dog>! 💥 type safety broken

// List<T> is INVARIANT — no in/out allowed because T is used in BOTH positions
// (Add takes T as input, indexer returns T as output)

// But IEnumerable<out T> IS covariant — read-only, T only in output position
IEnumerable<Dog>    dogEnum    = new List<Dog>();
IEnumerable<Animal> animalEnum = dogEnum; // ✅ safe
```

---

## Real-World Payoff

```csharp
// Works because IEnumerable<out T> is covariant
List<Dog> dogs = new() { new Dog(), new Dog() };

void PrintAll(IEnumerable<Animal> animals)
{
    foreach (var a in animals)
        Console.WriteLine(a.Name);
}

PrintAll(dogs); // ✅ no cast needed

// Works because Action<in T> is contravariant
void LogAnimal(Animal a) => Console.WriteLine(a.Name);

Action<Dog> processDog = LogAnimal; // ✅ no cast needed
processDog(new Dog());
```

---

## Memory Trick

```
co     = "with"     → return flows OUT  → output position  → covariant   (out)
contra = "against"  → input  flows IN   → input  position  → contravariant (in)

Producer<out T>  — T comes OUT  → covariant
Consumer<in T>   — T goes  IN   → contravariant
```

---

## Summary

| Rule | Direction | Keyword | Applies To |
|------|-----------|---------|------------|
| Covariance | Narrow → broad | `out` | Return types |
| Contravariance | Broad → narrow | `in` | Parameter types |
| Invariance | Exact match only | *(none)* | Both positions |
| Structural compat | Same signature | *(none)* | Method → delegate |
| Type compat | Same name required | *(none)* | Delegate → delegate |

---

*Previous: [11 - Delegate vs Interface](./11%20-%20Delegate%20vs%20Interface.md)*
*Next: See [toExplore.md](./toExplore.md) for what to tackle next!*
