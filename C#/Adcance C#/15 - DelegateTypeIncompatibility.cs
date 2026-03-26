using System;

namespace TechnicalCurosity.Delegates
{
    // Two delegates with identical signatures — but they are different types
    delegate void Notify(string message);
    delegate void Alert(string message);

    // A delegate with a different signature for contrast
    delegate int Transform(string input);

    public class DelegateTypeIncompatibility
    {
        // A simple method that matches both Notify and Alert signatures
        static void PrintToConsole(string message)
        {
            Console.WriteLine($"[Console] {message}");
        }

        static void LogToFile(string message)
        {
            Console.WriteLine($"[File] {message}");
        }

        public static void Explore()
        {
            Console.WriteLine("=== Delegate Type Incompatibility ===\n");

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
            // 3. Explicit construction — this is allowed
            //    You're saying: "I know what I'm doing, wrap the
            //    same underlying method in a new Alert instance"
            // -------------------------------------------------------
            Alert alert2 = new Alert(notify);
            Console.WriteLine("\n2. Explicit construction (new Alert(notify)):");
            alert2("I was created from a Notify delegate");

            // -------------------------------------------------------
            // 4. .Invoke() doesn't help either — it's still type-based
            // -------------------------------------------------------
            // UNCOMMENT TO SEE COMPILE ERROR:
            // Alert alert3 = notify.Invoke;  // Still CS0029

            // -------------------------------------------------------
            // 5. But you CAN assign the .Method target directly
            //    This bypasses the delegate type entirely
            // -------------------------------------------------------
            Alert alert3 = new Alert(notify.Invoke);
            Console.WriteLine("\n3. Constructed via notify.Invoke:");
            alert3("I was created from notify.Invoke");

            // -------------------------------------------------------
            // 6. What about Delegate.Combine / multicast?
            //    Even combining requires the same delegate type
            // -------------------------------------------------------
            Notify notify2 = LogToFile;
            Notify combined = notify + notify2;  // Works — same type

            Console.WriteLine("\n4. Multicast (same type works):");
            combined("Multicast message");

            // UNCOMMENT TO SEE RUNTIME ERROR:
            // Delegate bad = Delegate.Combine(notify, alert);  // ArgumentException at runtime!

            // -------------------------------------------------------
            // 7. Generic delegates (Action/Func) sidestep this issue
            //    because both variables share the SAME type
            // -------------------------------------------------------
            Action<string> action1 = PrintToConsole;
            Action<string> action2 = action1;  // Works perfectly!

            Console.WriteLine("\n5. Action<string> — same type, no problem:");
            action1("From action1");
            action2("From action2 (assigned from action1)");

            // -------------------------------------------------------
            // 8. Practical takeaway
            // -------------------------------------------------------
            Console.WriteLine("\n=== Key Takeaways ===");
            Console.WriteLine("• Each 'delegate' declaration creates a DISTINCT type");
            Console.WriteLine("• Same signature ≠ same type (nominal typing)");
            Console.WriteLine("• Use 'new DelegateType(other)' for explicit conversion");
            Console.WriteLine("• Use Action<T>/Func<T> to avoid the problem entirely");
        }
    }
}
