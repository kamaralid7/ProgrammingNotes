using System;
using System.Collections.Generic;
using System.Linq;

namespace TechnicalCurosity.Delegates
{
    // ====================================================================
    // PART A: Two delegates with identical signatures — different types
    // ====================================================================
    delegate void Notify(string message);
    delegate void Alert(string message);
    delegate int Transform(string input);

    // ====================================================================
    // PART B: Variance delegates — for covariance/contravariance demos
    // ====================================================================
    delegate T Producer<out T>();             // covariant   — T only returned
    delegate void Consumer<in T>(T input);   // contravariant — T only consumed

    // ====================================================================
    // Inheritance hierarchy for variance demos
    // ====================================================================
    class Animal
    {
        public string Name { get; set; } = "Animal";
        public override string ToString() => $"[{GetType().Name}] {Name}";
    }
    class Dog : Animal
    {
        public string Breed { get; set; } = "Labrador";
    }
    class GuideDog : Dog
    {
        public bool Certified { get; set; } = true;
    }
    class Cat : Animal { }

    // ====================================================================
    // SECTION 1: Delegate Type Incompatibility
    // ====================================================================
    public class DelegateTypeIncompatibility
    {
        static void PrintToConsole(string message)
        {
            Console.WriteLine($"  [Console] {message}");
        }

        static void LogToFile(string message)
        {
            Console.WriteLine($"  [File] {message}");
        }

        public static void Explore()
        {
            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║   SECTION 1: Delegate Type Incompatibility  ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝\n");

            // -------------------------------------------------------
            // 1. Both delegates can wrap the SAME method — no problem
            // -------------------------------------------------------
            Notify notify = PrintToConsole;
            Alert  alert  = PrintToConsole;

            Console.WriteLine("1. Same method, different delegate types:");
            notify("Hello from Notify");
            alert("Hello from Alert");

            // -------------------------------------------------------
            // 2. Direct assignment fails — even though signatures match
            // -------------------------------------------------------
            // UNCOMMENT TO SEE COMPILE ERROR:
            // Alert alert2 = notify;  // CS0029: Cannot implicitly convert type 'Notify' to 'Alert'

            // -------------------------------------------------------
            // 3. Explicit construction — wrap in a new delegate instance
            // -------------------------------------------------------
            Alert alert2 = new Alert(notify);
            Console.WriteLine("\n2. Explicit construction (new Alert(notify)):");
            alert2("I was created from a Notify delegate");

            // -------------------------------------------------------
            // 4. .Invoke() doesn't help — still type-based
            // -------------------------------------------------------
            // UNCOMMENT TO SEE COMPILE ERROR:
            // Alert alert3 = notify.Invoke;  // Still CS0029

            // -------------------------------------------------------
            // 5. But you CAN assign .Invoke as a method group
            // -------------------------------------------------------
            Alert alert3 = new Alert(notify.Invoke);
            Console.WriteLine("\n3. Constructed via notify.Invoke:");
            alert3("I was created from notify.Invoke");

            // -------------------------------------------------------
            // 6. Multicast — combining requires the SAME delegate type
            // -------------------------------------------------------
            Notify notify2  = LogToFile;
            Notify combined = notify + notify2;

            Console.WriteLine("\n4. Multicast (same type works):");
            combined("Multicast message");

            // UNCOMMENT TO SEE RUNTIME ERROR:
            // Delegate bad = Delegate.Combine(notify, alert);  // ArgumentException at runtime!

            // -------------------------------------------------------
            // 7. Action/Func sidestep the issue — same generic type
            // -------------------------------------------------------
            Action<string> action1 = PrintToConsole;
            Action<string> action2 = action1;

            Console.WriteLine("\n5. Action<string> — same type, assignment works:");
            action1("From action1");
            action2("From action2 (assigned from action1)");

            Console.WriteLine("\n  Key Takeaways:");
            Console.WriteLine("  • Each 'delegate' declaration = a DISTINCT type");
            Console.WriteLine("  • Same signature ≠ same type (nominal typing)");
            Console.WriteLine("  • Use 'new DelegateType(other)' for explicit conversion");
            Console.WriteLine("  • Use Action<T>/Func<T> to avoid the problem entirely");
        }
    }

    // ====================================================================
    // SECTION 2: Parameter Compatibility — Contravariance
    // ====================================================================
    public class ParameterCompatibility
    {
        static void HandleAnimal(Animal a)
        {
            Console.WriteLine($"  HandleAnimal called with: {a}");
        }

        static void HandleDog(Dog d)
        {
            Console.WriteLine($"  HandleDog called with: {d} (Breed: {d.Breed})");
        }

