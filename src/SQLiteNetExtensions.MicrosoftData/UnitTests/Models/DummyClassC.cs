using SQLiteNetExtensions.MicrosoftData.Attributes;

namespace SQLiteNetExtensions.MicrosoftData.UnitTests.Models;

public class DummyClassC
{
	[ManyToOne(inverseProperty: "")]
	public DummyClassD? ManyToOneD { get; set; }
}
