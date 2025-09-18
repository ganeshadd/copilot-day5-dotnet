using System;
using System.Collections.Generic;
using System.Text;

namespace FileProcessorLib
{
    /// <summary>
    /// A fluent SQL query builder for constructing SELECT statements
    /// </summary>
    public class SqlQueryBuilder
    {
        private readonly StringBuilder _query;
        private readonly List<string> _selectFields;
        private string _tableName;
        private readonly List<string> _whereConditions;
        private readonly List<string> _orderByFields;
        private readonly List<SqlParameter> _parameters;

        public SqlQueryBuilder()
        {
            _query = new StringBuilder();
            _selectFields = new List<string>();
            _whereConditions = new List<string>();
            _orderByFields = new List<string>();
            _parameters = new List<SqlParameter>();
        }

        /// <summary>
        /// Adds fields to the SELECT clause
        /// </summary>
        public SqlQueryBuilder Select(params string[] fields)
        {
            if (fields == null || fields.Length == 0)
            {
                _selectFields.Add("*");
            }
            else
            {
                // Validate all field names are safe
                foreach (var field in fields)
                {
                    if (!string.IsNullOrWhiteSpace(field) && field != "*")
                    {
                        ValidateIdentifierSafety(field, nameof(fields));
                    }
                }
                _selectFields.AddRange(fields);
            }
            return this;
        }

        /// <summary>
        /// Sets the table name for the FROM clause
        /// </summary>
        public SqlQueryBuilder From(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));
            
            // Validate table name is safe (alphanumeric, underscore, no SQL injection)
            ValidateIdentifierSafety(tableName, nameof(tableName));
            
