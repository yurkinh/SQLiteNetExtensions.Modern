using SQLite;
using SQLiteNetExtensions.Extensions;
using SQLitePCL;
using ToDoSampleApp.Models;

namespace ToDoSampleApp.Services;

public class DatabaseService : IDatabaseService
{
    public DatabaseService()
    {
        Batteries.Init();
    }

    readonly string databaseFilePath = Path.Combine(FileSystem.AppDataDirectory, "todo.db3");


    SQLiteAsyncConnection CreateAsyncConnection() => new(databaseFilePath);

    public async Task<List<TodoItem>> GetItemsAsync()
    {
        var conn = CreateAsyncConnection();
        var result = await conn.Table<TodoItem>().ToListAsync();
        await conn.CloseAsync();
        return result;
    }

    public async Task<int> SaveItemAsync(TodoItem item)
    {
        var conn = CreateAsyncConnection();
        await conn.CreateTableAsync<TodoItem>();
        var result = await conn.InsertAsync(item);
        await conn.CloseAsync();
        return result;
    }

    public async Task<int> DeleteItemAsync(TodoItem item)
    {
        var conn = CreateAsyncConnection();
        var result = await conn.DeleteAsync(item);
        await conn.CloseAsync();
        return result;
    }
}
