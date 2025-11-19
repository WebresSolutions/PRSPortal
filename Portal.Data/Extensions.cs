using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql;
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

        (string[], PropertyInfo[], string?) columnAndProperties = GetColumnAndPropertyInfo<T>(dbContext);
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
    public static (string[], PropertyInfo[], string?) GetColumnAndPropertyInfo<T>(this PrsDbContext dbContext) where T : class
    {
        IEntityType? entityType = dbContext.Model.FindEntityType(typeof(T))
            ?? throw new InvalidOperationException($"Entity type {typeof(T).Name} not found");
        string? tableName = entityType.GetTableName();
        List<IProperty> properties = [.. entityType.GetProperties().Where(p => p.ValueGenerated != ValueGenerated.OnAdd)];
        string[] columnNames = [.. properties.Select(p => p.GetColumnName())];
        PropertyInfo[] propertyInfos = [.. properties.Select(p => typeof(T).GetProperty(p.Name)!)];
        return (columnNames, propertyInfos, tableName);
    }
}
