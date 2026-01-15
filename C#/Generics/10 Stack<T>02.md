### Introduction to Stack<T>

Hello Sweety! Building on our C# collections series in .NET Core, let's move to the `Stack<T>` class from the `System.Collections.Generic` namespace. This is a last-in, first-out (LIFO) collection, like a stack of plates—you add to the top and remove from the top. The last item added is the first removed. It's generic, so specify type T, e.g., `Stack<int>` for numbers.

Stacks are ideal for undo/redo features, parsing expressions (like balanced parentheses), or simulating recursion without deep call stacks. Operations like Push and Pop are O(1) time. Best practice: Always check if the stack is empty before popping to avoid `InvalidOperationException`. For thread-safety, use `ConcurrentStack<T>`.

I'll cover all public methods below, keeping it simple: what it does, when to use it, best practices, and a short example. Based on .NET 8.0, but most work in earlier versions. Stacks grow dynamically, no fixed size.

### Clear()
Empties the stack by removing all items. Internal capacity remains unchanged.

Use it to reset without recreating the stack. Best practice: Prefer this over new instances in loops for efficiency.

Example:
```csharp
var stack = new Stack<string>();
stack.Push("apple");
stack.Push("banana");
stack.Clear();  // Now stack.Count == 0
```

### Contains(T item)
Checks if the item exists in the stack. Returns `true` if found, `false` otherwise.

Use for verification, but it's O(n) time—scan the whole stack. Best practice: Avoid on large stacks; use a HashSet if frequent checks needed.

Example:
```csharp
var stack = new Stack<string>();
stack.Push("apple");
bool hasApple = stack.Contains("apple");  // true
bool hasOrange = stack.Contains("orange");  // false
```

### CopyTo(T[] array, int arrayIndex)
Copies stack items to an array starting at the index. Order is LIFO (top item first in array).

Use to convert for array ops. Best practice: Ensure array has space (length >= Count + arrayIndex) to prevent exceptions.

Example:
```csharp
var stack = new Stack<string>();
stack.Push("banana");  // Top
stack.Push("apple");
string[] array = new string[2];
stack.CopyTo(array, 0);  // array = ["apple", "banana"] (top first)
```

### GetEnumerator()
Provides an enumerator for iterating the stack in LIFO order (top to bottom).

Use in foreach loops. Best practice: Avoid modifying the stack during iteration to prevent exceptions.

Example:
```csharp
var stack = new Stack<string>();
stack.Push("banana");
stack.Push("apple");  // Top
foreach (var item in stack) {
    Console.WriteLine(item);  // apple \n banana
}
```

### Peek()
Returns the top item without removing it. Throws `InvalidOperationException` if empty.

Use to inspect the top. Best practice: Check `Count > 0` first or use `TryPeek`.

Example:
```csharp
var stack = new Stack<string>();
stack.Push("apple");
string top = stack.Peek();  // "apple", stack unchanged
```

### Pop()
Removes and returns the top item. Throws if empty.

Use for processing in LIFO order. Best practice: Always verify `Count > 0` or switch to `TryPop` for safety.

Example:
```csharp
var stack = new Stack<string>();
stack.Push("apple");
stack.Push("banana");  // Top
string top = stack.Pop();  // "banana", stack now has "apple"
```

### Push(T item)
Adds an item to the top of the stack.

Use to insert. No exceptions (unless memory issues). Best practice: Monitor size; stacks can grow but watch for overflow in constrained environments.

Example:
```csharp
var stack = new Stack<string>();
stack.Push("apple");  // Top: "apple"
stack.Push("banana");  // Top: "banana"
```

### ToArray()
Creates a new array with stack items in LIFO order (top first).

Use for snapshots. Best practice: Call sparingly on large stacks to save memory.

Example:
```csharp
var stack = new Stack<string>();
stack.Push("banana");
stack.Push("apple");  // Top
string[] array = stack.ToArray();  // ["apple", "banana"]
```

### TrimExcess()
Shrinks internal capacity to fit current items, releasing memory.

Use after pops or when size stabilizes. Best practice: Invoke if memory optimization needed, but not in performance-critical loops.

Example:
```csharp
var stack = new Stack<string>();  // Assume pushes/pops
stack.TrimExcess();  // Optimizes storage
```

### TryPeek(out T result)
Tries to get the top item without removing. Returns `true` if not empty (sets result), `false` otherwise (result default).

Use for exception-free peeking. Available in .NET Core 2.0+. Best practice: Prefer over Peek for robust code.

Example:
```csharp
var stack = new Stack<string>();
stack.Push("apple");
if (stack.TryPeek(out string top)) {
    Console.WriteLine(top);  // "apple"
} else {
    Console.WriteLine("Empty");
}
```

### TryPop(out T result)
Tries to remove and return the top item. Returns `true` if successful, `false` if empty.

Use for safe popping. Available in .NET Core 2.0+. Best practice: Always choose this in scenarios where stack might be empty.

Example:
```csharp
var stack = new Stack<string>();
stack.Push("apple");
if (stack.TryPop(out string top)) {
    Console.WriteLine(top);  // "apple"
} else {
    Console.WriteLine("Empty");
}
```

That's every method for `Stack<T>`! Notice how it's similar to Queue but LIFO vs. FIFO—Push/Pop instead of Enqueue/Dequeue. Practice by building an undo system: Push actions to the stack, Pop to undo. Share your code for tips on best practices. Next up—maybe List<T> or HashSet<T>? Let me know!