        public static void Explore()
        {
            Console.WriteLine("\n╔══════════════════════════════════════════════╗");
            Console.WriteLine("║   SECTION 2: Parameter Contravariance       ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝\n");

            // -------------------------------------------------------
            // 1. A method that takes Animal can be used where Dog is expected
            //    Reason: if it can handle ANY Animal, it can handle a Dog
            // -------------------------------------------------------
            Console.WriteLine("1. Contravariance — broad param → narrow delegate:");
            Action<Dog> dogHandler = HandleAnimal; // ✅ contravariance
            dogHandler(new Dog { Name = "Rex", Breed = "Husky" });

            // -------------------------------------------------------
            // 2. The reverse does NOT work
            //    A method expecting Dog can't handle all Animals (Cat would crash)
            // -------------------------------------------------------
            // UNCOMMENT TO SEE COMPILE ERROR:
            // Action<Animal> animalHandler = HandleDog;  // ❌ CS0029

            // -------------------------------------------------------
            // 3. Works through the ENTIRE hierarchy
            // -------------------------------------------------------
            Console.WriteLine("\n2. Full hierarchy — HandleAnimal used as Action<GuideDog>:");
            Action<GuideDog> guideDogHandler = HandleAnimal; // ✅ Animal ← Dog ← GuideDog
            guideDogHandler(new GuideDog { Name = "Buddy", Breed = "Golden", Certified = true });

            // -------------------------------------------------------
            // 4. Practical example: event subscription
            // -------------------------------------------------------
            Console.WriteLine("\n3. Practical: one handler for multiple event types:");
            Action<Animal> universalLogger = a => Console.WriteLine($"  [LOG] {a}");

            Action<Dog>      logDog      = universalLogger; // ✅
            Action<Cat>      logCat      = universalLogger; // ✅
            Action<GuideDog> logGuideDog = universalLogger; // ✅

            logDog(new Dog { Name = "Max" });
            logCat(new Cat { Name = "Whiskers" });
            logGuideDog(new GuideDog { Name = "Scout" });

            Console.WriteLine("\n  Rule: Parameters are CONTRAVARIANT");
            Console.WriteLine("  broad parameter → narrow delegate ✅ (safe)");
            Console.WriteLine("  narrow parameter → broad delegate ❌ (unsafe)");
        }
    }

    // ====================================================================
    // SECTION 3: Return Type Compatibility — Covariance
    // ====================================================================
    public class ReturnTypeCompatibility
    {
        static Dog    GetDog()    => new Dog { Name = "Fido", Breed = "Poodle" };
        static Animal GetAnimal() => new Animal { Name = "Generic Animal" };

        public static void Explore()
        {
            Console.WriteLine("\n╔══════════════════════════════════════════════╗");
            Console.WriteLine("║   SECTION 3: Return Type Covariance         ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝\n");

            // -------------------------------------------------------
            // 1. A method returning Dog can be used where Animal return is expected
            //    Reason: caller asks for Animal, gets a Dog — that's fine
            // -------------------------------------------------------
            Console.WriteLine("1. Covariance — narrow return → broad delegate:");
            Func<Animal> getAnimal = GetDog; // ✅ Func<Dog> → Func<Animal>
            Animal result = getAnimal();
            Console.WriteLine($"  Got: {result} (actual type: {result.GetType().Name})");

            // -------------------------------------------------------
            // 2. The reverse does NOT work
            //    Caller expects Dog, but method might return a plain Animal
            // -------------------------------------------------------
            // UNCOMMENT TO SEE COMPILE ERROR:
            // Func<Dog> getDog = GetAnimal;  // ❌ CS0029

            // -------------------------------------------------------
            // 3. Works through the ENTIRE hierarchy
            // -------------------------------------------------------
            Console.WriteLine("\n2. Full hierarchy — GuideDog returned as Animal:");
            Func<Animal> getFromDeep = () => new GuideDog
            {
                Name = "Atlas",
                Breed = "Shepherd",
                Certified = true
            };
            result = getFromDeep();
            Console.WriteLine($"  Got: {result} (actual type: {result.GetType().Name})");

            // -------------------------------------------------------
            // 4. Practical: Factory pattern with covariance
            // -------------------------------------------------------
            Console.WriteLine("\n3. Practical: Factory registry using Func<Animal>:");
            var factories = new Dictionary<string, Func<Animal>>
            {
                ["dog"]      = () => new Dog { Name = "Rex" },           // Func<Dog> → Func<Animal>
                ["cat"]      = () => new Cat { Name = "Luna" },          // Func<Cat> → Func<Animal>
                ["guideDog"] = () => new GuideDog { Name = "Hero" },     // Func<GuideDog> → Func<Animal>
            };

            foreach (var (key, factory) in factories)
            {
                var animal = factory();
                Console.WriteLine($"  Factory[{key}] → {animal} (type: {animal.GetType().Name})");
            }

            Console.WriteLine("\n  Rule: Return types are COVARIANT");
            Console.WriteLine("  narrow return → broad delegate ✅ (safe)");
            Console.WriteLine("  broad return → narrow delegate ❌ (unsafe)");
        }
    }

