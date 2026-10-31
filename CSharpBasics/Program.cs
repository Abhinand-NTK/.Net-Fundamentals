using System;
using System.Linq;
using System.Text;

namespace CSharpBasics
{
    class Program
    {
        static void Main(string[] args)
        {
            int choice;
            do
            {
                Console.WriteLine();
                Console.WriteLine("=== C# Fundamentals ===");
                Console.WriteLine(" 1. Syntax and console I/O");
                Console.WriteLine(" 2. Variables and data types");
                Console.WriteLine(" 3. Operators");
                Console.WriteLine(" 4. Strings");
                Console.WriteLine(" 5. Control flow: decisions");
                Console.WriteLine(" 6. Loops");
                Console.WriteLine(" 7. Methods");
                Console.WriteLine(" 8. Value types vs. reference types");
                Console.WriteLine(" 9. Arrays");
                Console.WriteLine("10. Mini project: Student Marks Manager");
                Console.WriteLine(" 0. Exit");
                Console.Write("Choose a section: ");
                int.TryParse(Console.ReadLine(), out choice);
                Console.WriteLine();

                switch (choice)
                {
                    case 1: SyntaxAndConsole(); break;
                    case 2: VariablesAndTypes(); break;
                    case 3: Operators(); break;
                    case 4: Strings(); break;
                    case 5: Decisions(); break;
                    case 6: Loops(); break;
                    case 7: Methods(); break;
                    case 8: ValueVsReference(); break;
                    case 9: Arrays(); break;
                    case 10: StudentMarksManager(); break;
                    case 0: Console.WriteLine("Bye!"); break;
                    default: Console.WriteLine("Invalid choice."); break;
                }
            } while (choice != 0);
        }

        // ============================================================
        // 2. C# syntax rules + console input and output
        // ============================================================
        static void SyntaxAndConsole()
        {
            // Single-line comment

            /* Multi-line
               comment */

            Console.WriteLine("Hello, World!");

            Console.WriteLine("Prints and moves to a new line");
            Console.Write("Prints on the same line ");
            Console.WriteLine(); // empty line

            Console.Write("Enter your name: ");
            string? name = Console.ReadLine(); // always returns text (string), or null

            Console.WriteLine("Hello, " + name); // concatenation
            Console.WriteLine($"Hello, {name}"); // interpolation (preferred)

            Console.Write("Press any key to clear the screen...");
            Console.ReadKey(); // waits for any key press
            Console.Clear(); // clears the console

            // Escape characters inside strings
            Console.WriteLine("Line 1\nLine 2"); // \n = new line
            Console.WriteLine("Name:\tArun"); // \t = tab
            Console.WriteLine("She said \"Hi\""); // \" = double quote
            Console.WriteLine("C:\\Users\\Arun"); // \\ = backslash
            Console.WriteLine(@"C:\Users\Arun"); // @ = verbatim string, no escaping needed
        }

