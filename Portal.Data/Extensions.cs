using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql;
using System.Reflection;

namespace Portal.Data;

public static class Extensions
{
    /// <summary>
    /// A 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dbContext"></param>
    /// <param name="entitiesToInsert"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static async Task<int> BulkInsert<T>(this PrsDbContext dbContext, IEnumerable<T> entitiesToInsert) where T : class
    {
        List<T> entities = [.. entitiesToInsert];
        if (entities.Count == 0)
            return 0;

        IEntityType? entityType = dbContext.Model.FindEntityType(typeof(T)) ?? throw new InvalidOperationException($"Entity type {typeof(T).Name} not found");
        string? tableName = entityType.GetTableName();
        List<IProperty> properties = [.. entityType.GetProperties().Where(p => p.ValueGenerated != ValueGenerated.OnAdd)];

        string[] columnNames = [.. properties.Select(p => p.GetColumnName())];
        PropertyInfo[] propertyInfos = [.. properties.Select(p => typeof(T).GetProperty(p.Name)!)];

        using NpgsqlConnection connection = new(dbContext.Database.GetConnectionString());
        connection.Open();

        using NpgsqlBinaryImporter writer = await connection.BeginBinaryImportAsync(
            $"COPY {tableName} ({string.Join(", ", columnNames)}) FROM STDIN (FORMAT BINARY)");

        foreach (T entity in entities)
        {
            await writer.StartRowAsync();
            foreach (PropertyInfo? prop in propertyInfos)
                await writer.WriteAsync(prop.GetValue(entity));
        }

        await writer.CompleteAsync();
        await writer.CloseAsync();
        return entities.Count;
    }

}
