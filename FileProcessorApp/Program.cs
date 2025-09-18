using FileProcessorLib;
using FileProcessorApp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // Set up configuration from appsettings.json and secrets
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddUserSecrets<Program>() // For developer secrets
                .AddEnvironmentVariables() // For container/production secrets
                .Build();

            // Set up dependency injection
            var serviceProvider = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder.AddConfiguration(configuration.GetSection("Logging"));
                    builder.AddConsole();
                    builder.AddDebug();
                })
                .AddSingleton<FileProcessor>()
                .AddSingleton(sp =>
                {
                    // Configure PrimeChecker from settings
                    var config = new PrimeCheckerConfig();
                    configuration.GetSection("PrimeChecker").Bind(config);
                    return new PrimeChecker(config, sp.GetService<ILogger<PrimeChecker>>());
                })
                .BuildServiceProvider();

            // Get logger instance
            var logger = serviceProvider.GetService<ILogger<Program>>();
            logger?.LogInformation("FileProcessorApp is starting up");

            // Get services
            var processor = serviceProvider.GetRequiredService<FileProcessor>();
            var primeChecker = serviceProvider.GetRequiredService<PrimeChecker>();

            // Example: Check if a number is prime
            int numberToCheck = GetNumberToCheck(args, configuration);
            logger?.LogInformation("Checking if {Number} is prime...", numberToCheck);

            try
            {
                bool isPrime = await primeChecker.IsPrimeAsync(numberToCheck);
                Console.WriteLine($"The number {numberToCheck} is {(isPrime ? "prime" : "not prime")}.");
                logger?.LogInformation("Prime check complete. Number {Number} is {IsPrime}", numberToCheck, isPrime);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                logger?.LogError(ex, "Number out of range: {Number}", numberToCheck);
            }

            Console.WriteLine("FileProcessorApp has completed.");
            logger?.LogInformation("FileProcessorApp is shutting down");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unhandled exception: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Environment.ExitCode = 1;
        }
    }

    private static int GetNumberToCheck(string[] args, IConfiguration configuration)
    {
        // First priority: Command line argument
        if (args.Length > 0 && int.TryParse(args[0], out int number))
        {
            return number;
        }

        // Second priority: Configuration
        if (int.TryParse(configuration["PrimeChecker:DefaultNumber"], out number))
        {
            return number;
        }

        // Third priority: Default value
        return 97; // A prime number as default
    }
}
