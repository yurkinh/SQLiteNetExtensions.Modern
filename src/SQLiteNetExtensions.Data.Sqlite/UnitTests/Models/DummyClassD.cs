using SQLiteNetExtensions.Data.Sqlite.Attributes;

namespace SQLiteNetExtensions.Data.Sqlite.UnitTests.Models;

public class DummyClassD
{
	[ForeignKey(typeof(DummyClassC))]
	public int ClassCKey { get; set; }

	[ManyToMany(typeof(IntermediateDummyADummyD))]
	public List<DummyClassA>? ManyA { get; set; }
}
