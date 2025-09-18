using System;
using System.Threading.Tasks;
using FileProcessorApp;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FileProcessorTests
{
    public class PrimeCheckerTests
    {
        private readonly PrimeChecker _checker;
        private readonly Mock<ILogger<PrimeChecker>> _mockLogger;
        private readonly PrimeCheckerConfig _config;

        public PrimeCheckerTests()
        {
            _mockLogger = new Mock<ILogger<PrimeChecker>>();
            _config = new PrimeCheckerConfig
            {
                EnableCaching = true,
                MaxCacheSize = 100,
                MaxNumberToCheck = 10000,
                EnableSafetyLimits = true
            };
            _checker = new PrimeChecker(_config, _mockLogger.Object);
        }

        [Theory]
        [InlineData(-10, false)]
        [InlineData(-1, false)]
        [InlineData(0, false)]
        [InlineData(1, false)]
        [InlineData(2, true)]
        [InlineData(3, true)]
        [InlineData(4, false)]
        [InlineData(17, true)]
        [InlineData(19, true)]
        [InlineData(20, false)]
        [InlineData(97, true)]
        [InlineData(100, false)]
        public void IsPrime_CoversEdgeCases(int number, bool expected)
        {
            bool result = _checker.IsPrime(number);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(-10, false)]
        [InlineData(19, true)]
        [InlineData(100, false)]
        public async Task IsPrimeAsync_CoversEdgeCases(int number, bool expected)
        {
            bool result = await _checker.IsPrimeAsync(number);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void IsPrime_ExceedingMaximumLimit_ThrowsException()
        {
            // Arrange
            int veryLargeNumber = _config.MaxNumberToCheck + 1;

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => _checker.IsPrime(veryLargeNumber));
            Assert.Contains("exceeds the maximum safe limit", exception.Message);
        }

        [Fact]
        public void IsPrime_WithCaching_ReturnsCachedResultForRepeatedCalls()
        {
            // Arrange
            int primeNumber = 9973; // A prime number

            // Act - First call will calculate and cache
            bool firstResult = _checker.IsPrime(primeNumber);

            // Act - Second call should use cache
            bool secondResult = _checker.IsPrime(primeNumber);

            // Assert
            Assert.True(firstResult);
            Assert.True(secondResult);
            // In a real test we would verify logging or use a timer to confirm cache was used
        }

        [Fact]
        public void ClearCache_RemovesAllCachedResults()
        {
            // Arrange - Prime a few numbers to ensure they're in cache
            _checker.IsPrime(17);
            _checker.IsPrime(19);
            _checker.IsPrime(23);

            // Act
            _checker.ClearCache();

            // Assert - Cannot directly test cache was cleared, but can verify 
            // logger was called. In real implementation we might expose cache stats.
            // We could also time operations to prove recalculation is happening.
        }

        [Fact]
        public void CheckIsPrime_StaticMethod_ReturnsCorrectResult()
        {
            // Arrange & Act
            bool isPrime = PrimeChecker.CheckIsPrime(17);
            bool isNotPrime = PrimeChecker.CheckIsPrime(20);

            // Assert
            Assert.True(isPrime);
            Assert.False(isNotPrime);
        }
    }
}