    // ====================================================================
    // SECTION 4: Generic Delegate Variance — in/out in action
    // ====================================================================
    public class GenericDelegateVariance
    {
        public static void Explore()
        {
            Console.WriteLine("\n╔══════════════════════════════════════════════╗");
            Console.WriteLine("║   SECTION 4: Generic Delegate Variance      ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝\n");

            // -------------------------------------------------------
            // 1. Custom covariant delegate — Producer<out T>
            // -------------------------------------------------------
            Console.WriteLine("1. Producer<out T> — covariance:");

            Producer<Dog>    dogProducer    = () => new Dog { Name = "Bolt" };
            Producer<Animal> animalProducer = dogProducer; // ✅ covariance

            Animal produced = animalProducer();
            Console.WriteLine($"  Produced: {produced} (type: {produced.GetType().Name})");

            // -------------------------------------------------------
            // 2. Custom contravariant delegate — Consumer<in T>
            // -------------------------------------------------------
            Console.WriteLine("\n2. Consumer<in T> — contravariance:");

            Consumer<Animal> animalConsumer = a => Console.WriteLine($"  Consumed: {a}");
            Consumer<Dog>    dogConsumer    = animalConsumer; // ✅ contravariance

            dogConsumer(new Dog { Name = "Spike", Breed = "Bulldog" });

            // -------------------------------------------------------
            // 3. Func<in T, out TResult> — BOTH at once
            // -------------------------------------------------------
            Console.WriteLine("\n3. Func<in T, out TResult> — both variance directions:");

            // Func that takes Animal (broad) and returns Dog (narrow)
            Func<Animal, Dog> specificFunc = a => new Dog { Name = a.Name, Breed = "Mixed" };

            // Assign to Func<Dog, Animal> — contravariant on input, covariant on output
            Func<Dog, Animal> generalFunc = specificFunc; // ✅ both directions at once

            Animal resultAnimal = generalFunc(new Dog { Name = "Input Dog" });
            Console.WriteLine($"  Input: Dog → Output: {resultAnimal} (type: {resultAnimal.GetType().Name})");

            // -------------------------------------------------------
            // 4. Invariance — T in both positions, no variance
            // -------------------------------------------------------
            Console.WriteLine("\n4. Invariance — when T is in both input AND output:");
            Console.WriteLine("  Func<T, T> means T goes in AND comes out");
            Console.WriteLine("  Cannot assign Func<Dog, Dog> to Func<Animal, Animal>");
            Console.WriteLine("  Because: what if caller passes a Cat as input?");
            Console.WriteLine("  The Dog→Dog function can't handle that.");

            // UNCOMMENT TO SEE COMPILE ERROR:
            // Func<Dog, Dog> dogToDog = d => new Dog { Name = d.Name };
            // Func<Animal, Animal> animalToAnimal = dogToDog;  // ❌ invariant!

            Console.WriteLine("\n  Rules:");
            Console.WriteLine("  • out T  → T only in RETURN position  → covariant");
            Console.WriteLine("  • in T   → T only in PARAM position   → contravariant");
            Console.WriteLine("  • no mod → T in both positions         → invariant");
        }
    }

