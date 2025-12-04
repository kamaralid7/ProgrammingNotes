Below is a clear, beginner-friendly explanation of **Object-Oriented Programming (OOP) in C#**, along with examples to help you understand each concept.

---

# 🧱 **What Is OOP?**

**Object-Oriented Programming** is a programming paradigm that organizes code around **objects**—entities that contain **data** (fields/properties) and **behavior** (methods).

C# is a *strongly object-oriented language*, so understanding OOP is essential.

OOP in C# is built around four main pillars:

1. **Encapsulation**
2. **Abstraction**
3. **Inheritance**
4. **Polymorphism**

Let’s break each one down.

---

# 1️⃣ **Encapsulation**

> *Hiding data and protecting it from unauthorized access.*

You bundle related variables and methods inside a class and restrict access using **access modifiers** like `private`, `public`, or `protected`.

### ✅ Example:

```csharp
public class BankAccount
{
    private decimal balance; // hidden from outside

    public void Deposit(decimal amount)
    {
        balance += amount;
    }

    public decimal GetBalance()
    {
        return balance;
    }
}
```

**Why it matters:**
It prevents external code from messing with internal data.

---

# 2️⃣ **Abstraction**

> *Showing only essential features while hiding complex details.*

You create a simplified model for something complex. Methods do “work” behind the scenes, but the user only needs to know *how* to call them.

### ✅ Example:

```csharp
public class EmailService
{
    public void SendEmail(string address, string message)
    {
        ConnectServer();
        Authenticate();
        Send(address, message);
    }

    private void ConnectServer() { /* hidden */ }
    private void Authenticate() { /* hidden */ }
    private void Send(string addr, string msg) { /* hidden */ }
}
```

**Why it matters:**
Makes code easier to use and understand.

---

# 3️⃣ **Inheritance**

> *A class can acquire the properties and methods of another class.*

Use the `:` symbol to inherit.

### ❗Base Class (Parent)

```csharp
public class Animal
{
    public void Eat() => Console.WriteLine("Eating...");
}
```

### ✔️ Derived Class (Child)

```csharp
public class Dog : Animal
{
    public void Bark() => Console.WriteLine("Barking...");
}
```

### Usage:

```csharp
Dog dog = new Dog();
dog.Eat();   // inherited!
dog.Bark();
```

**Why it matters:**
Promotes code reuse.

---

# 4️⃣ **Polymorphism**

> *One interface, many forms.*

Meaning a method can behave differently depending on the object calling it.

There are two types in C#:

---

## ⭐ **Run-time Polymorphism (Overriding)**

Uses `virtual` and `override` keywords.

```csharp
public class Animal
{
    public virtual void Speak() => Console.WriteLine("Animal speaks");
}

public class Dog : Animal
{
    public override void Speak() => Console.WriteLine("Dog barks");
}
```

```csharp
Animal myDog = new Dog();
myDog.Speak(); // Dog barks
```

---

## ⭐ **Compile-time Polymorphism (Overloading)**

Same method name, different parameters.

```csharp
public class Calculator
{
    public int Add(int a, int b) => a + b;
    public int Add(int a, int b, int c) => a + b + c;
}
```

---

# ⚙️ **Classes and Objects**

A **class** is a blueprint.
An **object** is an instance of a class.

### Example:

```csharp
public class Person
{
    public string Name;
    public int Age;

    public void Speak()
    {
        Console.WriteLine($"Hi, I'm {Name}");
    }
}

Person p = new Person();
p.Name = "John";
p.Age = 30;
p.Speak();
```

---

# 🎁 Summary Table

| OOP Concept   | What It Means      | Keyword Examples                          |
| ------------- | ------------------ | ----------------------------------------- |
| Encapsulation | Hide internal data | `private`, `public`, `get/set`            |
| Abstraction   | Hide complexity    | `abstract`, `interface`                   |
| Inheritance   | Reuse behavior     | `:`                                       |
| Polymorphism  | Many forms         | `virtual`, `override`, method overloading |


