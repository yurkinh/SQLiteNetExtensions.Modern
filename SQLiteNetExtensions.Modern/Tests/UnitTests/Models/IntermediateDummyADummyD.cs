using SQLiteNetExtensions.Attributes;

namespace SQLiteNetExtensions.UnitTests.Models;

public class IntermediateDummyADummyD
{
    public int DummyClassAForeignKey { get; set; } // Convention name
    [ForeignKey(typeof(DummyClassD))]
    public int ClassDKey { get; set; } // Explicitly declared foreign key
}