using SQLiteNetExtensions.Attributes;

namespace SQLiteNetExtensions.UnitTests.Models;

public class DummyClassD
{
    [Attributes.ForeignKey(typeof(DummyClassC))]
    public int ClassCKey { get; set; }

    [ManyToMany(typeof(IntermediateDummyADummyD))]
    public List<DummyClassA> ManyA { get; set; }
}