using SQLiteNetExtensions.MicrosoftData.Attributes;

namespace SQLiteNetExtensions.MicrosoftData.UnitTests.Models;

public class DummyClassB
{
	[OneToOne]
	public DummyClassA? OneA { get; set; }

	[OneToOne]
	public DummyClassC? ObjectC { get; set; }
	public int DummyClassCKey { get; set; }
}
