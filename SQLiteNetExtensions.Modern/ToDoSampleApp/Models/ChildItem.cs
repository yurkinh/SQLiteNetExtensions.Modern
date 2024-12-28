using SQLite;

namespace ToDoSampleApp.Models;

[Table("ChildItems")]
public class ChildItem
{
	[PrimaryKey, AutoIncrement]
	public int ID { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public int ParentID { get; set; }

}
