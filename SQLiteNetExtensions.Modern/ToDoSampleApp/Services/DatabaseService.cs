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
        var result = await conn.GetAllWithChildrenAsync<TodoItem>();
        await conn.CloseAsync();
        return result;
    }

    public async Task SaveItemAsync(TodoItem item)
    {
        var conn = CreateAsyncConnection();
        await conn.InsertWithChildrenAsync(item);
        await conn.CloseAsync();
    }

    public async Task UpdateItemAsync(TodoItem item)
    {
        var conn = CreateAsyncConnection();
        await conn.UpdateWithChildrenAsync(item);
        await conn.CloseAsync();
    }

    public async Task<int> DeleteItemAsync(TodoItem item)
    {
        var conn = CreateAsyncConnection();
        var result = await conn.DeleteAsync(item);
        await conn.CloseAsync();
        return result;
    }

    public async Task Init()
    {
        var conn = CreateAsyncConnection();
        await conn.CreateTableAsync<TodoItem>();
        await conn.CreateTableAsync<Notes>();
        await conn.CloseAsync();
    }
}
