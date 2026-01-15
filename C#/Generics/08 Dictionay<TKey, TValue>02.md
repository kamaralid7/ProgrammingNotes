### Introduction to Dictionary<TKey, TValue>

Hello Sweety! As your C# fundamentals teacher, let's dive into the `Dictionary<TKey, TValue>` class from the `System.Collections.Generic` namespace in .NET Core. This is a powerful collection that stores key-value pairs, like a real-world dictionary where keys are unique words and values are their meanings. Keys must be unique (no duplicates allowed), and it provides fast lookups, additions, and removals based on the key—typically O(1) time complexity thanks to hashing.

It's generic, so you specify types for the key (TKey) and value (TValue), e.g., `Dictionary<string, int>` for names and ages. Best practice: Choose a key type with a good `GetHashCode()` implementation (like strings or ints) to avoid performance issues. Always handle cases where keys might not exist to prevent `KeyNotFoundException`.

Now, let's cover all the public methods of `Dictionary<TKey, TValue>`. I'll describe each one in simple words, explain what it does, when to use it, and include a short example. These are based on .NET 8.0, but most work in earlier .NET Core versions too. Note: Some methods like `GetObjectData` are obsolete—avoid them in new code and use modern serialization instead.

### Add(TKey key, TValue value)
This method adds a new key-value pair to the dictionary. If the key already exists, it throws a `ArgumentException`. 

Use it when you're sure the key is new. Best practice: Check with `ContainsKey` first or use `TryAdd` (below) to avoid exceptions.

Example:
```csharp
var dict = new Dictionary<string, int>();
dict.Add("apple", 5);  // Adds key "apple" with value 5
// dict.Add("apple", 10);  // Throws exception if key exists
```

### Clear()
Removes all key-value pairs from the dictionary, making it empty. It doesn't change the capacity (internal storage size).

Use it to reset the dictionary without creating a new one. Best practice: Call this instead of re-instantiating for better performance in loops.

Example:
```csharp
var dict = new Dictionary<string, int> { {"apple", 5}, {"banana", 3} };
dict.Clear();  // Now dict.Count == 0
```

### ContainsKey(TKey key)
Checks if a specific key exists in the dictionary. Returns `true` if it does, `false` otherwise.

Use it before accessing or adding to avoid errors. Best practice: Pair it with indexers for safe lookups.

Example:
```csharp
var dict = new Dictionary<string, int> { {"apple", 5} };
bool hasApple = dict.ContainsKey("apple");  // true
bool hasOrange = dict.ContainsKey("orange");  // false
```

### ContainsValue(TValue value)
Checks if a specific value exists in the dictionary (searches all values). Returns `true` if found, `false` otherwise.

Use it sparingly—it's O(n) time since it scans all values. Best practice: If you need frequent value checks, consider a different data structure like a HashSet for values.

Example:
```csharp
var dict = new Dictionary<string, int> { {"apple", 5}, {"banana", 5} };
bool hasFive = dict.ContainsValue(5);  // true
bool hasTen = dict.ContainsValue(10);  // false
```

### EnsureCapacity(int capacity)
Sets the dictionary's internal storage to hold at least the specified number of items without resizing. Returns the new capacity.

Use it for performance when you know how many items you'll add upfront. Available in .NET 5+. Best practice: Call this before bulk additions to minimize reallocations.

Example:
```csharp
var dict = new Dictionary<string, int>();
int newCapacity = dict.EnsureCapacity(100);  // Prepares for 100 items
```

### GetEnumerator()
Returns an enumerator (like a cursor) to loop through all key-value pairs. Implements `IEnumerable<KeyValuePair<TKey, TValue>>`.

Use it for foreach loops. Best practice: Prefer foreach over manual indexing for collections.

Example:
```csharp
var dict = new Dictionary<string, int> { {"apple", 5}, {"banana", 3} };
foreach (var pair in dict) {
    Console.WriteLine($"{pair.Key}: {pair.Value}");
}
// Output: apple: 5 \n banana: 3 (order not guaranteed)
```

### GetObjectData(SerializationInfo info, StreamingContext context)
Obsolete method for serializing the dictionary (saving to a file/stream). Don't use in new code.

Best practice: Use `System.Text.Json` or `Newtonsoft.Json` for serialization instead.

No example needed—avoid it.

### OnDeserialization(object sender)
Internal method called after deserialization. Implements ISerializable. Not typically called directly.

Best practice: Ignore unless dealing with legacy serialization.

No example needed.

### Remove(TKey key)
Removes the key-value pair with the specified key if it exists. Returns `true` if removed, `false` if key not found.

Use it for safe removal without needing the value. Best practice: Check return value to know if anything was removed.

Example:
```csharp
var dict = new Dictionary<string, int> { {"apple", 5} };
bool removed = dict.Remove("apple");  // true, dict now empty
bool removedAgain = dict.Remove("apple");  // false
```

### Remove(TKey key, out TValue value)
Removes the key-value pair if the key exists and copies the removed value to the out parameter. Returns `true` if removed, `false` otherwise.

Use it when you need the value being removed. Available in .NET Core 3.0+. Best practice: Great for cleanup scenarios.

Example:
```csharp
var dict = new Dictionary<string, int> { {"apple", 5} };
if (dict.Remove("apple", out int removedValue)) {
    Console.WriteLine(removedValue);  // 5
}
```

### TrimExcess()
Reduces the dictionary's internal storage to fit exactly the current number of items (calls TrimExcess(Count)).

Use it to free memory after removing many items. Best practice: Call after bulk removals if memory is a concern.

Example:
```csharp
var dict = new Dictionary<string, int>();  // Assume added/removed items
dict.TrimExcess();  // Optimizes storage
```

### TrimExcess(int capacity)
Sets the internal storage to hold at least the specified capacity, trimming if possible.

Use it to optimize memory for a known size. Available in .NET 5+. Best practice: Similar to EnsureCapacity, but for shrinking.

Example:
```csharp
var dict = new Dictionary<string, int>();
dict.TrimExcess(50);  // Sets capacity around 50
```

### TryAdd(TKey key, TValue value)
Tries to add the key-value pair. Returns `true` if added (key was new), `false` if key exists (no change).

Use it instead of Add for exception-free addition. Available in .NET Core 2.0+. Best practice: Always prefer this for robustness.

Example:
```csharp
var dict = new Dictionary<string, int> { {"apple", 5} };
bool added = dict.TryAdd("banana", 3);  // true
bool addedAgain = dict.TryAdd("apple", 10);  // false, value unchanged
```

### TryGetValue(TKey key, out TValue value)
Tries to get the value for a key. Returns `true` if key exists (value set), `false` otherwise (value set to default).

Use it for safe lookups without exceptions. Best practice: Always use this over direct `dict[key]` access.

Example:
```csharp
var dict = new Dictionary<string, int> { {"apple", 5} };
if (dict.TryGetValue("apple", out int count)) {
    Console.WriteLine(count);  // 5
} else {
    Console.WriteLine("Not found");
}
```

That's all the methods! Practice by creating a small console app with these—start with adding, checking, and removing. Remember, dictionaries aren't ordered; if you need order, use `SortedDictionary`. Let me know if you want examples with best practices or exercises!