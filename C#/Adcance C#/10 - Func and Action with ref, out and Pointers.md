# Advanced C# — 10: Func and Action with ref, out, and Pointers

> *"Func and Action are powerful — but they have limits. Here's where the walls are, and how to break through them."*

---

## The Hard Limit

`Func` and `Action` cannot represent `ref`, `out`, `in`, or pointer parameters. Generic type parameters cannot carry those modifiers.

```csharp
Func<ref int, int>    refFunc   = ...; // ❌ compile error
Func<out string, int> outFunc   = ...; // ❌ compile error
Action<out bool>      outAction = ...; // ❌ compile error
```

---

## Solution: Custom Delegates

For `ref` and `out`, define your own delegate:

```csharp
// Custom delegate with ref parameter
delegate void Modifier(ref int value);

// Custom delegate with out parameter
delegate bool TryParser<T>(string input, out T result);

// ref — mutates caller's variable
Modifier doubleIt = (ref int x) => x *= 2;
int num = 5;
doubleIt(ref num);
Console.WriteLine(num); // 10

// out — must assign before returning
TryParser<int>      parseInt  = int.TryParse;
TryParser<DateTime> parseDate = DateTime.TryParse;

if (parseInt("42", out int result))
    Console.WriteLine(result); // 42

if (parseDate("2024-01-15", out DateTime date))
    Console.WriteLine(date.Year); // 2024
```

---

## ref Parameters — Mutating in Place

The delegate reads AND writes the caller's variable directly:

```csharp
delegate void Transformer(ref int value);

Transformer square = (ref int x) => x = x * x;
Transformer negate = (ref int x) => x = -x;

int value = 4;
square(ref value); // value = 16
negate(ref value); // value = -16
Console.WriteLine(value); // -16

// Multicast works too
Transformer chain = square;
chain += negate;

int n = 3;
chain(ref n); // square → 9, then negate → -9
Console.WriteLine(n); // -9
```

---

## out Parameters — The TryXxx Pattern

Build a generic, reusable parse helper using a custom `out` delegate:

```csharp
delegate bool TryConvert<TIn, TOut>(TIn input, out TOut result);

TryConvert<string, int>  tryInt  = int.TryParse;
TryConvert<string, Guid> tryGuid = Guid.TryParse;
TryConvert<string, bool> tryBool = bool.TryParse;

// Generic helper that uses any TryConvert
static TOut ParseOrDefault<TIn, TOut>(
    TIn input,
    TryConvert<TIn, TOut> parser,
    TOut fallback = default)
{
    return parser(input, out TOut result) ? result : fallback;
}

Console.WriteLine(ParseOrDefault("123",  tryInt,  -1));    // 123
Console.WriteLine(ParseOrDefault("abc",  tryInt,  -1));    // -1
Console.WriteLine(ParseOrDefault("true", tryBool, false)); // True
```

---

## ref return — Returning a Reference

Return a reference to a memory location, not a copy:

```csharp
delegate ref int RefSelector(int[] array, int index);

RefSelector getRef = (arr, i) => ref arr[i];

int[] numbers = { 10, 20, 30, 40 };

ref int element = ref getRef(numbers, 2);
element = 999; // modifies the array directly!
Console.WriteLine(numbers[2]); // 999
```

---

## Pointers — Unsafe Code

Pointer parameters require `unsafe` context and a custom delegate:

```csharp
unsafe delegate void PointerOp(int* ptr, int length);

unsafe
{
    PointerOp doubleAll = (ptr, len) =>
    {
        for (int i = 0; i < len; i++)
            ptr[i] *= 2;
    };

    int[] data = { 1, 2, 3, 4, 5 };
    fixed (int* p = data)
    {
        doubleAll(p, data.Length);
    }

    Console.WriteLine(string.Join(", ", data)); // 2, 4, 6, 8, 10
}
```

---

## Modern Alternative — Span\<T\> Instead of Pointers

`Span<T>` gives pointer-level performance without `unsafe`, and works with normal delegates:

```csharp
delegate void SpanOp<T>(Span<T> buffer);

SpanOp<int> doubleAll = span =>
{
    for (int i = 0; i < span.Length; i++)
        span[i] *= 2;
};

SpanOp<int> fillZero = span => span.Fill(0);

int[] data = { 1, 2, 3, 4, 5 };
doubleAll(data); // implicit Span<int> from array
Console.WriteLine(string.Join(", ", data)); // 2, 4, 6, 8, 10
```

No `unsafe`. No `fixed`. No raw pointers. Prefer this in modern C#.

---

## Workaround — Wrap in a Reference Type

When you must use `Func`/`Action` but still need mutation, wrap the value:

```csharp
class Ref<T> { public T Value { get; set; } }

Action<Ref<int>> doubler  = r => r.Value *= 2;
Func<Ref<int>, int> square = r => r.Value * r.Value;

var x = new Ref<int> { Value = 5 };
doubler(x);
Console.WriteLine(x.Value);  // 10
Console.WriteLine(square(x)); // 100
```

---

## Comparison Table

| Modifier | `Func`/`Action` | Custom Delegate | Notes |
|----------|----------------|-----------------|-------|
| `ref` | ❌ | ✅ | Mutates caller's variable |
| `out` | ❌ | ✅ | Must assign before return |
| `in` | ❌ | ✅ | Read-only ref (perf optimization) |
| `ref return` | ❌ | ✅ | Returns reference to memory location |
| Pointer `*` | ❌ | ✅ (`unsafe`) | Needs unsafe context |
| `Span<T>` | ✅ | ✅ | Modern safe alternative to pointers |
| Class wrapper | ✅ | ✅ | Mutation via reference type |

---

*Previous: [09 - Func and Action Delegates](./09%20-%20Func%20and%20Action%20Delegates.md)*
*Next: [11 - Delegate vs Interface](./11%20-%20Delegate%20vs%20Interface.md)*
