using FileProcessorLib;
using System.Linq;

class Program
{
    static async Task Main(string[] args)
    {
        System.Console.WriteLine("FileProcessorApp is running.");
        
        // Example usage of refactored FileProcessor
        await DemoFileProcessor();
        
        // Example usage of SQL Query Builder
        DemoSqlQueryBuilder();
    }

    static async Task DemoFileProcessor()
    {
        System.Console.WriteLine("\n=== File Processor Demo ===");
        
        var processor = new FileProcessor();
        
        try
        {
            // Create a sample file for testing
            string testFilePath = "sample.txt";
            await File.WriteAllTextAsync(testFilePath, "This is a sample file for testing the FileProcessor.");
            
            // Use the async method for better performance
            int fileLength = await processor.GetFileLengthAsync(testFilePath);
            System.Console.WriteLine($"File length (async): {fileLength} bytes");
            
            // Clean up
            File.Delete(testFilePath);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error processing file: {ex.Message}");
        }
    }

    static void DemoSqlQueryBuilder()
    {
        System.Console.WriteLine("\n=== SQL Query Builder Demo ===");
        
        // Example 1: Get user by ID
        var getUserQuery = SqlQueryBuilder.GetUserById(123);
        System.Console.WriteLine("Get User By ID Query:");
        System.Console.WriteLine(getUserQuery.Build());
        System.Console.WriteLine($"Parameters: {string.Join(", ", getUserQuery.GetParameters().Select(p => $"{p.Name}={p.Value}"))}");
        
        // Example 2: Custom query with multiple conditions
        var customQuery = SqlQueryBuilder.Create()
            .Select("Id", "FirstName", "LastName", "Email")
            .From("Users")
            .Where("IsActive = ?", true)
            .And("CreatedDate > ?", DateTime.Now.AddDays(-30))
            .OrderBy("LastName")
            .OrderBy("FirstName");
        
        System.Console.WriteLine("\nCustom Query with Multiple Conditions:");
        System.Console.WriteLine(customQuery.Build());
        System.Console.WriteLine($"Parameters: {string.Join(", ", customQuery.GetParameters().Select(p => $"{p.Name}={p.Value}"))}");
        
        // Example 3: Search users by email pattern
        var searchQuery = SqlQueryBuilder.SearchUsersByEmail("gmail.com");
        System.Console.WriteLine("\nSearch Users by Email Pattern:");
        System.Console.WriteLine(searchQuery.Build());
        System.Console.WriteLine($"Parameters: {string.Join(", ", searchQuery.GetParameters().Select(p => $"{p.Name}={p.Value}"))}");
        
        // Example 4: Get all users ordered by name
        var allUsersQuery = SqlQueryBuilder.GetAllUsers();
        System.Console.WriteLine("\nGet All Users:");
        System.Console.WriteLine(allUsersQuery.Build());
    }
}
