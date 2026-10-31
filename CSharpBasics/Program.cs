using System;

namespace CSharpBasics
{
    class Program   
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


            // Escape characters inside strings

            Console.WriteLine("Line 1\nLine 2"); // \n = new line
            Console.WriteLine("Name:\tArun"); // \t = tab
            Console.WriteLine("She said \"Hi\""); // \" = double quote
            Console.WriteLine("C:\\Users\\Arun"); // \\ = backslash
            Console.WriteLine(@"C:\Users\Arun"); // @ = verbatim string, no escaping needed

            int count;
            count = 10;

            int a = 1, b = 2;

            var city = "New York"; // type inferred from value
            var total = 100;
            var rate = 4.23;
            //var x;


            Console.WriteLine($"City: {city}, Total: {total}, Rate: {rate}");

            // Default values

            int i = default; // default value for int is 0
            bool flag = default; // default value for bool is false
            string? s = default; // default value for string is null


            Console.WriteLine($"i: {i}, flag: {flag}, s: {s}");

            // Type Conversion

            // 1. Implicit (automatic, safe: small type -> big type)
            int small = 100;
            Console.WriteLine($"small: {small}");
            long big = small;
            Console.WriteLine($"big: {big}");
            double d = small;
            Console.WriteLine($"d: {d}");

            //2. Explicit (manual, unsafe: big type -> small type)
            double price = 99.75;
            Console.WriteLine($"price: {price:F2}"); // F2 = 2 decimal places
            int whole = (int)price;
            Console.WriteLine($"whole: {whole}");

            //3. String -> number with parse (crashes on bad input)

            int n1 = int.Parse("123");
            Console.WriteLine($"n1: {n1}");
            double n2 = double.Parse("123.45");
            Console.WriteLine($"n2: {n2}");

            //4. String -> number with tryparse (does not crash on bad input)
            
            if (int.TryParse("123", out int n3))
            {
                Console.WriteLine($"n3: {n3}");
            }
            else
            {
                Console.WriteLine("Invalid input for n3");
            }

        }
    }
}