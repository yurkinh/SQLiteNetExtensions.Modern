using SQLite;
using SQLitePCL;

namespace IntegratedTestsSampleApp.Helpers;

public class Utils
{
    static Utils()
    {
        Batteries.Init();
    }

    public static string DatabaseFilePath => Path.Combine(FileSystem.AppDataDirectory, "database.db3");

    public static SQLiteConnection CreateConnection()
    {
        var con = new SQLiteConnection(DatabaseFilePath);
        raw.sqlite3_trace(con.Handle, Log, null);
        return con;
    }

    private static void Log(object userData, string statement)
    {
        Console.WriteLine(statement);
    }

    public static SQLiteAsyncConnection CreateAsyncConnection()
    {
        return new SQLiteAsyncConnection(DatabaseFilePath);
    }
}
