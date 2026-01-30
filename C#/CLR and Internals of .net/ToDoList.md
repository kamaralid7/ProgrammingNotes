* IL vs native instruction comparison
* JIT vs C++ compiler differences
* Why .NET sometimes beats C++
* How `Span<T>` bypasses GC pressure
* How tiered compilation works internally


* Real JIT assembly dumps (`COMPlus_JitDisasm`)
* Tier-0 vs Tier-1 JIT
* Why generics in C# can beat C++ templates
* How `Span<T>` turns into pure register code
* Why virtual calls sometimes disappear entirely

Roslyn

* Write a **real source generator**
* Inspect metadata tables manually
* Build a **custom analyzer**
* Understand async lowering output
* Roslyn vs JVM compiler internals
* How EF Core uses generators internally


IL + Metadata

* Inspect metadata tables manually (`ildasm`, `dnSpy`)
* Design a metadata-driven ETL framework
* Build SharePoint list generators from C# models
* Compare .NET metadata vs JVM bytecode
* Source generators vs metadata at runtime

CLR
* How GC cooperates with JIT
* CLR memory model & barriers
* AppDomains → AssemblyLoadContext
* How exceptions unwind at native level
* CLR vs Native AOT tradeoffs