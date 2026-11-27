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
    readonly Lock initLock = new();
    Task? initTask;


    SQLiteAsyncConnection CreateAsyncConnection() => new(databaseFilePath);

    public async Task<List<TodoItem>> GetItemsAsync()
    {
        await Init();
        var conn = CreateAsyncConnection();
        var result = await conn.GetAllWithChildrenAsync<TodoItem>();
        await conn.CloseAsync();
        return result;
    }

    public async Task SaveItemAsync(TodoItem item)
    {
        await Init();
        var conn = CreateAsyncConnection();
        await conn.InsertWithChildrenAsync(item);
        await conn.CloseAsync();
    }

    public async Task UpdateItemAsync(TodoItem item)
    {
        await Init();
        var conn = CreateAsyncConnection();
        await conn.UpdateWithChildrenAsync(item);
        await conn.CloseAsync();
    }

    public async Task<int> DeleteItemAsync(TodoItem item)
    {
        await Init();
        var conn = CreateAsyncConnection();
        var result = await conn.DeleteAsync(item);
        await conn.CloseAsync();
        return result;
    }

    // Creates the tables once. Every operation awaits this first, so a query can't run
    // before the tables exist, whatever order App.OnStart and the first page load run in.
    public Task Init()
    {
        lock (initLock)
        {
            if (initTask is null || initTask.IsFaulted)
                initTask = InitCoreAsync();
            return initTask;
        }
    }

    async Task InitCoreAsync()
    {
        var conn = CreateAsyncConnection();
        await conn.CreateTableAsync<TodoItem>();
        await conn.CreateTableAsync<Notes>();
        await conn.CloseAsync();
    }
}
