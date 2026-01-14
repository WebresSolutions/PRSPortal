using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql;
using System.Linq.Expressions;
using System.Reflection;

namespace Portal.Data;

public static class Extensions
{
    /// <summary>
    /// Inserts a large number of entities into the database using PostgreSQL's COPY command for efficiency
    /// This method provides significantly better performance than individual inserts for bulk operations
    /// </summary>
    /// <typeparam name="T">The entity type to insert</typeparam>
    /// <param name="dbContext">The database context to use</param>
    /// <param name="entitiesToInsert">The collection of entities to insert</param>
    /// <returns>The number of entities inserted</returns>
    /// <exception cref="InvalidOperationException">Thrown when the entity type is not found in the model</exception>
    public static async Task<int> BulkInsertAsync<T>(this PrsDbContext dbContext, IEnumerable<T> entitiesToInsert) where T : class
    {
        List<T> entities = [.. entitiesToInsert];
        if (entities.Count == 0)
            return 0;

        (string[], PropertyInfo?[], string?) columnAndProperties = GetColumnAndPropertyInfo<T>(dbContext);
        using NpgsqlConnection connection = new(dbContext.Database.GetConnectionString());
        connection.Open();

        using NpgsqlBinaryImporter writer = await connection.BeginBinaryImportAsync(
            $"COPY {columnAndProperties.Item3} ({string.Join(", ", columnAndProperties.Item1)}) FROM STDIN (FORMAT BINARY)");

        foreach (T entity in entities)
        {
            await writer.StartRowAsync();
            foreach (PropertyInfo? prop in columnAndProperties.Item2)
                await writer.WriteAsync(prop!.GetValue(entity));
        }

        await writer.CompleteAsync();
        await writer.CloseAsync();
        return entities.Count;
    }

    /// <summary>
    /// Inserts a large number of entities into the database using PostgreSQL's COPY command for efficiency
    /// Synchronous version of BulkInsertAsync - use async version when possible
    /// </summary>
    /// <typeparam name="T">The entity type to insert</typeparam>
    /// <param name="dbContext">The database context to use</param>
    /// <param name="entitiesToInsert">The collection of entities to insert</param>
    /// <returns>The number of entities inserted</returns>
    /// <exception cref="InvalidOperationException">Thrown when the entity type is not found in the model</exception>
    public static int BulkInsert<T>(this PrsDbContext dbContext, IEnumerable<T> entitiesToInsert) where T : class
    {
        List<T> entities = [.. entitiesToInsert];
        if (entities.Count == 0)
            return 0;

        (string[], PropertyInfo?[], string?) columnAndProperties = GetColumnAndPropertyInfo<T>(dbContext);
        using NpgsqlConnection connection = new(dbContext.Database.GetConnectionString());
        connection.Open();

        using NpgsqlBinaryImporter writer = connection.BeginBinaryImport(
            $"COPY {columnAndProperties.Item3} ({string.Join(", ", columnAndProperties.Item1)}) FROM STDIN (FORMAT BINARY)");

        foreach (T entity in entities)
        {
            writer.StartRow();
            foreach (PropertyInfo? prop in columnAndProperties.Item2)
                writer.Write(prop!.GetValue(entity));
        }
        writer.Complete();
        writer.Close();
        return entities.Count;
    }

    /// <summary>
    /// Gets column and property information for the specified entity type
    /// Extracts metadata needed for bulk insert operations
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="dbContext">The database context to use</param>
    /// <returns>A tuple containing column names, property infos, and the full table name</returns>
    /// <exception cref="InvalidOperationException">Thrown when the entity type is not found in the model</exception>
    private static (string[] columns, PropertyInfo?[] properties, string? tableName) GetColumnAndPropertyInfo<T>(PrsDbContext dbContext) where T : class
    {
        IEntityType? entityType = dbContext.Model.FindEntityType(typeof(T));
        if (entityType == null)
            throw new InvalidOperationException($"Entity type {typeof(T).Name} not found in the model.");

        string? tableName = entityType.GetTableName();
        string? schema = entityType.GetSchema();
        string? fullTableName = schema != null ? $"{schema}.{tableName}" : tableName;

        // Get only non-generated columns
        // Include primary keys that are not auto-generated (e.g., composite keys, foreign keys used as PKs)
        // Exclude computed columns and auto-generated primary keys (but include composite keys we set explicitly)
        List<IProperty> properties = [.. entityType.GetProperties()
            .Where(p => p.GetComputedColumnSql() == null)
            .Where(p => !p.IsPrimaryKey() || p.ValueGenerated == Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never)];

        string[] columnNames = [.. properties.Select(p => p.GetColumnName())];

        PropertyInfo?[] propertyInfos = properties
            .Select(p => typeof(T).GetProperty(p.Name))
            .Where(p => p != null)
            .ToArray();

        return (columnNames, propertyInfos, fullTableName);
    }

    /// <summary>
    /// Creates an expression for a case-insensitive LIKE search using PostgreSQL's ILIKE operator
    /// Wraps the search term with wildcards for pattern matching
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="source">The queryable source to filter</param>
    /// <param name="propertySelector">Expression selecting the string property to search</param>
    /// <param name="searchTerm">The search term to match (will be wrapped with % wildcards)</param>
    /// <returns>A filtered queryable with the ILIKE condition applied</returns>
    public static IQueryable<T> ILike<T>(this IQueryable<T> source,
    Expression<Func<T, string>> propertySelector,
    string searchTerm)
    {
        ParameterExpression parameter = propertySelector.Parameters[0];
        ConstantExpression wrappedSearchTerm = Expression.Constant($"%{searchTerm}%");
        Type extensionsType = typeof(NpgsqlDbFunctionsExtensions);
        MethodInfo? ilikeMethod = extensionsType.GetMethod("ILike", [typeof(DbFunctions), typeof(string), typeof(string)]);

        if (ilikeMethod is null)
        {
            throw new Exception("Failed ot get the ILike method.");
        }

        Expression propertyAccess = propertySelector.Body;
        MemberExpression efFunctionsProperty = Expression.Property(propertyAccess, nameof(EF.Functions));

        MethodCallExpression ilikeCall = Expression.Call(ilikeMethod, efFunctionsProperty, propertyAccess, wrappedSearchTerm);

        Expression<Func<T, bool>> finalExpression = Expression.Lambda<Func<T, bool>>(ilikeCall, parameter);
        return source.Where(finalExpression);
    }
}
