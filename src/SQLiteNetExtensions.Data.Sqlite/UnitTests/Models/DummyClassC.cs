using SQLiteNetExtensions.Data.Sqlite.Attributes;

namespace SQLiteNetExtensions.Data.Sqlite.UnitTests.Models;

public class DummyClassC
{
	[ManyToOne(inverseProperty: "")]
	public DummyClassD? ManyToOneD { get; set; }
}
