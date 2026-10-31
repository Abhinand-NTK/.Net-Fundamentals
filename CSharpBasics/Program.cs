using System;

namespace CSharpBasics
{
    class program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            Console.WriteLine("Prints and moves to a new line");
            Console.Write("Prints on the same line ");
            Console.WriteLine(); // empty line
            Console.Write("Enter your name: "); string? name = Console.ReadLine(); // always returns text (string), or null
            Console.WriteLine("Hello, " + name); // concatenation
            Console.WriteLine($"Hello, {name}"); // interpolation (preferred)
            // Console.Clear(); // clears the console
            // Console.ReadKey();
        }
    }
}