using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FileProcessorApp
{
    /// <summary>
    /// Provides functionality to check if a number is prime.
    /// Includes advanced options for performance tuning and logging.
    /// </summary>
    public class PrimeChecker
    {
        private readonly ILogger<PrimeChecker>? _logger;
        private readonly PrimeCheckerConfig _config;

        // Cache for previously calculated primes to improve performance
        private static readonly Dictionary<int, bool> _primeCache = new Dictionary<int, bool>();

        // Well-known small primes for quick checking
        private static readonly HashSet<int> _knownPrimes = new HashSet<int>
        {
            2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41,
            43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97
        };

        /// <summary>
        /// Creates a new instance of the PrimeChecker with default configuration
        /// </summary>
        public PrimeChecker() : this(new PrimeCheckerConfig(), null) { }

        /// <summary>
        /// Creates a new instance of the PrimeChecker with custom configuration
        /// </summary>
        /// <param name="config">Configuration for the prime checker</param>
        /// <param name="logger">Optional logger for operations</param>
        public PrimeChecker(PrimeCheckerConfig config, ILogger<PrimeChecker>? logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger;
        }

        /// <summary>
        /// Determines whether the specified number is prime.
        /// </summary>
        /// <param name="number">The number to check.</param>
        /// <returns>True if the number is prime; otherwise, false.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when number is extremely large and exceeds configured limits.</exception>
        public bool IsPrime(int number)
        {
            try
            {
                _logger?.LogDebug("Checking if {Number} is prime", number);

                // For very large numbers, enforce safety limits
                if (_config.EnableSafetyLimits && number > _config.MaxNumberToCheck)
                {
                    _logger?.LogWarning("Number {Number} exceeds maximum safe limit of {MaxLimit}",
                        number, _config.MaxNumberToCheck);
                    throw new ArgumentOutOfRangeException(nameof(number),
                        $"Number {number} exceeds the maximum safe limit of {_config.MaxNumberToCheck}");
                }

                // Check for negative numbers, 0, and 1 - not prime by definition
                if (number <= 1)
                {
                    _logger?.LogDebug("Number {Number} is <= 1, not prime", number);
                    return false;
                }

                // Check the cache if enabled
                if (_config.EnableCaching)
                {
                    lock (_primeCache)
                    {
                        if (_primeCache.TryGetValue(number, out bool isPrime))
                        {
                            _logger?.LogDebug("Cache hit for {Number}, result: {IsPrime}", number, isPrime);
                            return isPrime;
                        }
                    }
                }

                // Check against known small primes
                if (_knownPrimes.Contains(number))
                {
                    _logger?.LogDebug("Number {Number} is in the known primes list", number);
                    CacheResult(number, true);
                    return true;
                }

                // Quick check for even numbers (except 2, handled above)
                if (number % 2 == 0)
                {
                    _logger?.LogDebug("Number {Number} is even and > 2, not prime", number);
                    CacheResult(number, false);
                    return false;
                }

                // Optimization: Only need to check up to the square root
                int boundary = (int)Math.Sqrt(number);

                // Check odd divisors from 3 to sqrt(number)
                for (int i = 3; i <= boundary; i += 2)
                {
                    if (_config.EnablePerformanceLogging && i % 1000 == 0)
                    {
                        _logger?.LogTrace("Prime check for {Number} - testing divisor {Divisor}", number, i);
                    }

                    if (number % i == 0)
                    {
                        _logger?.LogDebug("Number {Number} is divisible by {Divisor}, not prime", number, i);
                        CacheResult(number, false);
                        return false;
                    }
                }

                _logger?.LogDebug("Number {Number} is prime", number);
                CacheResult(number, true);
                return true;
            }
            catch (OverflowException ex)
            {
                _logger?.LogError(ex, "Overflow error when checking if {Number} is prime", number);
                throw new ArgumentOutOfRangeException(nameof(number), ex,
                    "An overflow occurred while checking if the number is prime.");
            }
            catch (Exception ex) when (ex is not ArgumentOutOfRangeException)
            {
                _logger?.LogError(ex, "Unexpected error when checking if {Number} is prime", number);
                throw new InvalidOperationException("An unexpected error occurred while checking if the number is prime.", ex);
            }
        }

        /// <summary>
        /// Asynchronously determines whether the specified number is prime.
        /// </summary>
        /// <param name="number">The number to check.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The value of the TResult parameter contains true if the number is prime; otherwise, false.</returns>
        public async Task<bool> IsPrimeAsync(int number, CancellationToken cancellationToken = default)
        {
            // For very large numbers, we want to do the calculation on a background thread
            if (number > _config.AsyncThreshold)
            {
                _logger?.LogDebug("Number {Number} exceeds async threshold, calculating on background thread", number);
                return await Task.Run(() => IsPrime(number), cancellationToken);
            }

            // For smaller numbers, just do it synchronously
            return IsPrime(number);
        }

        /// <summary>
        /// Cache the primality result if caching is enabled
        /// </summary>
        private void CacheResult(int number, bool isPrime)
        {
            if (_config.EnableCaching && number <= _config.MaxCachedValue)
            {
                lock (_primeCache)
                {
                    if (_primeCache.Count >= _config.MaxCacheSize)
                    {
                        // Simple cache management - just clear it when it gets too big
                        _logger?.LogDebug("Prime cache reached max size {MaxSize}, clearing", _config.MaxCacheSize);
                        _primeCache.Clear();
                    }

                    _primeCache[number] = isPrime;
                }
            }
        }

        /// <summary>
        /// Clears the prime number cache
        /// </summary>
        public void ClearCache()
        {
            lock (_primeCache)
            {
                _primeCache.Clear();
                _logger?.LogInformation("Prime cache cleared");
            }
        }

        /// <summary>
        /// Static convenience method using default configuration
        /// </summary>
        public static bool CheckIsPrime(int number)
        {
            return new PrimeChecker().IsPrime(number);
        }
    }

    /// <summary>
    /// Configuration settings for the PrimeChecker
    /// </summary>
    public class PrimeCheckerConfig
    {
        /// <summary>
        /// Whether to enable result caching for improved performance
        /// </summary>
        public bool EnableCaching { get; set; } = true;

        /// <summary>
        /// The maximum size of the prime cache before it's cleared
        /// </summary>
        public int MaxCacheSize { get; set; } = 10000;

        /// <summary>
        /// The largest value to store in the cache
        /// </summary>
        public int MaxCachedValue { get; set; } = 100000;

        /// <summary>
        /// Whether to enforce safety limits on the maximum number that can be checked
        /// </summary>
        public bool EnableSafetyLimits { get; set; } = true;

        /// <summary>
        /// The maximum number that can be checked for primality
        /// </summary>
        public int MaxNumberToCheck { get; set; } = 100000000;

        /// <summary>
        /// Whether to log performance information during prime checking
        /// </summary>
        public bool EnablePerformanceLogging { get; set; } = false;

        /// <summary>
        /// Threshold above which prime checking will be done asynchronously
        /// </summary>
        public int AsyncThreshold { get; set; } = 100000;
    }
}
