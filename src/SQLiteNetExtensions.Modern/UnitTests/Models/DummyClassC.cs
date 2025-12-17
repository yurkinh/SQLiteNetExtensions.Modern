using SQLiteNetExtensions.Attributes;

namespace SQLiteNetExtensions.UnitTests.Models
{
    public class DummyClassC
    {
        [ManyToOne(inverseProperty: "")]
        public DummyClassD? ManyToOneD { get; set; }
    }
}