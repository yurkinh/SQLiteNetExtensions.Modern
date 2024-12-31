using SQLite;

namespace ToDoSampleApp.Models;

[Table("Notes")]
public class Notes
{
	[PrimaryKey, AutoIncrement]
	public int ID { get; set; }
	public string? Title { get; set; }
	public string? Body { get; set; }
	public DateTime CreationTime { get; set; } = DateTime.Now;
	public DateTime EventDateTime { get; set; }
}
