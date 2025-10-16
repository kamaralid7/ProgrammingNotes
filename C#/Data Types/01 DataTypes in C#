In **C#**, data types define the kind of data a variable can hold — such as numbers, text, or true/false values.

They fall into **two main categories**:

1. **Value Types**
2. **Reference Types**

---

## 🧮 1. Value Types

Value types directly contain their data and are stored in **stack memory**.

### 🔹 Common Value Types

| Category           | Type      | Example                          | Description                        |
| ------------------ | --------- | -------------------------------- | ---------------------------------- |
| **Integral**       | `byte`    | `byte a = 10;`                   | 8-bit unsigned integer (0–255)     |
|                    | `sbyte`   | `sbyte b = -10;`                 | 8-bit signed integer (–128 to 127) |
|                    | `short`   | `short c = 30000;`               | 16-bit signed integer              |
|                    | `ushort`  | `ushort d = 65000;`              | 16-bit unsigned integer            |
|                    | `int`     | `int e = 1000;`                  | 32-bit signed integer              |
|                    | `uint`    | `uint f = 1000U;`                | 32-bit unsigned integer            |
|                    | `long`    | `long g = 100000L;`              | 64-bit signed integer              |
|                    | `ulong`   | `ulong h = 100000UL;`            | 64-bit unsigned integer            |
| **Floating-point** | `float`   | `float i = 3.14F;`               | 32-bit single precision            |
|                    | `double`  | `double j = 3.14159;`            | 64-bit double precision            |
|                    | `decimal` | `decimal k = 100.5M;`            | 128-bit high-precision decimal     |
| **Character**      | `char`    | `char l = 'A';`                  | 16-bit Unicode character           |
| **Boolean**        | `bool`    | `bool m = true;`                 | True or False                      |
| **Structs**        | `struct`  | `struct Point { int x; int y; }` | User-defined value type            |
| **Enum**           | `enum`    | `enum Days { Sun, Mon, Tue }`    | Named constants                    |

---

## 🧠 2. Reference Types

Reference types store a **reference (memory address)** to the data, which is kept in the **heap memory**.

### 🔹 Common Reference Types

| Type            | Example                       | Description                        |
| --------------- | ----------------------------- | ---------------------------------- |
| **`string`**    | `string name = "Alice";`      | Sequence of Unicode characters     |
| **`object`**    | `object obj = 42;`            | Base type for all types in C#      |
| **`class`**     | `class Person { ... }`        | User-defined blueprint for objects |
| **`interface`** | `interface IShape { ... }`    | Contract for classes               |
| **`delegate`**  | `delegate void MyDelegate();` | Reference to a method              |
| **`array`**     | `int[] nums = {1,2,3};`       | Collection of same-type elements   |
| **`dynamic`**   | `dynamic d = "Hello";`        | Type resolved at runtime           |

---

## 🧩 3. Nullable Types

Allows value types to hold `null`:

```csharp
int? age = null;
bool? isAvailable = null;
```

---

## 🧾 Example:

```csharp
using System;

class Program
{
    static void Main()
    {
        int age = 25;
        double salary = 45000.75;
        string name = "John";
        bool isEmployed = true;
        
        Console.WriteLine($"Name: {name}, Age: {age}, Salary: {salary}, Employed: {isEmployed}");
    }
}
```