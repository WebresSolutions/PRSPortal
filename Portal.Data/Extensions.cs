using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql;
using System.Linq.Expressions;
using System.Reflection;

namespace Portal.Data;

public static class Extensions
{
    /// <summary>
    /// Inserts a large number of entities into the database using PostgreSQL's COPY command for efficiency.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dbContext"></param>
    /// <param name="entitiesToInsert"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static async Task<int> BulkInsertAsync<T>(this PrsDbContext dbContext, IEnumerable<T> entitiesToInsert) where T : class
    {
        List<T> entities = [.. entitiesToInsert];
        if (entities.Count == 0)
            return 0;

        (string[], PropertyInfo[], string?) columnAndProperties = GetColumnAndPropertyInfo<T>(dbContext);
        using NpgsqlConnection connection = new(dbContext.Database.GetConnectionString());
        connection.Open();

        using NpgsqlBinaryImporter writer = await connection.BeginBinaryImportAsync(
            $"COPY {columnAndProperties.Item3} ({string.Join(", ", columnAndProperties.Item1)}) FROM STDIN (FORMAT BINARY)");

        foreach (T entity in entities)
        {
            await writer.StartRowAsync();
            foreach (PropertyInfo? prop in columnAndProperties.Item2)
                await writer.WriteAsync(prop.GetValue(entity));
        }

        await writer.CompleteAsync();
        await writer.CloseAsync();
        return entities.Count;
    }

    /// <summary>
    /// Inserts a large number of entities into the database using PostgreSQL's COPY command for efficiency.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dbContext"></param>
    /// <param name="entitiesToInsert"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
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
                writer.Write(prop.GetValue(entity));
        }
        writer.Complete();
        writer.Close();
        return entities.Count;
    }

    /// <summary>
    /// Gets column and property info for the specified entity type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dbContext"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
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
    /// Forms and expression for a case-insensitive LIKE search using PostgreSQL's ILIKE.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="propertySelector"></param>
    /// <param name="searchTerm"></param>
    /// <returns></returns>
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
        MemberExpression efFunctionsProperty = Expression.Property(null, nameof(EF.Functions));

        MethodCallExpression ilikeCall = Expression.Call(ilikeMethod, efFunctionsProperty, propertyAccess, wrappedSearchTerm);

        Expression<Func<T, bool>> finalExpression = Expression.Lambda<Func<T, bool>>(ilikeCall, parameter);
        return source.Where(finalExpression);
    }
}
