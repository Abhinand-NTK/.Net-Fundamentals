using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedLibrary;

namespace DotNetProjectBasics
{
    class Program
    {
        static string[] startupArgs = [];

        static void Main(string[] args)
        {
            startupArgs = args;

            int choice;
            do
            {
                Console.WriteLine();
                Console.WriteLine("=== .NET Project Fundamentals ===");
                Console.WriteLine(" 1. What is .NET? (SDK, runtime, CLR, IL)");
                Console.WriteLine(" 2. The project file (.csproj)");
                Console.WriteLine(" 3. The entry point: Main and command-line args");
                Console.WriteLine(" 4. Namespaces and using directives");
                Console.WriteLine(" 5. Build output: bin, obj, Debug vs. Release");
                Console.WriteLine(" 6. NuGet packages");
                Console.WriteLine(" 7. Project references and class libraries");
                Console.WriteLine(" 8. Configuration: appsettings.json, env vars, args");
                Console.WriteLine(" 9. Dependency injection");
                Console.WriteLine("10. Logging");
                Console.WriteLine("11. Mini project: everything wired together");
                Console.WriteLine(" 0. Exit");
                Console.Write("Choose a section: ");
                int.TryParse(Console.ReadLine(), out choice);
                Console.WriteLine();

                switch (choice)
                {
                    case 1: WhatIsDotNet(); break;
                    case 2: ProjectFile(); break;
                    case 3: EntryPoint(); break;
                    case 4: NamespacesAndUsings(); break;
                    case 5: BuildOutput(); break;
                    case 6: NuGetPackages(); break;
                    case 7: ProjectReferences(); break;
                    case 8: Configuration(); break;
                    case 9: DependencyInjection(); break;
                    case 10: Logging(); break;
                    case 11: MiniProject(); break;
                    case 0: Console.WriteLine("Bye!"); break;
                    default: Console.WriteLine("Invalid choice."); break;
                }
            } while (choice != 0);
        }

        // ============================================================
        // 1. What is .NET?
        // ============================================================
        static void WhatIsDotNet()
        {
            // .NET          = the platform (runtime + libraries + tools). ".NET Core" was renamed
            //                 to just ".NET" from version 5 onwards; this project uses .NET 10.
            // .NET SDK      = tools for DEVELOPERS: compiler, 'dotnet build', 'dotnet new', ...
            // .NET Runtime  = what USERS need to RUN an app. The SDK includes a runtime.
            // CLR           = Common Language Runtime: runs your code, manages memory (garbage collector).
            // BCL           = Base Class Library: Console, List<T>, File, DateTime, ...
            //
            // How your code runs:
            //   Program.cs --(C# compiler)--> IL code inside a .dll --(JIT compiler at runtime)--> machine code
            //   IL = Intermediate Language: CPU-independent, so the same .dll runs on Windows, Linux and macOS.
            //
            // Useful terminal commands:
            //   dotnet --version        SDK version used in this folder
            //   dotnet --list-sdks      all installed SDKs
            //   dotnet --list-runtimes  all installed runtimes
            //   dotnet --info           everything above + OS details

            Console.WriteLine($".NET version:     {Environment.Version}");
            Console.WriteLine($"Framework:        {RuntimeInformation.FrameworkDescription}");
            Console.WriteLine($"OS:               {RuntimeInformation.OSDescription}");
            Console.WriteLine($"CPU architecture: {RuntimeInformation.ProcessArchitecture}");
            Console.WriteLine($"64-bit process:   {Environment.Is64BitProcess}");
            Console.WriteLine($"Processor count:  {Environment.ProcessorCount}");
            Console.WriteLine($"Machine name:     {Environment.MachineName}");
        }