            _tableName = tableName;
            return this;
        }

        /// <summary>
        /// Adds a WHERE condition with a parameter
        /// </summary>
        public SqlQueryBuilder Where(string condition, object value)
        {
            if (string.IsNullOrWhiteSpace(condition))
                throw new ArgumentException("Condition cannot be null or empty", nameof(condition));

            // Validate condition contains exactly one placeholder
            int placeholderCount = CountOccurrences(condition, '?');
            if (placeholderCount != 1)
                throw new ArgumentException("Condition must contain exactly one '?' placeholder", nameof(condition));

            // Validate condition doesn't contain dangerous SQL keywords
            ValidateConditionSafety(condition);

            var paramName = $"@param{_parameters.Count}";
            var safeCondition = condition.Replace("?", paramName);
            _whereConditions.Add(safeCondition);
            _parameters.Add(new SqlParameter(paramName, value));
            return this;
        }

        private static int CountOccurrences(string text, char target)
        {
            int count = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == target) count++;
            }
            return count;
        }

        private static void ValidateConditionSafety(string condition)
        {
            var dangerousKeywords = new[] { "DROP", "DELETE", "INSERT", "UPDATE", "CREATE", "ALTER", "EXEC", "EXECUTE", "--", "/*", "*/", "XP_", "SP_" };
            var upperCondition = condition.ToUpperInvariant();
            
            foreach (var keyword in dangerousKeywords)
            {
                if (upperCondition.Contains(keyword))
                    throw new ArgumentException($"Condition contains dangerous SQL keyword: {keyword}", nameof(condition));
            }
        }

        /// <summary>
        /// Adds a WHERE condition for fetching user by ID
        /// </summary>
        public SqlQueryBuilder WhereUserId(int userId)
        {
            return Where("Id = ?", userId);
        }

        /// <summary>
        /// Adds a WHERE condition for fetching user by email
        /// </summary>
        public SqlQueryBuilder WhereUserEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));
            
            return Where("Email = ?", email);
        }

        /// <summary>
        /// Adds an AND condition to the WHERE clause
        /// </summary>
        public SqlQueryBuilder And(string condition, object value)
        {
            if (_whereConditions.Count == 0)
                throw new InvalidOperationException("Cannot add AND condition without an existing WHERE condition");
            
            return Where($"AND {condition}", value);
        }

        /// <summary>
        /// Adds an OR condition to the WHERE clause
        /// </summary>
        public SqlQueryBuilder Or(string condition, object value)
        {
            if (_whereConditions.Count == 0)
                throw new InvalidOperationException("Cannot add OR condition without an existing WHERE condition");
            
            return Where($"OR {condition}", value);
        }

        /// <summary>
        /// Adds ORDER BY clause
        /// </summary>
        public SqlQueryBuilder OrderBy(string field, SortDirection direction = SortDirection.Ascending)
        {
            if (string.IsNullOrWhiteSpace(field))
                throw new ArgumentException("Field cannot be null or empty", nameof(field));

            // Validate field name is safe
            ValidateIdentifierSafety(field, nameof(field));

            var orderClause = direction == SortDirection.Descending ? $"{field} DESC" : field;
            _orderByFields.Add(orderClause);
            return this;
        }

        private static void ValidateIdentifierSafety(string identifier, string paramName)
        {
            // Allow only alphanumeric characters, underscores, and dots (for table.column)
            for (int i = 0; i < identifier.Length; i++)
            {
                char c = identifier[i];
                if (!char.IsLetterOrDigit(c) && c != '_' && c != '.')
                {
                    throw new ArgumentException($"Invalid character '{c}' in {paramName}. Only letters, digits, underscores, and dots are allowed.", paramName);
                }
            }
        }

        /// <summary>
        /// Builds and returns the SQL query string
        /// </summary>
        public string Build()
        {
            if (string.IsNullOrEmpty(_tableName))
                throw new InvalidOperationException("Table name must be specified using From() method");

            _query.Clear();
            
            // SELECT clause
            _query.Append("SELECT ");
            _query.Append(_selectFields.Count > 0 ? string.Join(", ", _selectFields) : "*");
            
            // FROM clause
            _query.Append($" FROM {_tableName}");
            
            // WHERE clause
            if (_whereConditions.Count > 0)
            {
                _query.Append(" WHERE ");
                _query.Append(string.Join(" ", _whereConditions));
            }
            
            // ORDER BY clause
            if (_orderByFields.Count > 0)
            {
                _query.Append(" ORDER BY ");
                _query.Append(string.Join(", ", _orderByFields));
            }

            return _query.ToString();
        }

        /// <summary>
        /// Gets the parameters for the query
        /// </summary>
        public IReadOnlyList<SqlParameter> GetParameters()
        {
            return _parameters.AsReadOnly();
        }

        /// <summary>
        /// Creates a new SqlQueryBuilder instance for method chaining
        /// </summary>
        public static SqlQueryBuilder Create()
        {
            return new SqlQueryBuilder();
        }

        /// <summary>
        /// Quick method to fetch user by ID
        /// </summary>
        public static SqlQueryBuilder GetUserById(int userId)
        {
            return Create()
                .Select("Id", "FirstName", "LastName", "Email", "CreatedDate")
                .From("Users")
                .WhereUserId(userId);
        }

        /// <summary>
        /// Quick method to fetch all users
        /// </summary>
        public static SqlQueryBuilder GetAllUsers()
        {
            return Create()
                .Select("Id", "FirstName", "LastName", "Email", "CreatedDate")
                .From("Users")
                .OrderBy("LastName");
        }

        /// <summary>
        /// Quick method to search users by email pattern
        /// </summary>
        public static SqlQueryBuilder SearchUsersByEmail(string emailPattern)
        {
            return Create()
                .Select("Id", "FirstName", "LastName", "Email")
                .From("Users")
                .Where("Email LIKE ?", $"%{emailPattern}%")
                .OrderBy("Email");
        }
    }

    /// <summary>
    /// Represents a SQL parameter
    /// </summary>
    public class SqlParameter
    {
        public string Name { get; }
        public object Value { get; }

        public SqlParameter(string name, object value)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = value;
        }
    }

    /// <summary>
    /// Sort direction for ORDER BY clauses
    /// </summary>
    public enum SortDirection
    {
        Ascending,
        Descending
    }
}