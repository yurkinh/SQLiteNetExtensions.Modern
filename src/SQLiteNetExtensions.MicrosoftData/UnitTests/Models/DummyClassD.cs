using SQLiteNetExtensions.MicrosoftData.Attributes;

namespace SQLiteNetExtensions.MicrosoftData.UnitTests.Models;

public class DummyClassD
{
	[ForeignKey(typeof(DummyClassC))]
	public int ClassCKey { get; set; }

	[ManyToMany(typeof(IntermediateDummyADummyD))]
	public List<DummyClassA>? ManyA { get; set; }
}