        // ============================================================
        // 2. The project file (.csproj)
        // ============================================================
        static void ProjectFile()
        {
            // Open DotNetProjectBasics.csproj next to this file - every line is commented.
            // Key ideas:
            //   - Every project has exactly one .csproj. Its folder is the "project folder".
            //   - All .cs files in the project folder (and sub-folders) are compiled automatically.
            //   - <TargetFramework> picks the .NET version: net8.0, net9.0, net10.0 ...
            //   - <OutputType>Exe</OutputType> makes a runnable app; without it you get a library (.dll).
            //   - <PackageReference> = NuGet package, <ProjectReference> = another project.
            //
            // A solution file (.slnx or .sln) groups several projects so you can build them together:
            //   dotnet new sln -n Fundamentals            create a solution
            //   dotnet sln add CSharpBasics                add a project to it
            //   dotnet build Fundamentals.slnx             build every project in it

            // The values from the .csproj end up inside the compiled assembly. We can read them back:
            Assembly assembly = Assembly.GetExecutingAssembly();
            AssemblyName info = assembly.GetName();

            Console.WriteLine($"Assembly name:  {info.Name}"); // <AssemblyName>
            Console.WriteLine($"Version:        {info.Version}"); // <Version>
            Console.WriteLine($"File location:  {assembly.Location}");

            string? product = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            string? company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company; // <Authors>
            string? framework = assembly.GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>()?.FrameworkName;
            Console.WriteLine($"Info version:   {product}");
            Console.WriteLine($"Company:        {company}");
            Console.WriteLine($"Target:         {framework}"); // <TargetFramework>
        }

        // ============================================================
        // 3. The entry point: Main and command-line args
        // ============================================================
        static void EntryPoint()
        {
            // Every console app starts at ONE method: static void Main(string[] args).
            // Allowed forms:
            //   static void Main()                  static int Main(string[] args)   <- returns an exit code
            //   static void Main(string[] args)     static async Task Main(string[] args)
            //
            // Top-level statements (what 'dotnet new console' gives you):
            //   A Program.cs with NO class and NO Main, just code:
            //       Console.WriteLine("Hello");
            //   The compiler generates the Program class and Main for you. 'args' is still available.
            //   Only ONE file in a project may use top-level statements.
            //
            // Passing arguments (everything after '--' goes to your app, not to dotnet):
            //   dotnet run -- hello 42 --Store:Name=Test
            //
            // In Visual Studio / VS Code you can set them in Properties/launchSettings.json:
            //   "commandLineArgs": "hello 42"

            Console.WriteLine($"Number of args: {startupArgs.Length}");
            for (int i = 0; i < startupArgs.Length; i++)
                Console.WriteLine($"args[{i}] = {startupArgs[i]}");

            if (startupArgs.Length == 0)
                Console.WriteLine("No args. Try: dotnet run --project DotNetProjectBasics -- hello 42");

            // Exit codes: 0 = success, anything else = error. Scripts and CI pipelines check this.
            //   Environment.Exit(1);     // ends the app immediately with exit code 1
            //   return 1;                // from 'static int Main' does the same, more cleanly
            // Check it after running:  PowerShell -> $LASTEXITCODE    bash -> echo $?

            Console.WriteLine($"Current directory: {Environment.CurrentDirectory}");
            Console.WriteLine($"Process id:        {Environment.ProcessId}");
            Console.WriteLine($"Command line:      {Environment.CommandLine}");
        }

        // ============================================================
        // 4. Namespaces and using directives
        // ============================================================
        static void NamespacesAndUsings()
        {
            // A namespace groups related types and avoids name clashes:
            //   SharedLibrary.PriceCalculator vs. SomeOtherLibrary.PriceCalculator
            // Convention: namespace = project name + folder path, e.g. DotNetProjectBasics.Services
            //
            // Two ways to declare one:
            //   namespace DotNetProjectBasics { ... }   block-scoped (used in this file)
            //   namespace DotNetProjectBasics;          file-scoped (C# 10+, one less indent level)

            // Without 'using', you write the full name:
            System.Text.StringBuilder sb1 = new System.Text.StringBuilder();

            // With 'using System.Text;' at the top you could write: StringBuilder sb2 = new();
            sb1.Append("Full names work without any using");
            Console.WriteLine(sb1);

            // Where do Console, List<T>, File etc. come from with no 'using' at the top?
            // <ImplicitUsings>enable</ImplicitUsings> in the .csproj adds these to every file:
            //   System, System.IO, System.Collections.Generic, System.Linq,
            //   System.Net.Http, System.Threading, System.Threading.Tasks
            // You can see them in obj/Debug/net10.0/DotNetProjectBasics.GlobalUsings.g.cs
            //
            // <Using Include="System.Reflection" /> in the .csproj is OUR global using -
            // that is why 'Assembly' works in section 2 without 'using System.Reflection;'.
            // Same thing in code:  global using System.Reflection;  (usually in a GlobalUsings.cs file)

            // Alias: give a long name a short one
            //   using Calc = SharedLibrary.PriceCalculator;

            // Static using: call static members without the class name
            //   using static System.Math;   ->   Sqrt(16) instead of Math.Sqrt(16)

            Console.WriteLine($"This class lives in: {typeof(Program).Namespace}");
            Console.WriteLine($"Full type name:      {typeof(Program).FullName}");
            Console.WriteLine($"PriceCalculator:     {typeof(PriceCalculator).FullName}");
            Console.WriteLine($"List<int>:           {typeof(List<int>).Namespace}");
        }

