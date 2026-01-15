### Introduction to HashSet<T>

Hello Sweety! As we progress in our C# collections adventure with .NET Core, let's tackle `HashSet<T>` from the `System.Collections.Generic` namespace. This is a collection of unique items—no duplicates allowed! It's like a bag where each thing is one-of-a-kind, using hashing for super-fast checks (O(1) time for add, remove, contains). You specify the type T, e.g., `HashSet<string>` for unique names.

HashSets shine in scenarios like removing duplicates from a list, checking memberships quickly, or performing set operations (union, intersection). Best practice: Use when order doesn't matter and uniqueness is key. Choose T with a solid `GetHashCode()` and `Equals()` (like primitives or custom types with overrides). Always handle empty sets gracefully. If you need sorted uniqueness, go for `SortedSet<T>`.

Below, I'll describe all public methods of `HashSet<T>`. Based on .NET 8.0 (compatible with earlier .NET Core), with simple explanations, usage tips, best practices, and examples. Methods are grouped logically for ease.

### Add(T item)
Adds the item if it's not already present. Returns `true` if added, `false` if duplicate.

Use for inserting unique items. Best practice: Check return value to detect duplicates; prefer over List if uniqueness matters.

Example:
```csharp
var set = new HashSet<string>();
bool added = set.Add("apple");  // true, set now has "apple"
bool addedAgain = set.Add("apple");  // false, no change
```

### Clear()
Removes all items, emptying the set.

Use to reset. Best practice: Call instead of recreating for performance in loops.

Example:
```csharp
var set = new HashSet<string> { "apple", "banana" };
set.Clear();  // Now set.Count == 0
```

### Contains(T item)
Checks if the item exists. Returns `true` if yes, `false` no.

Use for quick lookups. O(1) time! Best practice: Ideal for membership tests; faster than List.Contains.

Example:
```csharp
var set = new HashSet<string> { "apple" };
bool hasApple = set.Contains("apple");  // true
bool hasOrange = set.Contains("orange");  // false
```

### CopyTo(T[] array)
Copies all items to the array starting at index 0.

Use for array conversion. Best practice: Ensure array length >= Count to avoid exceptions.

Example:
```csharp
var set = new HashSet<string> { "apple", "banana" };
string[] array = new string[2];
set.CopyTo(array);  // array = ["apple", "banana"] (order not guaranteed)
```

### CopyTo(T[] array, int arrayIndex)
Copies to array from specified index.

Similar to above. Best practice: Validate index and space.

Example:
```csharp
string[] array = new string[3];
set.CopyTo(array, 1);  // array[1] and [2] get items
```

### CopyTo(T[] array, int arrayIndex, int count)
Copies 'count' items starting from arrayIndex.

Use for partial copies. Available in .NET Framework; in Core, ensure count <= Count. Best practice: Handle edge cases like count=0.

Example:
```csharp
set.CopyTo(array, 0, 1);  // Copies first item only
```

### EnsureCapacity(int capacity)
Prepares internal storage for at least 'capacity' items. Returns new capacity. .NET 5+.

Use for performance when knowing size upfront. Best practice: Call before bulk adds to reduce resizes.

Example:
```csharp
var set = new HashSet<string>();
int cap = set.EnsureCapacity(100);  // Ready for 100 items
```

### ExceptWith(IEnumerable<T> other)
Removes items from this set that are in 'other' (set difference).

Use for subtracting sets. Best practice: 'other' can be any enumerable; modifies in-place.

Example:
```csharp
var set1 = new HashSet<string> { "apple", "banana", "cherry" };
var set2 = new HashSet<string> { "banana" };
set1.ExceptWith(set2);  // set1: "apple", "cherry"
```

### GetEnumerator()
Returns enumerator for looping through items. Order not guaranteed.

Use in foreach. Best practice: Don't modify during iteration.

Example:
```csharp
var set = new HashSet<string> { "apple", "banana" };
foreach (var item in set) {
    Console.WriteLine(item);  // apple \n banana (or reverse)
}
```

