using SQLite;
using SQLiteNetExtensions.Attributes;

namespace ToDoSampleApp.Models;

[Table("TodoItems")]
public class TodoItem
{
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }
    public string? Name { get; set; }
    public bool Done { get; set; }

    [ForeignKey(typeof(Notes))]
    public int NotesId { get; set; }

    [OneToOne(CascadeOperations = CascadeOperation.All)]
    public Notes? Notes { get; set; }
}

