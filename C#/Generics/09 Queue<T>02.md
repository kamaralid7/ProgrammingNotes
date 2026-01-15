### Introduction to Queue<T>

Hello Sweety! Continuing our journey through C# collections in .NET Core, let's explore the `Queue<T>` class from the `System.Collections.Generic` namespace. This is a first-in, first-out (FIFO) collection, like a real-world queue at a store—items added first are removed first. It's generic, so you specify the type T, e.g., `Queue<string>` for a queue of names.

Queues are great for scenarios like task processing, breadth-first search in graphs, or buffering data. They provide O(1) time for Enqueue and Dequeue operations. Best practice: Use queues when order of addition matters, and always check if the queue is empty before dequeuing to avoid `InvalidOperationException`. If you need thread-safety, consider `ConcurrentQueue<T>` instead.

Now, I'll describe all the public methods of `Queue<T>`. These are based on .NET 8.0, but most apply to earlier versions. I'll keep it simple: what it does, when to use it, best practices, and a short example. Note: Queues aren't fixed-size; they grow dynamically.

### Clear()
Removes all items from the queue, resetting it to empty. Doesn't change the internal capacity.

Use it to reuse the queue without creating a new instance. Best practice: Call this in loops to avoid memory buildup.

Example:
```csharp
var queue = new Queue<string>();
queue.Enqueue("apple");
queue.Enqueue("banana");
queue.Clear();  // Now queue.Count == 0
```

### Contains(T item)
Checks if a specific item exists in the queue. Returns `true` if found, `false` otherwise.

Use it to verify presence before operations. It's O(n) time, so avoid in large queues. Best practice: If frequent checks are needed, consider a HashSet alongside the queue.

Example:
```csharp
var queue = new Queue<string>();
queue.Enqueue("apple");
bool hasApple = queue.Contains("apple");  // true
bool hasOrange = queue.Contains("orange");  // false
```

### CopyTo(T[] array, int arrayIndex)
Copies the queue's items to an array, starting at the specified index. Items are copied in FIFO order.

Use it to convert the queue to an array for random access. Best practice: Ensure the array has enough space (length >= Count + arrayIndex) to avoid exceptions.

Example:
```csharp
var queue = new Queue<string>();
queue.Enqueue("apple");
queue.Enqueue("banana");
string[] array = new string[2];
queue.CopyTo(array, 0);  // array = ["apple", "banana"]
```

### Dequeue()
Removes and returns the item at the front of the queue (oldest item).

Use it to process items in order. Throws `InvalidOperationException` if empty. Best practice: Always check `Count > 0` or use `TryDequeue` (below) for safety.

Example:
```csharp
var queue = new Queue<string>();
queue.Enqueue("apple");
queue.Enqueue("banana");
string first = queue.Dequeue();  // "apple", queue now has "banana"
```

### Enqueue(T item)
Adds an item to the end of the queue (newest position).

Use it to insert items. No exceptions unless out of memory. Best practice: Enqueue in batches if possible, and monitor size for performance.

Example:
```csharp
var queue = new Queue<string>();
queue.Enqueue("apple");  // Queue: "apple"
queue.Enqueue("banana");  // Queue: "apple", "banana"
```

### GetEnumerator()
Returns an enumerator to iterate through the queue in FIFO order. Implements `IEnumerable<T>`.

Use it for foreach loops. Best practice: Don't modify the queue while enumerating to avoid exceptions.

Example:
```csharp
var queue = new Queue<string>();
queue.Enqueue("apple");
queue.Enqueue("banana");
foreach (var item in queue) {
    Console.WriteLine(item);  // apple \n banana
}
```

### Peek()
Returns the item at the front without removing it.

Use it to inspect the next item. Throws `InvalidOperationException` if empty. Best practice: Check `Count > 0` or use `TryPeek` (below).

Example:
```csharp
var queue = new Queue<string>();
queue.Enqueue("apple");
string next = queue.Peek();  // "apple", queue unchanged
```

### ToArray()
Creates and returns a new array with all items in FIFO order.

Use it for snapshots or when needing array operations. Best practice: Call this sparingly on large queues to avoid memory overhead.

Example:
```csharp
var queue = new Queue<string>();
queue.Enqueue("apple");
queue.Enqueue("banana");
string[] array = queue.ToArray();  // ["apple", "banana"]
```

### TrimExcess()
Reduces the internal capacity to match the current number of items, freeing memory.

Use it after removing items or when the queue stabilizes. Best practice: Call if memory is tight, but not in hot paths as it may reallocate.

Example:
```csharp
var queue = new Queue<string>();  // Assume added/removed items
queue.TrimExcess();  // Optimizes storage
```

### TryDequeue(out T result)
Tries to remove and return the front item. Returns `true` if successful (queue not empty), `false` otherwise (result set to default).

Use it for exception-free dequeuing. Available in .NET Core 2.0+. Best practice: Prefer this over Dequeue for robust code.

Example:
```csharp
var queue = new Queue<string>();
queue.Enqueue("apple");
if (queue.TryDequeue(out string item)) {
    Console.WriteLine(item);  // "apple"
} else {
    Console.WriteLine("Empty");
}
```

### TryPeek(out T result)
Tries to return the front item without removing it. Returns `true` if successful, `false` otherwise.

Use it for safe peeking. Available in .NET Core 2.0+. Best practice: Always use this in scenarios where the queue might be empty.

Example:
```csharp
var queue = new Queue<string>();
queue.Enqueue("apple");
if (queue.TryPeek(out string item)) {
    Console.WriteLine(item);  // "apple"
} else {
    Console.WriteLine("Empty");
}


Method Name,Description in Simple Words,When to Use / Best Practice
Clear(),Empties the queue.,Reuse the queue; check Count after.
Contains(T item),Checks if an item exists (O(n) time).,Rare use; avoid on large queues.
"CopyTo(T[] array, int arrayIndex)",Copies queue to an array.,For array conversion; ensure array space.
Dequeue(),Removes and returns front item (throws if empty).,Processing; always check Count > 0.
Enqueue(T item),Adds item to the end.,Inserting; monitor queue size.
GetEnumerator(),Gets iterator for looping.,Use in foreach; don't modify during loop.
Peek(),Views front item without removing (throws if empty).,Inspecting; check Count > 0.
ToArray(),Returns new array of items.,Snapshots; avoid on huge queues.
TrimExcess(),Shrinks internal storage.,Memory optimization after shrinks.
TryDequeue(out T result),Safe dequeue (returns bool).,Prefer over Dequeue for no exceptions.
TryPeek(out T result),Safe peek (returns bool).,Prefer over Peek for robustness.
```

That's all the methods for `Queue<T>`! Note that unlike Dictionary, Queue doesn't have add/remove by key—it's strictly FIFO. Practice by simulating a task queue in a console app: Enqueue tasks, process with Dequeue, and handle empty cases. If you need LIFO (last-in, first-out), check out `Stack<T>` next. Questions or exercises? Let me know!