        // ============================================================
        // 5. Build output: bin, obj, Debug vs. Release
        // ============================================================
        static void BuildOutput()
        {
            // dotnet restore   downloads NuGet packages (runs automatically before build)
            // dotnet build     compiles to bin/Debug/net10.0/
            // dotnet run       build + run
            // dotnet clean     deletes build output
            // dotnet watch     re-runs automatically when you save a file
            //
            // obj/  = temporary files the build uses (generated code, package lists). Never edit.
            // bin/  = the final result: your .dll, .exe, dependencies, appsettings.json.
            // Both are in .gitignore - they are rebuilt from source, never committed.
            //
            // What is in bin/Debug/net10.0/ ?
            //   DotNetProjectBasics.dll               your compiled code (IL)
            //   DotNetProjectBasics.exe               small Windows launcher for the .dll
            //   DotNetProjectBasics.pdb               debug symbols (line numbers in stack traces)
            //   DotNetProjectBasics.deps.json         list of dependencies
            //   DotNetProjectBasics.runtimeconfig.json which runtime version to start
            //   SharedLibrary.dll, Microsoft.*.dll    referenced projects and packages
            //
            // Debug vs. Release (configurations):
            //   Debug   (default) - no optimizations, easy to step through in the debugger
            //   Release           - optimized, used for deployment:  dotnet build -c Release
            //
            // Publishing (preparing the app to copy to another machine):
            //   dotnet publish -c Release                              needs .NET installed there
            //   dotnet publish -c Release -r win-x64 --self-contained   bundles the runtime too
            //   dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true   one .exe file

#if DEBUG
            Console.WriteLine("Built in DEBUG configuration");
#else
            Console.WriteLine("Built in RELEASE configuration");
#endif
            // #if is a compiler directive: the other branch is not even compiled.
            // DEBUG is defined automatically in Debug builds.

            Console.WriteLine($"Debugger attached: {Debugger.IsAttached}");

            string baseDir = AppContext.BaseDirectory; // the bin folder the app runs from
            Console.WriteLine($"Running from: {baseDir}");
            Console.WriteLine("Files in the output folder:");
            foreach (string file in Directory.GetFiles(baseDir).Select(Path.GetFileName).Order()!)
                Console.WriteLine($"  {file}");
        }

        // ============================================================
        // 6. NuGet packages
        // ============================================================
        static void NuGetPackages()
        {
            // NuGet = the package manager for .NET (like npm for JavaScript). Packages live on nuget.org.
            //
            //   dotnet add package Newtonsoft.Json                  add the latest version
            //   dotnet add package Newtonsoft.Json --version 13.0.3 add a specific version
            //   dotnet remove package Newtonsoft.Json
            //   dotnet list package                                 what this project uses
            //   dotnet list package --outdated                      what can be updated
            //
            // 'dotnet add package' simply adds a <PackageReference> line to the .csproj.
            // Downloaded packages are cached in %USERPROFILE%\.nuget\packages (shared by all projects).
            //
            // Packages can depend on other packages ("transitive dependencies").
            // Restore downloads the whole tree; the list is written to obj/project.assets.json.

            // This project uses the Microsoft.Extensions.* packages (see the .csproj).
            // Let's list every assembly loaded into this app right now:
            var loaded = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetName())
                .OrderBy(n => n.Name);

            foreach (AssemblyName name in loaded)
                Console.WriteLine($"  {name.Name,-55} {name.Version}");

