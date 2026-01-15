Great choice 👍
`Stack<T>` is **simple but powerful**, and learning *every method with intent* will remove that “overwhelmed” feeling you mentioned earlier.

Below is a **complete, learning-oriented walkthrough of ALL `Stack<T>` methods**, grouped by **purpose**, with **what + why + example**.

---

# 📦 Stack<T> in C#

> **LIFO** collection → **Last In, First Out**
> Think: **plates stacked on top of each other** 🍽️

---

## 1️⃣ Core Stack Operations (Most Important)

These define how a stack behaves.

---

### 🔹 `Push(T item)`

**What it does:**
Adds an item **to the top** of the stack.

**Why it exists:**
Stack grows only from the top.

```csharp
Stack<int> stack = new Stack<int>();
stack.Push(10);
stack.Push(20);
```

📌 Stack now: `20 (top), 10`

---

### 🔹 `Pop()`

**What it does:**
Removes **and returns** the top item.

**Why it exists:**
LIFO behavior — last added is removed first.

```csharp
int value = stack.Pop(); // returns 20
```

⚠️ Throws `InvalidOperationException` if stack is empty.

---

### 🔹 `Peek()`

**What it does:**
Returns the top item **without removing it**.

**Why it exists:**
Sometimes you need to *look*, not *take*.

```csharp
int top = stack.Peek(); // returns 10
```

⚠️ Throws exception if empty.

---

## 2️⃣ Safety & Status Methods

---

### 🔹 `Count`

**What it does:**
Returns number of elements.

```csharp
int count = stack.Count;
```

📌 O(1) operation → very fast.

---

### 🔹 `TryPop(out T result)`

**What it does:**
Safely removes top element **without throwing exception**.

```csharp
if (stack.TryPop(out int value))
{
    Console.WriteLine(value);
}
```

✅ Preferred in production code.

---

### 🔹 `TryPeek(out T result)`

**What it does:**
Safely reads top element.

```csharp
if (stack.TryPeek(out int top))
{
    Console.WriteLine(top);
}
```

---

## 3️⃣ Search & Query Methods

---

### 🔹 `Contains(T item)`

**What it does:**
Checks if item exists.

```csharp
bool exists = stack.Contains(10);
```

⏱️ O(n) — linear search
📌 Not sorted, no indexing.

---

## 4️⃣ Conversion & Copy Methods

---

### 🔹 `ToArray()`

**What it does:**
Copies stack to an array.

```csharp
int[] arr = stack.ToArray();
```

⚠️ Order is **LIFO**:

```text
Top → First element in array
```

---

### 🔹 `CopyTo(T[] array, int arrayIndex)`

**What it does:**
Copies elements into an existing array.

```csharp
int[] arr = new int[5];
stack.CopyTo(arr, 1);
```

📌 Useful when integrating with legacy APIs.

---

## 5️⃣ Cleanup & Reset

---

### 🔹 `Clear()`

**What it does:**
Removes **all elements**.

```csharp
stack.Clear();
```

📌 Capacity remains allocated (performance optimization).

---

## 6️⃣ Enumeration (foreach support)

---

### 🔹 `GetEnumerator()`

**What it does:**
Allows iteration using `foreach`.

```csharp
foreach (var item in stack)
{
    Console.WriteLine(item);
}
```

📌 Enumeration order:

```text
Top → Bottom
```

⚠️ Modifying stack during enumeration throws exception.

---

## 7️⃣ Constructors (Important but Ignored Often)

---

### 🔹 `Stack<T>()`

Creates empty stack.

```csharp
var stack = new Stack<string>();
```

---

### 🔹 `Stack<T>(int capacity)`

Pre-allocates memory.

```csharp
var stack = new Stack<int>(1000);
```

📌 Performance optimization when size is known.

---

### 🔹 `Stack<T>(IEnumerable<T> collection)`

Initializes stack from collection.

```csharp
var stack = new Stack<int>(new[] { 1, 2, 3 });
```

📌 Last item of collection becomes **top of stack**.

---

## 8️⃣ Summary Table 🧠

| Method          | Purpose             |
| --------------- | ------------------- |
| `Push`          | Add item            |
| `Pop`           | Remove + return top |
| `Peek`          | Read top            |
| `TryPop`        | Safe Pop            |
| `TryPeek`       | Safe Peek           |
| `Count`         | Number of items     |
| `Contains`      | Search              |
| `ToArray`       | Convert             |
| `CopyTo`        | Copy to array       |
| `Clear`         | Reset               |
| `GetEnumerator` | foreach             |
| Constructors    | Create stack        |

---

## 9️⃣ When SHOULD you use Stack<T>?

✔ Undo / Redo
✔ Expression evaluation
✔ Backtracking (DFS)
✔ Parsing (HTML, brackets, syntax)
✔ Call stack simulation
✔ Navigation history (browser back)

---

## 🔁 Next Best Learning Step (Recommended)

To **lock this into memory**, we can:

1. Build **Undo/Redo system**
2. Compare **Stack<T> vs Queue<T>**
3. Solve **real interview problems**
4. Implement **custom Stack<T>**
5. Understand **internal array resizing**

