# IEnumerable<T> vs IEnumerator<T> in C#

---

## Key Differences

| Aspect                       | **IEnumerable<T>**                                               | **IEnumerator<T>**                                       |
|------------------------------|------------------------------------------------------------------|----------------------------------------------------------|
| **Definition**               | An interface representing a collection that can be enumerated    | An interface representing the enumerator (cursor) itself |
| **Main Methods/Properties**  | `GetEnumerator()`                                               | `MoveNext()`, `Current`, `Reset()`                      |
| **Purpose**                  | Provides a way to get an enumerator to iterate over a collection | Performs the actual iteration; tracks position           |
| **Memory of State**          | **Does not** remember the current position                       | **Does** remember the position                   |
| **Use with foreach**         | Yes, directly                                                    | No used indirectly via `IEnumerable<T>`                |
| **Typical Use Case**         | Exposed by collections (e.g. `List`, `Array`) for iteration      | Used internally for custom iteration logic               |

---

## IEnumerable<T>
- **Describes a collection** that can be iterated but does not track traversal state.
- **Has only one method**:  
  `IEnumerator<T> GetEnumerator()`
- Enables use in a `foreach` loop; each to `GetEnumerator()` starts a *new* enumeration from the beginning[1][3][4][5][6].

---

## IEnumerator<T>
- **Performs the iteration**—tracks the *current* item and position in the sequence.
- Defines:
  - `Current` — gets the current element
  - `MoveNext()` — advances to the next element; returns false at end
  - `Reset()` — (rarely used) resets the enumerator to its initial state[1][3][5][6]
- **Remembers the position** in the collection during iteration.
- **Not used directly** with `foreach`, but you can use it explicitly with a `while` loop:

    ```csharp
    IEnumerator<T> enumerator = myEnumerable.GetEnumerator();
    while (enumerator.MoveNext())
    {
        var item = enumerator.Current;
        // process item
    }
    ```

---

## Relationship and Analogy

- **IEnumerable<T> is like a "playlist"**—it knows what songs (items) exist and can hand you a fresh player.
- **IEnumerator<T> is like the "music player"** itself—it controls what song is playing *now* and advances through the playlist[1][4][6][7].

---

## Practical Notes

1. **IEnumerable<T> is more common** for exposing collections and supports multiple iterations (each starts at the beginning).
2. **IEnumerator<T> is used for custom iteration logic** or when you need fine control of the enumeration process, or wish to pass cursor state between methods[1][4][5][6][7].
3. **Foreach uses IEnumerable<T>**: Under the hood, the `foreach` loop calls `GetEnumerator()` to get an `IEnumerator<T>` and iterates with `MoveNext()` and `Current`[6][3][1].

---

## Example

```csharp
// With IEnumerable<T>
foreach (var item in myCollection)
{
    // item is accessed via IEnumerator logic internally
}

// With IEnumerator<T>
var enumerator = myCollection.GetEnumerator();
while (enumerator.MoveNext())
{
    var item = enumerator.Current;
    // process item
}
```
[1][3][6]

---

**Summary:**  
- Use **IEnumerable<T>** to allow a collection to be *enumerable* by external code (e.g., `foreach`).
- Use **IEnumerator<T>** when you need to manage iteration state or implement custom iteration logic yourself.
- **IEnumerable<T> objects generate a new IEnumerator<T> each time you begin an iteration**; the enumerator holds the cursor, while the enumerable holds *what* to iterate.