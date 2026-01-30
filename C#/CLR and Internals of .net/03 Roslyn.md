# Roslyn Compiler: Architecture, Stages & Build Integration

## 1️⃣ What Roslyn *actually* is (important framing)

Roslyn is **not just a compiler**.

It is:

* A **compiler-as-a-service**
* Exposes **syntax trees, semantic models, symbols**
* Fully **incremental & API-driven**
* Same engine used by:

  * `csc`
  * Visual Studio IntelliSense
  * Analyzers
  * Source Generators
  * Refactorings

This is why C# tooling feels “alive”.

---

## 2️⃣ High-level Roslyn compilation pipeline

```text
Source Text
   ↓
Lexical Analysis (Tokens)
   ↓
Syntax Analysis (Syntax Tree)
   ↓
Semantic Analysis (Symbols + Types)
   ↓
Binding & Lowering
   ↓
IL + Metadata Emission
```

Each stage produces **immutable data structures**.

---

## 3️⃣ Stage-by-stage breakdown (deep but clean)

---

### 🔹 Stage 1: Lexical Analysis (Tokenizer)

**Input:** Raw text
**Output:** Tokens

Example:

```csharp
int x = 10;
```

Tokens:

```
[int] [identifier:x] [=] [literal:10] [;]
```

Key properties:

* Whitespace preserved (important for formatting tools)
* Trivia (comments, whitespace) are first-class citizens

📌 This is why Roslyn can do *lossless* code rewriting.

---

### 🔹 Stage 2: Syntax Analysis (Parsing)

**Input:** Tokens
**Output:** Syntax Tree (AST)

* Grammar-only validation
* No type info
* Tree is **green/red tree model**:

  * Green = immutable, compact
  * Red = parent-linked, navigable

Example:

```csharp
int x = "hello"; // syntactically valid
```

Still passes syntax.

---

### 🔹 Stage 3: Semantic Analysis

This is where meaning enters.

**Produces:**

* `SemanticModel`
* `Symbol` table:

  * Types
  * Methods
  * Fields
  * Namespaces

Validates:

* Type correctness
* Overload resolution
* Accessibility
* Generic constraints
* Nullable flow analysis

This is **the most expensive stage**.

---

### 🔹 Stage 4: Binding & Lowering

Roslyn now **rewrites high-level constructs** into simpler forms.

Examples:

* `foreach` → `for`
* `using` → `try/finally`
* `async/await` → state machines
* Pattern matching → decision trees

📌 This is *not* IL yet — still C#-level but “desugared”.

---

### 🔹 Stage 5: IL & Metadata Emission

Final stage:

Produces:

* **CIL (MSIL)**
* **Metadata tables**
* Optional **PDB**

This is handed to the CLR.

---

## 4️⃣ What files Roslyn generates (DLL / EXE anatomy)

### 📦 Output artifacts

| File                  | Purpose             |
| --------------------- | ------------------- |
| `.dll`                | Library assembly    |
| `.exe`                | Executable assembly |
| `.pdb`                | Debug symbols       |
| `.deps.json`          | Dependency graph    |
| `.runtimeconfig.json` | Runtime settings    |
| `.xml`                | XML documentation   |

---

## 5️⃣ Internal structure of DLL / EXE

### Assembly layout (simplified)

```
PE Header
CLR Header
Metadata Tables
IL Code
Resources
```

---

### 🔹 Metadata tables (this is gold)

Contain:

* Type definitions
* Method signatures
* Field layouts
* Attributes
* Assembly references

Why this matters:

* Reflection
* Dependency Injection
* Serialization
* ORMs
* Dynamic proxies

📌 C# is **metadata-first**, unlike C++.

---

### 🔹 IL Code

* Stack-based instructions
* CPU-agnostic
* Verifiable
* GC-aware

---

### 🔹 PDB file

Contains:

* Source file paths
* Line numbers
* Local variables
* Async state machine maps

Modern PDBs:

* Portable
* Cross-platform

---

## 6️⃣ How `dotnet build` actually works

### Build pipeline (simplified)

```text
dotnet build
   ↓
MSBuild
   ↓
Resolve references
   ↓
Run analyzers
   ↓
Run source generators
   ↓
Roslyn compilation
   ↓
Emit IL + metadata
   ↓
Copy outputs
```

---

### MSBuild’s role

* Orchestration
* Dependency resolution
* Incremental builds
* Project-to-project references

Roslyn is **invoked by MSBuild**, not the other way around.

---

## 7️⃣ Source Generators (this is where power lies)

### What source generators are

> Compile-time code that **writes C# code**
> which then becomes part of your compilation.

No reflection
No runtime cost
No magic strings

---

### Where they run

```text
Syntax Tree
   ↓
Source Generator executes
   ↓
New C# source added
   ↓
Compilation continues
```

They run **after syntax**, **before semantic binding finishes**.

---

### What they can do

* Generate DTOs
* Mappers
* Serialization code
* DI registrations
* Strongly-typed configs
* Domain boilerplate

Examples in the wild:

* `System.Text.Json`
* `EF Core`
* `ASP.NET Minimal APIs`

---

### Simple mental model

```csharp
[AutoMapper]
public partial class User { }
```

Generator creates:

```csharp
partial class User
{
    public static UserDto ToDto(...) { ... }
}
```

Roslyn treats it as if *you wrote it yourself*.

---

## 8️⃣ How to leverage this in your projects (practical advice)

### 🔥 Use Roslyn analyzers when:

* Enforcing architectural rules
* Preventing bad patterns
* Domain invariants

Example:

* “Repositories cannot depend on Controllers”
* “No DateTime.Now allowed”

---

### 🔥 Use source generators when:

* Reflection is slow
* Boilerplate explodes
* Patterns repeat

Concrete ideas for **your kind of projects**:

* Generate validation chains
* Generate DTO ↔ Entity mapping
* Generate API error contracts
* Generate logging wrappers
* Generate domain event dispatchers

---

### 🔥 Combine analyzers + generators

Pattern:

1. Analyzer detects intent
2. Generator emits implementation
3. Compiler enforces correctness

This is how modern .NET frameworks are built.

---

## 9️⃣ Why Roslyn is a competitive advantage

Compared to Java:

* No comparable compile-time generation
* Annotation processors are weaker

Compared to C++:

* No metadata
* No safe introspection

Roslyn enables:

* Tooling-driven architecture
* Compile-time correctness
* Zero-runtime-cost abstractions

---

## 10️⃣ Mental summary (pin this)

```
Roslyn = Parser + Type System + Rewrite Engine + Emitter
MSBuild = Orchestrator
Source Generators = Compile-time metaprogramming
DLL = IL + Metadata
CLR = Execution + Optimization
```

---

## Where we can go next (your level 😈)

* Write a **real source generator**
* Inspect metadata tables manually
* Build a **custom analyzer**
* Understand async lowering output
* Roslyn vs JVM compiler internals
* How EF Core uses generators internally

Just say the direction — we’ll go *all in*.