            Console.WriteLine("Run section 8 or 9 first, then come back: more Microsoft.Extensions.* appear,");
            Console.WriteLine("because .NET loads an assembly only when the code first uses it.");
        }

        // ============================================================
        // 7. Project references and class libraries
        // ============================================================
        static void ProjectReferences()
        {
            // Real apps are split into several projects, e.g.:
            //   MyApp.Api        (web project, Exe)
            //   MyApp.Core       (business logic, Library)
            //   MyApp.Data       (database code, Library)
            //   MyApp.Tests      (unit tests)
            //
            // SharedLibrary is a class library in this repo. Created with:
            //   dotnet new classlib -n SharedLibrary
            //   dotnet add DotNetProjectBasics reference SharedLibrary
            // The second command added <ProjectReference> to DotNetProjectBasics.csproj.
            //
            // Rules:
            //   - Only 'public' types are visible to other projects ('internal' is the default for classes).
            //   - References go ONE way. SharedLibrary cannot also reference DotNetProjectBasics (circular).
            //   - Building DotNetProjectBasics builds SharedLibrary first, automatically.

            var calculator = new PriceCalculator(); // class from the other project
            decimal price = 1000m;
            decimal discounted = calculator.ApplyDiscount(price, 10);
            decimal final = calculator.AddTax(discounted, 0.18m);

            Console.WriteLine($"Price:            {price:F2}");
            Console.WriteLine($"After 10% off:    {discounted:F2}");
            Console.WriteLine($"After 18% tax:    {final:F2}");
            Console.WriteLine($"Came from:        {typeof(PriceCalculator).Assembly.GetName().Name}.dll");
        }

        // ============================================================
        // 8. Configuration
        // ============================================================
        static void Configuration()
        {
            // Never hard-code settings (tax rates, connection strings, API URLs) in C# code.
            // Put them in configuration so they can change WITHOUT recompiling.
            //
            // Sources are layered; a LATER source overrides an earlier one with the same key:
            //   1. appsettings.json                     defaults for every environment
            //   2. appsettings.{Environment}.json       e.g. appsettings.Development.json
            //   3. Environment variables                Store__Name=X   ('__' means ':')
            //   4. Command-line args                    --Store:Name=X
            //
            // The environment name comes from the DOTNET_ENVIRONMENT variable
            // (ASPNETCORE_ENVIRONMENT in web apps). Default is "Production".
            //   PowerShell:  $env:DOTNET_ENVIRONMENT = "Development"; dotnet run --project DotNetProjectBasics
            //
            // Secrets (passwords, API keys) do NOT belong in appsettings.json (it is committed to git).
            // In development use:  dotnet user-secrets set "ApiKey" "abc123"

            IConfiguration config = BuildConfiguration();
            string environment = GetEnvironmentName();
            Console.WriteLine($"Environment: {environment}");

            // 1. Read single values by key. ':' walks into nested sections.
            string? appName = config["AppName"];
            string? storeName = config["Store:Name"];
            Console.WriteLine($"AppName:    {appName}");
            Console.WriteLine($"Store:Name: {storeName}"); // has (DEV) when environment is Development

            // 2. Read typed values (needs the Configuration.Binder package)
            decimal taxRate = config.GetValue<decimal>("Store:TaxRate");
            int maxItems = config.GetValue("Store:MaxItemsPerOrder", defaultValue: 5);
            Console.WriteLine($"TaxRate: {taxRate}, MaxItems: {maxItems}");

            // 3. Bind a whole section to a class (the cleanest way)
            StoreSettings store = config.GetSection("Store").Get<StoreSettings>() ?? new StoreSettings();
            Console.WriteLine($"Bound object -> Name: {store.Name}, TaxRate: {store.TaxRate}, MaxItems: {store.MaxItemsPerOrder}");

            // Missing keys return null, not an exception
            Console.WriteLine($"Missing key: {config["Does:Not:Exist"] ?? "(null)"}");

            Console.WriteLine();
            Console.WriteLine("Try overriding from the command line:");
            Console.WriteLine("  dotnet run --project DotNetProjectBasics -- --Store:Name=FromArgs");
        }

        static string GetEnvironmentName() =>
            Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