### IntersectWith(IEnumerable<T> other)
Keeps only items in both this set and 'other' (intersection).

Use for common elements. Best practice: Modifies in-place; efficient for overlaps.

Example:
```csharp
var set1 = new HashSet<string> { "apple", "banana" };
var set2 = new HashSet<string> { "banana", "cherry" };
set1.IntersectWith(set2);  // set1: "banana"
```

### IsProperSubsetOf(IEnumerable<T> other)
Checks if this set is a proper subset of 'other' (all items in other, but stricter—not equal).

Returns bool. Best practice: Use for relationship checks; O(n) time.

Example:
```csharp
var set1 = new HashSet<string> { "apple" };
var set2 = new HashSet<string> { "apple", "banana" };
bool isProper = set1.IsProperSubsetOf(set2);  // true
```

### IsProperSupersetOf(IEnumerable<T> other)
Checks if this set is a proper superset of 'other' (contains all of other, plus more).

Similar to above. Best practice: Symmetric to subset.

Example:
```csharp
bool isProperSuper = set2.IsProperSupersetOf(set1);  // true
```

### IsSubsetOf(IEnumerable<T> other)
Checks if this set is a subset of 'other' (all items in other, can be equal).

Best practice: Allows equality unlike proper.

Example:
```csharp
bool isSubset = set1.IsSubsetOf(set2);  // true
bool equalSubset = set1.IsSubsetOf(set1);  // true
```

### IsSupersetOf(IEnumerable<T> other)
Checks if this set is a superset of 'other' (contains all of other, can be equal).

Best practice: For containment tests.

Example:
```csharp
bool isSuperset = set2.IsSupersetOf(set1);  // true
```

### Overlaps(IEnumerable<T> other)
Checks if there's any common item with 'other'.

Returns bool. Best practice: Quick way to detect intersection without modifying.

Example:
```csharp
bool overlaps = set1.Overlaps(set2);  // true (if sharing items)
```

### Remove(T item)
Removes the item if present. Returns `true` if removed, `false` if not found.

Use for single removals. Best practice: Check return for confirmation.

Example:
```csharp
var set = new HashSet<string> { "apple" };
bool removed = set.Remove("apple");  // true
```

### RemoveWhere(Predicate<T> match)
Removes items matching the predicate. Returns count removed.

Use for conditional bulk remove. Best practice: Define clear predicates; O(n) time.

Example:
```csharp
int removed = set.RemoveWhere(s => s.StartsWith("a"));  // Removes "apple" if present
```

### SetEquals(IEnumerable<T> other)
Checks if this set equals 'other' (same items, ignore order).

Best practice: For exact match comparisons.

Example:
```csharp
bool equals = set1.SetEquals(new HashSet<string> { "apple" });  // true
```

### SymmetricExceptWith(IEnumerable<T> other)
Keeps only items in exactly one set (symmetric difference, XOR).

Modifies in-place. Best practice: For unique-to-each differences.

Example:
```csharp
set1.SymmetricExceptWith(set2);  // Items only in one
```

### TrimExcess()
Shrinks capacity to fit current items.

Use for memory optimization. Best practice: After removals.

Example:
```csharp
set.TrimExcess();
```

### TryGetValue(T equalValue, out T actualValue)
Gets the actual stored value equal to 'equalValue'. Returns `true` if found. .NET 4.7.2+.

Use when custom equality might differ. Best practice: Rare; mostly for complex types.

Example:
```csharp
if (set.TryGetValue("apple", out string found)) { /* use found */ }
```

### UnionWith(IEnumerable<T> other)
Adds all items from 'other' (union), ignoring duplicates.

Best practice: Combines sets efficiently.

Example:
```csharp
set1.UnionWith(set2);  // Adds unique from set2
```

That's all the methods for `HashSet<T>`! It's all about uniqueness and set math—great for data deduplication. Practice by creating a HashSet from a List with duplicates, then try set operations like Union/Intersect. Share your code for best practice feedback. Next—maybe SortedSet<T> or back to List<T>? Let me know!