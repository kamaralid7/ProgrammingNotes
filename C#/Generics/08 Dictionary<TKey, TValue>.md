Great idea 👍
`Dictionary<TKey, TValue>` **looks simple**, but mastering *every method* will make you very strong in C# collections, performance, and real-world coding.

Below is a **complete, structured, practical guide** to **ALL important members** of `Dictionary<TKey, TValue>` in **.NET**, explained simply.

---

# 🔹 Dictionary<TKey, TValue> – Complete Method Guide

A `Dictionary<TKey, TValue>` stores **key–value pairs** where:

* **Keys are unique**
* Lookup is **O(1)** average (hash table)

```csharp
Dictionary<int, string> dict = new Dictionary<int, string>();
```

---

## 1️⃣ ADDING DATA

### 🔹 `Add(TKey key, TValue value)`

Adds a new key-value pair.

```csharp
dict.Add(1, "Apple");
```

✅ Throws **exception** if key already exists.

Use when:

* Duplicate keys are **not allowed**
* You want strict behavior

---

### 🔹 `TryAdd(TKey key, TValue value)` (Recommended)

Adds safely without exception.

```csharp
bool added = dict.TryAdd(2, "Banana");
```

* Returns `true` if added
* Returns `false` if key exists

✅ Best for **safe code**

---

## 2️⃣ ACCESSING DATA

### 🔹 Indexer `dict[key]`

Get or set value.

```csharp
string value = dict[1];
dict[1] = "Green Apple";
```

⚠️ Throws exception if key not found while reading.

---

### 🔹 `TryGetValue(TKey key, out TValue value)` ⭐ MOST IMPORTANT

Safely gets value.

```csharp
if (dict.TryGetValue(1, out string fruit))
{
    Console.WriteLine(fruit);
}
```

✅ No exception
✅ Fast
✅ Production-grade

---

### 🔹 `ContainsKey(TKey key)`

Checks if key exists.

```csharp
bool exists = dict.ContainsKey(1);
```

⚠️ Often **worse** than `TryGetValue` because it does 2 lookups.

---

### 🔹 `ContainsValue(TValue value)`

Checks if value exists.

```csharp
bool hasApple = dict.ContainsValue("Apple");
```

❌ Slow (O(n)) – scans all values

---

## 3️⃣ REMOVING DATA

### 🔹 `Remove(TKey key)`

Removes by key.

```csharp
dict.Remove(1);
```

Returns `true` if removed.

---

### 🔹 `Remove(TKey key, out TValue value)` (.NET Core+)

Removes and gives value.

```csharp
dict.Remove(1, out string removedValue);
```

✅ Useful when you need the deleted value

---

### 🔹 `Clear()`

Removes everything.

```csharp
dict.Clear();
```

---

## 4️⃣ COUNT & STATE

### 🔹 `Count`

Total elements.

```csharp
int total = dict.Count;
```

---

### 🔹 `Comparer`

Returns the equality comparer used.

```csharp
var comparer = dict.Comparer;
```

Example use:

* Case-insensitive keys

```csharp
new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
```

---

## 5️⃣ ITERATION (VERY IMPORTANT)

### 🔹 `foreach` (KeyValuePair)

Default iteration.

```csharp
foreach (var kvp in dict)
{
    Console.WriteLine($"{kvp.Key} : {kvp.Value}");
}
```

---

### 🔹 `Keys`

Returns collection of keys.

```csharp
foreach (var key in dict.Keys)
{
    Console.WriteLine(key);
}
```

⚠️ **Live view** – reflects dictionary changes

---

### 🔹 `Values`

Returns collection of values.

```csharp
foreach (var value in dict.Values)
{
    Console.WriteLine(value);
}
```

---

## 6️⃣ COPYING DATA

### 🔹 `ToList()`

Convert to list.

```csharp
var list = dict.ToList();
```

---

### 🔹 `ToArray()`

Convert to array.

```csharp
var array = dict.ToArray();
```

---

### 🔹 `CopyTo(KeyValuePair<TKey, TValue>[] array, int index)`

Low-level copy.

```csharp
var arr = new KeyValuePair<int, string>[dict.Count];
dict.CopyTo(arr, 0);
```

---

## 7️⃣ UPDATE / UPSERT PATTERNS

### 🔹 Overwrite using indexer

```csharp
dict[key] = "New Value";
```

✔ Adds if not exists
✔ Updates if exists

---

### 🔹 Update only if exists

```csharp
if (dict.ContainsKey(key))
{
    dict[key] = "Updated";
}
```

---

## 8️⃣ THREAD SAFETY (IMPORTANT)

`Dictionary<TKey, TValue>` ❌ is **NOT thread-safe**

For multi-threading use:

```csharp
ConcurrentDictionary<TKey, TValue>
```

---

## 9️⃣ COMMON EXCEPTIONS

| Situation              | Exception             |
| ---------------------- | --------------------- |
| Duplicate key in Add   | ArgumentException     |
| Missing key in indexer | KeyNotFoundException  |
| Null key               | ArgumentNullException |

---

## 🔟 PERFORMANCE SUMMARY

| Operation     | Complexity |
| ------------- | ---------- |
| Add           | O(1)       |
| Remove        | O(1)       |
| Lookup        | O(1)       |
| ContainsValue | O(n)       |

---

## 🔹 REAL-WORLD BEST PRACTICES ⭐

✔ Prefer `TryGetValue` over `ContainsKey + indexer`
✔ Prefer `TryAdd` over `Add`
✔ Avoid `ContainsValue` in large dictionaries
✔ Use proper `IEqualityComparer`
✔ Use `ConcurrentDictionary` for multithreading

---

## 🧠 MENTAL MODEL (IMPORTANT)

Think of `Dictionary<TKey, TValue>` as:

> **Hash Table → Buckets → Fast Lookup → Unique Keys**

---

## 📌 NEXT STEP (Highly Recommended)

To truly master it, next learn:

1. `ConcurrentDictionary<TKey, TValue>`
2. `IReadOnlyDictionary<TKey, TValue>`
3. Difference between `Dictionary` vs `Hashtable`
4. Custom `IEqualityComparer<TKey>`


* Give **practice problems**
* Explain **internal working (hashing & buckets)**
* Compare **Dictionary vs IEnumerable vs IQueryable**
* Explain **Span<T> relation with performance**