        // ============================================================
        // 3. Variables and data types
        // ============================================================
        static void VariablesAndTypes()
        {
            // Built-in types you will use most
            int age = 25;
            long views = 9_000_000_000L; // L suffix for long
            double height = 5.8;
            float temp = 36.6f; // f suffix for float
            decimal price = 99.99m; // m suffix for decimal (money)
            bool isActive = true;
            char grade = 'A'; // single quotes
            string name = "Arun"; // double quotes
            DateTime now = DateTime.Now;
            Console.WriteLine($"{age} {views} {height} {temp} {price} {isActive} {grade} {name} {now}");

            // Declaring variables
            int count; // declared, not yet assigned
            count = 10; // assigned later

            int a = 1, b = 2; // several on one line

            var city = "Kochi"; // 'var' = compiler infers the type (string)
            var total = 100; // int
            var rate = 7.5; // double
            // var x; // ERROR: var needs a value to infer from

            const double Pi = 3.14159; // constant: can never change
            // Pi = 3; // ERROR

            Console.WriteLine($"count: {count}, a: {a}, b: {b}");
            Console.WriteLine($"City: {city}, Total: {total}, Rate: {rate}, Pi: {Pi}");

            // Default values
            int i = default; // 0
            bool flag = default; // false
            string? s = default; // null
            Console.WriteLine($"i: {i}, flag: {flag}, s: {s}");

            // Type conversion

            // 1. Implicit (automatic, safe: small type -> big type)
            int small = 100;
            long big = small;
            double d = small;
            Console.WriteLine($"small: {small}, big: {big}, d: {d}");

            // 2. Explicit cast (you force it; data may be lost)
            double unitCost = 99.75;
            int whole = (int)unitCost; // 99 (decimal part is cut, not rounded)
            Console.WriteLine($"whole: {whole}");

            // 3. String -> number with Parse (crashes on bad input)
            int n1 = int.Parse("42");
            double n2 = double.Parse("3.5");
            // int bad = int.Parse("abc"); // FormatException at runtime
            Console.WriteLine($"n1: {n1}, n2: {n2}");

            // 4. String -> number with TryParse (safe, preferred for user input)
            if (int.TryParse("123", out int result))
                Console.WriteLine($"Valid: {result}");
            else
                Console.WriteLine("Invalid number");

            // 5. Convert class
            int n3 = Convert.ToInt32("55");
            bool b1 = Convert.ToBoolean("true");
            Console.WriteLine($"n3: {n3}, b1: {b1}");

            // 6. Anything -> string
            string text = 42.ToString();
            string money = 1234.5m.ToString("C"); // currency format, e.g. ₹1,234.50
            string fixed2 = 3.14159.ToString("F2"); // "3.14"
            Console.WriteLine($"text: {text}, money: {money}, fixed2: {fixed2}");

            // Full example
            Console.Write("Enter product name: ");
            string productName = Console.ReadLine() ?? "";

            Console.Write("Enter price: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal unitPrice))
            {
                Console.WriteLine("Invalid price.");
                return; // stop the method
            }

            Console.Write("Enter quantity: ");
            int quantity = int.TryParse(Console.ReadLine(), out int q) ? q : 1;

            decimal lineTotal = unitPrice * quantity;
            Console.WriteLine($"{quantity} x {productName} = {lineTotal:F2}");
        }

        // ============================================================
        // 4. Operators
        // ============================================================
        static void Operators()
        {
            // Arithmetic
            int a = 17, b = 5;
            Console.WriteLine(a + b); // 22
            Console.WriteLine(a - b); // 12
            Console.WriteLine(a * b); // 85
            Console.WriteLine(a / b); // 3  <- int / int drops the decimal part
            Console.WriteLine(a % b); // 2  <- remainder (modulus)
            Console.WriteLine(17.0 / 5); // 3.4
            Console.WriteLine((double)a / b); // 3.4
            Console.WriteLine(Math.Pow(2, 3)); // 8
            Console.WriteLine(Math.Round(3.456, 2)); // 3.46

            // Assignment and increment
            int x = 10;
            x += 5; // x = x + 5 -> 15
            x -= 3; // 12
            x *= 2; // 24
            x /= 4; // 6
            x %= 4; // 2
            Console.WriteLine($"x: {x}");

            int i = 5;
            int p = i++; // p = 5, then i becomes 6 (post-increment)
            int q = ++i; // i becomes 7, then q = 7 (pre-increment)
            Console.WriteLine($"p: {p}, q: {q}, i: {i}");

            // Comparison (result is always bool)
            int m = 10, n = 20;
            bool r1 = m == n; // false (equal; note two '=' signs)
            bool r2 = m != n; // true (not equal)
            bool r3 = m > n; // false
            bool r4 = m < n; // true
            bool r5 = m >= 10; // true
            bool r6 = m <= 9; // false
            Console.WriteLine($"{r1} {r2} {r3} {r4} {r5} {r6}");

            // Logical
            int age = 25;
            bool hasId = true;

            bool canEnter = age >= 18 && hasId; // AND: both must be true
            bool discount = age < 12 || age > 60; // OR: at least one true
            bool isMinor = !(age >= 18); // NOT: flips the value
            Console.WriteLine($"canEnter: {canEnter}, discount: {discount}, isMinor: {isMinor}");

            // Ternary (conditional) operator
            // condition ? valueIfTrue : valueIfFalse
            string status = age >= 18 ? "Adult" : "Minor";
            Console.WriteLine($"status: {status}");

            // Null operators (very common in real .NET code)
            string? name = null;

            string display = name ?? "Guest"; // ?? : use right side if left is null
            int? length = name?.Length; // ?. : returns null instead of crashing
            name ??= "Default"; // ??= : assign only if currently null
            Console.WriteLine($"display: {display}, length: {length}, name: {name}");
        }

        // ============================================================
        // 5. Strings
        // ============================================================
        static void Strings()
        {
            // Creating and combining
            string first = "Arun";
            string last = "Kumar";

            string full1 = first + " " + last; // concatenation
            string full2 = $"{first} {last}"; // interpolation (preferred)
            string full3 = string.Format("{0} {1}", first, last); // older style
            Console.WriteLine($"{full1} | {full2} | {full3}");

            decimal price = 1499.5m;
            Console.WriteLine($"Price: {price:F2}"); // 1499.50
            Console.WriteLine($"Price: {price:N2}"); // 1,499.50
            Console.WriteLine($"Today: {DateTime.Now:dd-MM-yyyy}");

            string multi = """
                Raw string literal:
                keeps "quotes" and new lines as they are.
                """;
            Console.WriteLine(multi);

            // Most-used string methods
            string[] arr = "a,b,c".Split(',');
            Console.WriteLine("Hello".Length); // 5
            Console.WriteLine("Hello".ToUpper()); // HELLO
            Console.WriteLine(" hi ".Trim()); // hi
            Console.WriteLine("Hello".Contains("ell")); // True
            Console.WriteLine("file.txt".EndsWith(".txt")); // True
            Console.WriteLine("Hello".IndexOf('l')); // 2 (first match, -1 if none)
            Console.WriteLine("Hello".Substring(1, 3)); // ell
            Console.WriteLine("a-b-c".Replace("-", ",")); // a,b,c
            Console.WriteLine(string.Join("-", arr)); // a-b-c
            Console.WriteLine(string.IsNullOrWhiteSpace(" ")); // True
            Console.WriteLine("abc".Equals("ABC", StringComparison.OrdinalIgnoreCase)); // True

            // Indexing and looping over characters
            string word = "Kerala";
            char firstChar = word[0]; // 'K'
            char lastChar = word[^1]; // 'a' (^1 = last)
            string part = word[0..3]; // "Ker" (range: index 0 up to, not including, 3)
            Console.WriteLine($"{firstChar} {lastChar} {part}");

            foreach (char c in word)
                Console.Write(c + " "); // K e r a l a
            Console.WriteLine();

            // Immutability in action
            string s = "hello";
            s.ToUpper(); // result thrown away
            Console.WriteLine(s); // hello

            s = s.ToUpper(); // store the new string
            Console.WriteLine(s); // HELLO

            // StringBuilder for building text in a loop (much faster than += many times)
            var sb = new StringBuilder();
            for (int i = 1; i <= 5; i++)
            {
                sb.Append(i);
                sb.Append(", ");
            }
            sb.AppendLine("done");
            Console.WriteLine(sb.ToString()); // 1, 2, 3, 4, 5, done
        }

        // ============================================================
        // 6. Control flow: decisions
        // ============================================================
        static void Decisions()
        {
            // if / else if / else
            int marks = 72;

            if (marks >= 90)
            {
                Console.WriteLine("Grade A");
            }
            else if (marks >= 75)
            {
                Console.WriteLine("Grade B");
            }
            else if (marks >= 50)
            {
                Console.WriteLine("Grade C");
            }
            else
            {
                Console.WriteLine("Fail");
            }

            // Braces are optional for a single statement, but always using them avoids bugs
            if (marks == 100) Console.WriteLine("Perfect!");

            // Nested if
            bool hasAttendance = true;
            if (marks >= 50)
            {
                if (hasAttendance)
                    Console.WriteLine("Eligible for certificate");
            }

            // Classic switch statement
            int day = 3;

            switch (day)
            {
                case 1:
                    Console.WriteLine("Monday");
                    break; // break is required at the end of each case
                case 2:
                    Console.WriteLine("Tuesday");
                    break;
                case 6:
                case 7: // several cases share one block
                    Console.WriteLine("Weekend");
                    break;
                default: // when nothing matches
                    Console.WriteLine("Midweek");
                    break;
            }

            // switch expression (modern, returns a value)
            // Syntax: result = value switch { pattern => result, ... , _ => fallback };
            string dayName = day switch
            {
                1 => "Monday",
                2 => "Tuesday",
                3 => "Wednesday",
                6 or 7 => "Weekend",
                _ => "Unknown" // _ = default
            };
            Console.WriteLine(dayName);

            // Relational patterns: ranges without if/else chains
            string grade = marks switch
            {
                >= 90 => "A",
                >= 75 => "B",
                >= 50 => "C",
                _ => "F"
            };
            Console.WriteLine($"Grade: {grade}");

            // Combined patterns with 'and' / 'or' / 'not'
            int age = 25;
            string ageGroup = age switch
            {
                < 13 => "Child",
                >= 13 and < 20 => "Teen",
                _ => "Adult"
            };
            Console.WriteLine($"Age group: {ageGroup}");
        }

        // ============================================================
        // 7. Loops
        // ============================================================
        static void Loops()
        {
            // for
            // Syntax: for (start; condition; step) { body }
            for (int i = 1; i <= 5; i++)
            {
                Console.WriteLine($"Number {i}");
            }

            // Counting down, step of 2
            for (int i = 10; i >= 0; i -= 2)
                Console.Write(i + " "); // 10 8 6 4 2 0
            Console.WriteLine();

            // foreach
            // Syntax: foreach (type item in collection) { body }
            string[] cities = { "Kochi", "Chennai", "Bengaluru" };

            foreach (string city in cities)
            {
                Console.WriteLine(city);
            }
            // You cannot change 'city' itself inside foreach; it is read-only.

            // while
            // Syntax: while (condition) { body }
            int balance = 1000;
            int months = 0;
            while (balance > 0)
            {
                balance -= 300;
                months++;
            }
            Console.WriteLine($"Money runs out after {months} months");

            // do-while
            // Syntax: do { body } while (condition);   <- note the semicolon
            int choice;
            do
            {
                Console.WriteLine("1. Say hello  2. Show time  0. Back");
                int.TryParse(Console.ReadLine(), out choice);

                if (choice == 1) Console.WriteLine("Hello!");
                else if (choice == 2) Console.WriteLine(DateTime.Now.ToShortTimeString());
            } while (choice != 0);

            // break and continue
            for (int i = 1; i <= 10; i++)
            {
                if (i == 3) continue; // skip the rest of this round, go to the next i
                if (i == 8) break; // exit the loop completely
                Console.Write(i + " "); // 1 2 4 5 6 7
            }
            Console.WriteLine();

            // Nested loops (multiplication table)
            for (int row = 1; row <= 3; row++)
            {
                for (int col = 1; col <= 5; col++)
                {
                    Console.Write($"{row * col,4}"); // ,4 = right-align in 4 spaces
                }
                Console.WriteLine();
            }
        }

        // ============================================================
        // 8. Methods
        // ============================================================
        static void Methods()
        {
            // Basic methods
            SayHello("Arun");
            int s = Square(4); // 16
            Console.WriteLine(s);
            Console.WriteLine(Cube(3)); // 27
            Console.WriteLine(IsEven(7)); // False

            // Optional and named parameters
            Console.WriteLine(CalculatePrice(1000)); // uses default tax and discount
            Console.WriteLine(CalculatePrice(1000, 0.05m)); // custom tax
            Console.WriteLine(CalculatePrice(1000, discount: 100)); // named argument: skip taxRate

            // Method overloading (same name, different parameters)
            Console.WriteLine(Area(5)); // circle
            Console.WriteLine(Area(4, 6)); // rectangle

            // Returning more than one value
            // 1. out parameters (you saw this in TryParse)
            GetMinMax(new[] { 4, 9, 1 }, out int lo, out int hi);
            Console.WriteLine($"{lo} to {hi}");

            // 2. Tuples (modern and cleaner)
            var result = MinMax(new[] { 4, 9, 1 });
            Console.WriteLine($"{result.Min} to {result.Max}");

            // ref: let a method change the caller's variable
            int x = 5;
            Double(ref x);
            Console.WriteLine(x); // 10

            // params: any number of arguments
            Console.WriteLine(Sum(1, 2, 3, 4)); // 10

            // Recursion (a method that calls itself)
            Console.WriteLine(Factorial(5)); // 120
        }

        //  access   static?  return  name     parameters
        public static int Add(int a, int b)
        {
            return a + b; // 'return' sends a value back and exits
        }

        static void SayHello(string name) // no return value
        {
            Console.WriteLine($"Hello, {name}");
        }

        static int Square(int n)
        {
            return n * n;
        }

        static int Cube(int n) => n * n * n; // expression-bodied: one-line method

        static bool IsEven(int n) => n % 2 == 0;

        static decimal CalculatePrice(decimal amount, decimal taxRate = 0.18m, decimal discount = 0)
        {
            return (amount - discount) * (1 + taxRate);
        }

        static double Area(double radius) => Math.PI * radius * radius;
        static double Area(double length, double width) => length * width;

        static void GetMinMax(int[] nums, out int min, out int max)
        {
            min = nums.Min();
            max = nums.Max();
        }

        static (int Min, int Max) MinMax(int[] nums) => (nums.Min(), nums.Max());

        static void Double(ref int value) => value *= 2;

        static int Sum(params int[] numbers) => numbers.Sum();

        static long Factorial(int n) => n <= 1 ? 1 : n * Factorial(n - 1);

        // ============================================================
        // 9. Value types vs. reference types
        // ============================================================
        static void ValueVsReference()
        {
            // Value type: independent copies
            int a = 10;
            int b = a; // copy of the value
            b = 99;
            Console.WriteLine(a); // 10 (unchanged)

            // Reference type: shared object
            var s1 = new Student { Name = "Arun" };
            var s2 = s1; // copy of the reference, same object
            s2.Name = "Meera";
            Console.WriteLine(s1.Name); // Meera (changed!)

            // struct vs. class side by side
            var p1 = new PointStruct { X = 1 };
            var p2 = p1;
            p2.X = 50;
            Console.WriteLine(p1.X); // 1  (struct = value type)

            var c1 = new PointClass { X = 1 };
            var c2 = c1;
            c2.X = 50;
            Console.WriteLine(c1.X); // 50 (class = reference type)

            // Passing to methods works the same way
            int num = 5;
            ChangeNumber(num);
            Console.WriteLine(num); // 5 (method got a copy)

            var stu = new Student { Name = "Arun" };
            ChangeStudent(stu);
            Console.WriteLine(stu.Name); // Changed (method got the same object)

            // null and nullable types
            Student? nobody = null; // ? = this reference may be null
            // Console.WriteLine(nobody.Name); // NullReferenceException at runtime
            Console.WriteLine(nobody?.Name ?? "No student"); // safe

            int? score = null; // nullable value type
            if (score.HasValue)
                Console.WriteLine(score.Value);
            int finalScore = score ?? 0; // 0 when null
            Console.WriteLine(finalScore);

            // string is special: reference type but immutable, so it behaves like a value
            string t1 = "old";
            string t2 = t1;
            t2 = "new";
            Console.WriteLine(t1); // old

            // enum (a value type for named constants)
            OrderStatus status = OrderStatus.Shipped;
            if (status == OrderStatus.Shipped)
                Console.WriteLine("On the way");
            Console.WriteLine((int)status); // 2
        }

        static void ChangeNumber(int n) => n = 100;
        static void ChangeStudent(Student st) => st.Name = "Changed";

        // ============================================================
        // 10. Arrays
        // ============================================================
        static void Arrays()
        {
            // Declaring and initializing
            int[] marks = new int[5]; // 5 slots, all 0
            int[] scores = new int[] { 90, 75, 60 }; // with values
            int[] ages = { 21, 25, 30 }; // short form
            string[] names = ["Arun", "Meera"]; // collection expression (C# 12+)
            Console.WriteLine($"{ages.Length} ages, {names.Length} names");

            // Reading and writing items
            marks[0] = 85; // first item
            marks[4] = 70; // last item (Length - 1)
            Console.WriteLine(marks[0]); // 85
            Console.WriteLine(marks.Length); // 5
            Console.WriteLine(scores[^1]); // 60 (last item)
            // marks[5] = 10; // IndexOutOfRangeException

            // Looping
            // with index
            for (int i = 0; i < scores.Length; i++)
                Console.WriteLine($"Item {i}: {scores[i]}");

            // without index
            foreach (int s in scores)
                Console.WriteLine(s);

            // Useful built-in operations
            int[] nums = { 5, 2, 9, 1, 7 };

            Array.Sort(nums); // 1 2 5 7 9 (changes the array)
            Array.Reverse(nums); // 9 7 5 2 1
            int pos = Array.IndexOf(nums, 5); // 2
            bool has = nums.Contains(9); // true
            Console.WriteLine($"pos: {pos}, has: {has}");

            Console.WriteLine(nums.Sum()); // 24
            Console.WriteLine(nums.Max()); // 9
            Console.WriteLine(nums.Min()); // 1
            Console.WriteLine(nums.Average()); // 4.8
            Console.WriteLine(string.Join(", ", nums)); // print all items

            // Finding the largest value by hand (a classic exercise)
            int[] values = { 12, 45, 7, 89, 23 };
            int largest = values[0];
            foreach (int v in values)
            {
                if (v > largest)
                    largest = v;
            }
            Console.WriteLine($"Largest: {largest}"); // 89

            // Two-dimensional arrays (rows and columns)
            int[,] grid = new int[2, 3]
            {
                { 1, 2, 3 },
                { 4, 5, 6 }
            };
            Console.WriteLine(grid[1, 2]); // 6 (row 1, column 2)

            for (int r = 0; r < grid.GetLength(0); r++) // rows
            {
                for (int c = 0; c < grid.GetLength(1); c++) // columns
                    Console.Write(grid[r, c] + " ");
                Console.WriteLine();
            }

            // Arrays are reference types
            int[] original = { 1, 2, 3 };
            int[] alias = original; // same array
            alias[0] = 100;
            Console.WriteLine(original[0]); // 100

            int[] copy = (int[])original.Clone(); // a real, separate copy
            copy[0] = 1;
            Console.WriteLine(original[0]); // still 100
        }

        // ============================================================
        // 11. Mini project: Student Marks Manager
        // ============================================================
        static void StudentMarksManager()
        {
            Console.WriteLine("=== Student Marks Manager ===");

            int studentCount = ReadInt("How many students? ", 1, 50);
            string[] subjects = { "Maths", "Science", "English" };

            string[] names = new string[studentCount];
            int[,] marks = new int[studentCount, subjects.Length];

            // 1. Input
            for (int i = 0; i < studentCount; i++)
            {
                Console.WriteLine($"\nStudent {i + 1}");
                Console.Write("Name: ");
                string? input = Console.ReadLine();
                names[i] = string.IsNullOrWhiteSpace(input) ? $"Student{i + 1}" : input.Trim();

                for (int s = 0; s < subjects.Length; s++)
                    marks[i, s] = ReadInt($"{subjects[s]} marks (0-100): ", 0, 100);
            }

            // 2. Report
            Console.WriteLine();
            Console.WriteLine($"{"Name",-15}{"Total",8}{"Average",10}{"Grade",8}");
            Console.WriteLine(new string('-', 41));

            int topperIndex = 0;
            double topAverage = -1;

            for (int i = 0; i < studentCount; i++)
            {
                int total = 0;
                for (int s = 0; s < subjects.Length; s++)
                    total += marks[i, s];

                double average = CalculateAverage(total, subjects.Length);
                Console.WriteLine($"{names[i],-15}{total,8}{average,10:F2}{GetGrade(average),8}");

                if (average > topAverage)
                {
                    topAverage = average;
                    topperIndex = i;
                }
            }

            Console.WriteLine(new string('-', 41));
            Console.WriteLine($"Class topper: {names[topperIndex]} ({topAverage:F2})");
        }

        static int ReadInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int value) && value >= min && value <= max)
                    return value;
                Console.WriteLine($"Please enter a whole number between {min} and {max}.");
            }
        }

        static double CalculateAverage(int total, int count) => (double)total / count;

        static string GetGrade(double average) => average switch
        {
            >= 90 => "A",
            >= 75 => "B",
            >= 50 => "C",
            _ => "F"
        };
    }

    // Types used in section 9
    class Student
    {
        public string Name { get; set; } = "";
    }

    struct PointStruct { public int X; }
    class PointClass { public int X; }

    enum OrderStatus { Pending, Paid, Shipped, Delivered } // 0, 1, 2, 3
}
