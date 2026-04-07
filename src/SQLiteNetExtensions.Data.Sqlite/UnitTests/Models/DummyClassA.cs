using SQLiteNetExtensions.Data.Sqlite.Attributes;

namespace SQLiteNetExtensions.Data.Sqlite.UnitTests.Models;

public class DummyClassA
{
	public int DummyBForeignKey { get; set; }

	[OneToOne("DummyBForeignKey")]
	public DummyClassB? OneB { get; set; }

	[OneToMany]
	public List<DummyClassC>? OneToManyC { get; set; }

	[ManyToMany(typeof(IntermediateDummyADummyD))]
	public DummyClassD[]? ManyToManyD { get; set; }

	public int FooInt { get; set; }
	public string? BarString { get; set; }
}