        static IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory) // look for json files in bin/
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{GetEnvironmentName()}.json", optional: true)
                .AddEnvironmentVariables()
                .AddCommandLine(startupArgs)
                .Build();
        }

        // ============================================================
        // 9. Dependency injection
        // ============================================================
        static void DependencyInjection()
        {
            // Problem: if a class does 'new EmailSender()' inside itself, it is glued to that one class.
            // You cannot swap it (e.g. for an SmsSender) or replace it with a fake in a unit test.
            //
            // Dependency Injection (DI): a class ASKS for what it needs in its constructor,
            // usually as an interface, and a "container" creates and passes it in.
            //
            // 1. Register services:  services.AddSingleton<IInterface, Implementation>();
            // 2. Build the provider:  services.BuildServiceProvider();
            // 3. Ask for a service:   provider.GetRequiredService<IInterface>();
            //
            // Lifetimes:
            //   Singleton - ONE instance for the whole app
            //   Scoped    - one instance per scope (in web apps: per HTTP request)
            //   Transient - a NEW instance every time it is requested
            //
            // ASP.NET Core is built entirely on this. You will see it in every Program.cs.

            var services = new ServiceCollection();
            services.AddSingleton<INotifier, ConsoleNotifier>(); // swap ConsoleNotifier for another class here, nothing else changes
            services.AddTransient<OrderService>();
            services.AddSingleton<CounterService>();
            services.AddTransient<GuidService>();

            using ServiceProvider provider = services.BuildServiceProvider();

            // The container sees OrderService needs an INotifier and passes ConsoleNotifier in for us
            var orders = provider.GetRequiredService<OrderService>();
            orders.PlaceOrder("Laptop");

            // Singleton: same object both times, so the count keeps going up
            provider.GetRequiredService<CounterService>().Increment();
            provider.GetRequiredService<CounterService>().Increment();
            Console.WriteLine($"Singleton counter: {provider.GetRequiredService<CounterService>().Count}"); // 2

            // Transient: a new object each time, so the ids differ
            Console.WriteLine($"Transient id 1: {provider.GetRequiredService<GuidService>().Id}");
            Console.WriteLine($"Transient id 2: {provider.GetRequiredService<GuidService>().Id}");

            // Scoped: same object inside one scope, a different one in the next scope
            var scopedServices = new ServiceCollection();
            scopedServices.AddScoped<GuidService>();
            using ServiceProvider scopedProvider = scopedServices.BuildServiceProvider();

            using (IServiceScope scope1 = scopedProvider.CreateScope())
            {
                Console.WriteLine($"Scope 1, first:  {scope1.ServiceProvider.GetRequiredService<GuidService>().Id}");
                Console.WriteLine($"Scope 1, second: {scope1.ServiceProvider.GetRequiredService<GuidService>().Id}"); // same
            }
            using (IServiceScope scope2 = scopedProvider.CreateScope())
            {
                Console.WriteLine($"Scope 2:         {scope2.ServiceProvider.GetRequiredService<GuidService>().Id}"); // different
            }
        }

        // ============================================================
        // 10. Logging
        // ============================================================
        static void Logging()
        {
            // Console.WriteLine is fine for learning. Real apps use ILogger because:
            //   - every message has a LEVEL, and you choose the minimum level in configuration
            //   - output can go to console, files, Azure, Seq ... without changing your code
            //   - messages are "structured": {Item} is stored as a named value, not just text
            //
            // Levels, lowest to highest:
            //   Trace < Debug < Information < Warning < Error < Critical
            // Logging:LogLevel:Default in appsettings.json sets the minimum. Anything below is skipped.
            // appsettings.Development.json lowers it to Debug, so you see more while developing.

            IConfiguration config = BuildConfiguration();

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConfiguration(config.GetSection("Logging"));
                builder.AddSimpleConsole(options => options.SingleLine = true);
            });

            ILogger logger = loggerFactory.CreateLogger<Program>(); // category = class name

            logger.LogTrace("Trace: very detailed, almost never on");
            logger.LogDebug("Debug: shown only in Development");
            logger.LogInformation("Order placed for {Item} at {Price}", "Laptop", 55000); // placeholders, NOT $"..."
            logger.LogWarning("Stock is low: {Count} left", 2);

            try
            {
                int zero = 0;
                Console.WriteLine(10 / zero);
            }
            catch (DivideByZeroException ex)
            {
                logger.LogError(ex, "Calculation failed"); // pass the exception first
            }

            logger.LogCritical("Critical: the app cannot continue");

            // The console logger writes on a background thread; disposing the factory flushes it.
        }

        // ============================================================
        // 11. Mini project: everything wired together
        // ============================================================
        static void MiniProject()
        {
            // This is roughly what the "Generic Host" (Host.CreateApplicationBuilder) does for you
            // in real console, worker and web apps: configuration + logging + DI in one place.

            // 1. Configuration
            IConfiguration config = BuildConfiguration();

            // 2. Register services
            var services = new ServiceCollection();
            services.AddSingleton(config);
            services.AddLogging(builder =>
            {
                builder.AddConfiguration(config.GetSection("Logging"));
                builder.AddSimpleConsole(options => options.SingleLine = true);
            });
            services.AddSingleton(config.GetSection("Store").Get<StoreSettings>() ?? new StoreSettings());
            services.AddSingleton<PriceCalculator>(); // from the SharedLibrary project
            services.AddTransient<CheckoutService>();

            // 3. Build and run
            using ServiceProvider provider = services.BuildServiceProvider();
            var checkout = provider.GetRequiredService<CheckoutService>();

            Console.Write("Item name: ");
            string item = Console.ReadLine() ?? "";
            Console.Write("Price: ");
            decimal.TryParse(Console.ReadLine(), out decimal price);
            Console.Write("Quantity: ");
            int.TryParse(Console.ReadLine(), out int quantity);

            checkout.Checkout(item, price, quantity);
        }
    }

    // ============================================================
    // Types used in sections 8-11
    // (in a real project each would be in its own file, e.g. Services/OrderService.cs)
    // ============================================================

    // Section 8: settings class. Property names must match the json keys.
    class StoreSettings
    {
        public string Name { get; set; } = "";
        public decimal TaxRate { get; set; }
        public int MaxItemsPerOrder { get; set; }
    }

    // Section 9: an interface and an implementation
    interface INotifier
    {
        void Send(string message);
    }

    class ConsoleNotifier : INotifier
    {
        public void Send(string message) => Console.WriteLine($"[Notification] {message}");
    }

    class OrderService
    {
        private readonly INotifier _notifier;

        // Constructor injection: OrderService only knows the interface, not the concrete class
        public OrderService(INotifier notifier)
        {
            _notifier = notifier;
        }

        public void PlaceOrder(string item) => _notifier.Send($"Order placed: {item}");
    }

    class CounterService
    {
        public int Count { get; private set; }
        public void Increment() => Count++;
    }

    class GuidService
    {
        public Guid Id { get; } = Guid.NewGuid();
    }

    // Section 11: a service that uses config, logging and the class library together
    // (primary constructor syntax, C# 12+: parameters are available in the whole class)
    class CheckoutService(StoreSettings store, PriceCalculator calculator, ILogger<CheckoutService> logger)
    {
        public void Checkout(string item, decimal price, int quantity)
        {
            if (string.IsNullOrWhiteSpace(item) || price <= 0 || quantity <= 0)
            {
                logger.LogWarning("Invalid order: item={Item}, price={Price}, quantity={Quantity}", item, price, quantity);
                return;
            }

            if (quantity > store.MaxItemsPerOrder)
            {
                logger.LogWarning("Quantity {Quantity} is over the limit of {Max}", quantity, store.MaxItemsPerOrder);
                return;
            }

            decimal subtotal = price * quantity;
            decimal total = calculator.AddTax(subtotal, store.TaxRate);

            logger.LogDebug("Subtotal {Subtotal}, tax rate {TaxRate}", subtotal, store.TaxRate);
            Console.WriteLine($"--- {store.Name} ---");
            Console.WriteLine($"{quantity} x {item} @ {price:F2} = {subtotal:F2}");
            Console.WriteLine($"Tax ({store.TaxRate:P0}):  {total - subtotal:F2}");
            Console.WriteLine($"Total:      {total:F2}");
            logger.LogInformation("Checkout complete for {Item}, total {Total}", item, total);
        }
    }
}
