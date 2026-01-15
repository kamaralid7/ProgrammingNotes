Great choice 👍
`Queue<T>` is **small but very important**, especially for **BFS, scheduling, producer–consumer**, and async pipelines.

I’ll go **method by method**, with **clear intent + simple examples**, so it doesn’t feel overwhelming.

---

# Queue<T> in C# — Complete Method Guide

**Namespace:** `System.Collections.Generic`
**Concept:** **FIFO** → *First In, First Out*

Think of a **line at a ticket counter** 🎟️
First person in → first person out.

---

## 1. `Enqueue(T item)`

**Purpose:** Add an item to the **end** of the queue.

```csharp
Queue<int> q = new Queue<int>();
q.Enqueue(10);
q.Enqueue(20);
q.Enqueue(30);
```

**Queue now:** `10 → 20 → 30`

🔹 Time Complexity: **O(1)**

---

## 2. `Dequeue()`

**Purpose:** Remove **and return** the item from the **front**.

```csharp
int first = q.Dequeue(); // 10
```

**Queue now:** `20 → 30`

⚠️ Throws **InvalidOperationException** if queue is empty.

🔹 Time Complexity: **O(1)**

---

## 3. `Peek()`

**Purpose:** Return the **front item** without removing it.

```csharp
int next = q.Peek(); // 20
```

**Queue unchanged:** `20 → 30`

⚠️ Throws exception if empty.

🔹 Time Complexity: **O(1)**

---

## 4. `TryDequeue(out T result)`

**Purpose:** Safely remove the front item **without exceptions**.

```csharp
if (q.TryDequeue(out int value))
{
    Console.WriteLine(value);
}
```

✔️ Returns `true` if successful
❌ Returns `false` if queue is empty

✅ **Preferred in production code**

---

## 5. `TryPeek(out T result)`

**Purpose:** Safely look at the front item **without removing it**.

```csharp
if (q.TryPeek(out int value))
{
    Console.WriteLine(value);
}
```

✔️ No exception
✔️ Queue unchanged

---

## 6. `Contains(T item)`

**Purpose:** Check if an item exists in the queue.

```csharp
bool exists = q.Contains(30);
```

⚠️ Internally loops → **O(n)**

Use carefully for large queues.

---

## 7. `Clear()`

**Purpose:** Remove **all items** from the queue.

```csharp
q.Clear();
```

Queue becomes empty.

---

## 8. `Count`

**Purpose:** Get number of elements in the queue.

```csharp
int total = q.Count;
```

✔️ Property, not a method
✔️ Very fast

---

## 9. `ToArray()`

**Purpose:** Copy queue elements into an array (FIFO order).

```csharp
int[] arr = q.ToArray();
```

Queue remains unchanged.

---

## 10. `GetEnumerator()`

**Purpose:** Enables `foreach` iteration.

```csharp
foreach (var item in q)
{
    Console.WriteLine(item);
}
```

⚠️ Enumeration is **read-only**

---

## 11. Constructors

### a) Empty Queue

```csharp
Queue<string> q = new Queue<string>();
```

### b) With Initial Capacity

```csharp
Queue<int> q = new Queue<int>(100);
```

⚡ Avoids resizing → performance benefit

### c) From Collection

```csharp
Queue<int> q = new Queue<int>(new[] { 1, 2, 3 });
```

---

## 12. `TrimExcess()`

**Purpose:** Reduce memory usage by trimming unused capacity.

```csharp
q.TrimExcess();
```

Use after **large dequeue operations**.

---

## Complete Method Summary Table

| Method          | Purpose               |
| --------------- | --------------------- |
| `Enqueue`       | Add item to end       |
| `Dequeue`       | Remove + return front |
| `Peek`          | View front            |
| `TryDequeue`    | Safe dequeue          |
| `TryPeek`       | Safe peek             |
| `Contains`      | Search item           |
| `Clear`         | Remove all            |
| `Count`         | Item count            |
| `ToArray`       | Copy to array         |
| `TrimExcess`    | Reduce memory         |
| `GetEnumerator` | `foreach` support     |

---

## When to Use Queue<T>

✔️ Task scheduling
✔️ Breadth-First Search (BFS)
✔️ Producer–Consumer
✔️ Message processing
✔️ Async pipelines

---

## Mental Model (Very Important)

```
Enqueue → → → →
           Queue
← ← ← ← Dequeue
```

Front = **oldest item**

---

next we can:

1. Compare **Queue vs Stack**
2. Implement **real-world examples** (job scheduler)
3. See **Queue<T> vs ConcurrentQueue<T>**
4. Practice **10 interview-style questions**