    // ====================================================================
    // SECTION 5: Real-World Covariance / Contravariance Usage
    // ====================================================================
    public class RealWorldVariance
    {
        public static void Explore()
        {
            Console.WriteLine("\n╔══════════════════════════════════════════════╗");
            Console.WriteLine("║   SECTION 5: Real-World Variance Examples   ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝\n");

            // -------------------------------------------------------
            // 1. IEnumerable<out T> — the most common covariance
            // -------------------------------------------------------
            Console.WriteLine("1. IEnumerable<out T> — read-only collection covariance:");
            List<Dog> dogs = new()
            {
                new Dog { Name = "Max",  Breed = "Husky" },
                new Dog { Name = "Ace",  Breed = "Corgi" },
                new Dog { Name = "Zeus", Breed = "German Shepherd" },
            };

            // ✅ List<Dog> → IEnumerable<Animal> — covariance at work
            IEnumerable<Animal> animals = dogs;
            foreach (var a in animals)
                Console.WriteLine($"  {a}");

            // -------------------------------------------------------
            // 2. IComparer<in T> — the most common contravariance
            // -------------------------------------------------------
            Console.WriteLine("\n2. IComparer<in T> — one comparer for the whole hierarchy:");
            var byNameComparer = Comparer<Animal>.Create((a, b) =>
                string.Compare(a.Name, b.Name, StringComparison.Ordinal));

            // ✅ IComparer<Animal> used where IComparer<Dog> is expected
            dogs.Sort(byNameComparer);
            Console.WriteLine("  Dogs sorted by name:");
            foreach (var d in dogs)
                Console.WriteLine($"    {d.Name} ({d.Breed})");

            // -------------------------------------------------------
            // 3. Action<in T> — single handler for many types
            // -------------------------------------------------------
            Console.WriteLine("\n3. Action<in T> — universal handler via contravariance:");
            Action<Animal> logAnimal = a => Console.WriteLine($"  [LOG] {a}");

            // Use the same Animal handler for any subtype
            Action<Dog>      logDog = logAnimal;      // ✅
            Action<Cat>      logCat = logAnimal;      // ✅
            Action<GuideDog> logGuide = logAnimal;    // ✅

            logDog(new Dog { Name = "Rover" });
            logCat(new Cat { Name = "Mittens" });
            logGuide(new GuideDog { Name = "Scout" });

            // -------------------------------------------------------
            // 4. Func<out T> — factory covariance
            // -------------------------------------------------------
            Console.WriteLine("\n4. Func<out TResult> — factory pattern:");
            var registry = new Dictionary<string, Func<Animal>>
            {
                ["dog"] = () => new Dog { Name = "Buddy", Breed = "Beagle" },
                ["cat"] = () => new Cat { Name = "Shadow" },
            };

            foreach (var (key, factory) in registry)
                Console.WriteLine($"  registry[\"{key}\"] → {factory()}");

            // -------------------------------------------------------
            // 5. The TRAP — why List<T> is invariant
            // -------------------------------------------------------
            Console.WriteLine("\n5. Why List<T> is INVARIANT (cannot be covariant):");
            Console.WriteLine("  List<Dog> dogs = new();");
            Console.WriteLine("  List<Animal> animals = dogs;  // ❌ won't compile");
            Console.WriteLine("  animals.Add(new Cat());       // Cat in Dog list → broken!");
            Console.WriteLine("  That's why List<T> is invariant — T goes in AND out.");
            Console.WriteLine("  IEnumerable<T> IS covariant — T only comes out (read-only).");

            // -------------------------------------------------------
            // 6. Practical LINQ — covariance makes this seamless
            // -------------------------------------------------------
            Console.WriteLine("\n6. LINQ + covariance — seamless mixed-type processing:");
            List<Dog> moreDogs = new() { new Dog { Name = "Tank" } };
            List<Cat> cats     = new() { new Cat { Name = "Cleo" } };

            // Both List<Dog> and List<Cat> covariant to IEnumerable<Animal>
            IEnumerable<Animal> allAnimals = moreDogs
                .Cast<Animal>()
                .Concat(cats.Cast<Animal>());

            Console.WriteLine("  All animals:");
            foreach (var a in allAnimals)
                Console.WriteLine($"    {a}");
        }
    }

    // ====================================================================
    // RUNNER — Execute all sections
    // ====================================================================
    public class Program
    {
        public static void Main(string[] args)
        {
            DelegateTypeIncompatibility.Explore();
            ParameterCompatibility.Explore();
            ReturnTypeCompatibility.Explore();
            GenericDelegateVariance.Explore();
            RealWorldVariance.Explore();

            Console.WriteLine("\n══════════════════════════════════════════════");
            Console.WriteLine("  MASTER CHEAT SHEET");
            Console.WriteLine("══════════════════════════════════════════════");
            Console.WriteLine("  co     = with    → out → return flows OUT  → narrow → wide  ✅");
            Console.WriteLine("  contra = against → in  → input  flows IN   → wide → narrow  ✅");
            Console.WriteLine("  invariant        →       T in both places  → exact match only");
            Console.WriteLine("");
            Console.WriteLine("  IEnumerable<out T>   → covariant     (read-only)");
            Console.WriteLine("  IComparer<in T>      → contravariant (consume-only)");
            Console.WriteLine("  Func<in T, out R>    → both!         (T in, R out)");
            Console.WriteLine("  List<T>              → invariant     (read AND write)");
            Console.WriteLine("══════════════════════════════════════════════\n");
        }
    }
}
