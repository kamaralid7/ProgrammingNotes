Below is a **complete, learning-oriented description of every important method and property of `HashSet<T>` in C#**, grouped by purpose so it’s easier to digest and **not overwhelming**.

Think of `HashSet<T>` as:

> **A fast collection for unique items with powerful set operations**

---

# 🔹 What is `HashSet<T>`?

* Stores **unique elements**
* **No order guaranteed**
* **Very fast** lookups (O(1) average)
* Ideal for **set mathematics** (union, intersection, difference)

---

# 1️⃣ Adding & Removing Elements

### `Add(T item)`

Adds an item to the set.

* Returns `true` if added
* Returns `false` if item already exists

```csharp
set.Add(10);
```

---

### `Remove(T item)`

Removes an item if it exists.

* Returns `true` if removed

```csharp
set.Remove(10);
```

---

### `RemoveWhere(Predicate<T> match)`

Removes all elements matching a condition.

```csharp
set.RemoveWhere(x => x > 50);
```

---

### `Clear()`

Removes **all elements** from the set.

```csharp
set.Clear();
```

---

# 2️⃣ Lookup & Checking

### `Contains(T item)`

Checks if the item exists.

```csharp
set.Contains(5);
```

⏱️ **O(1)** lookup

---

### `TryGetValue(T equalValue, out T actualValue)`

Gets the actual stored value equal to the given one.

Useful when objects have same hash but different references.

```csharp
set.TryGetValue(search, out var actual);
```

---

# 3️⃣ Count & Capacity

### `Count`

Number of elements in the set.

```csharp
int total = set.Count;
```

---

### `EnsureCapacity(int capacity)`

Pre-allocates memory to avoid resizing.

```csharp
set.EnsureCapacity(1000);
```

---

### `TrimExcess()`

Shrinks internal memory to fit actual size.

```csharp
set.TrimExcess();
```

---

# 4️⃣ Copying & Enumeration

### `CopyTo(T[] array)`

Copies elements into an array.

```csharp
set.CopyTo(arr);
```

---

### `CopyTo(T[] array, int arrayIndex)`

Starts copying at a specific index.

---

### `CopyTo(T[] array, int arrayIndex, int count)`

Copies a limited number of items.

---

### `GetEnumerator()`

Returns an enumerator (used in `foreach`).

```csharp
foreach (var item in set) { }
```

---

# 5️⃣ Set Operations (🔥 Most Powerful Part)

### `UnionWith(IEnumerable<T> other)`

Adds all elements from another collection.

```csharp
set.UnionWith(otherSet);
```

➡️ **A ∪ B**

---

### `IntersectWith(IEnumerable<T> other)`

Keeps only common elements.

```csharp
set.IntersectWith(otherSet);
```

➡️ **A ∩ B**

---

### `ExceptWith(IEnumerable<T> other)`

Removes elements present in the other set.

```csharp
set.ExceptWith(otherSet);
```

➡️ **A − B**

---

### `SymmetricExceptWith(IEnumerable<T> other)`

Keeps elements in either set but not both.

```csharp
set.SymmetricExceptWith(otherSet);
```

➡️ **(A ∪ B) − (A ∩ B)**

---

# 6️⃣ Relationship Checks (Very Important)

### `IsSubsetOf(IEnumerable<T> other)`

Checks if current set ⊆ other.

---

### `IsSupersetOf(IEnumerable<T> other)`

Checks if current set ⊇ other.

---

### `IsProperSubsetOf(IEnumerable<T> other)`

Subset but **not equal**.

---

### `IsProperSupersetOf(IEnumerable<T> other)`

Superset but **not equal**.

---

### `Overlaps(IEnumerable<T> other)`

Checks if **any common element exists**.

---

### `SetEquals(IEnumerable<T> other)`

Checks if both sets contain **exactly the same elements**.

---

# 7️⃣ Equality Comparer

### `Comparer`

Returns the equality comparer used by the set.

```csharp
var comparer = set.Comparer;
```

Useful when working with **custom objects**.

---

# 8️⃣ Interfaces Implemented (Why it’s powerful)

`HashSet<T>` implements:

* `ICollection<T>`
* `ISet<T>`
* `IEnumerable<T>`
* `IReadOnlyCollection<T>`

➡️ Works seamlessly with LINQ, APIs, and frameworks.

---

# ⚡ When to Use `HashSet<T>`

✅ Remove duplicates
✅ Fast lookups
✅ Compare collections
✅ Permissions / roles / feature flags
✅ Mathematical set logic

❌ Not for ordered data
❌ Not for indexed access

---

# 🧠 Mental Model (Remember This)

> **List = order matters**
> **Dictionary = key → value**
> **HashSet = uniqueness + speed + set logic**

---

If you want next:

* 🔥 **Real-world examples**
* 🔥 **HashSet vs List vs Dictionary**
* 🔥 **Custom object equality (`Equals` & `GetHashCode`)**
* 🔥 **Performance internals**

Just tell me 👍
