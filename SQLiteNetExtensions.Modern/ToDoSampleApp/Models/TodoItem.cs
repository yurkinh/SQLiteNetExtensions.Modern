using SQLite;
using SQLiteNetExtensions.Attributes;

namespace ToDoSampleApp.Models;

[Table("TodoItems")]
public class TodoItem
{
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }
    public string? Name { get; set; }
    public string? Notes { get; set; }
    public bool Done { get; set; }

    [OneToOne]
    public ChildItem? Children { get; set; }
}

