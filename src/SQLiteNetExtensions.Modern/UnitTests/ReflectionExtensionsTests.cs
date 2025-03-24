using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;
using SQLiteNetExtensions.UnitTests.Models;

namespace SQLiteNetExtensions.UnitTests;

[TestFixture]
public class ReflectionExtensionsTests
{

    [Test]
    public void TestOneToOneInverse()
    {
        var typeA = typeof(DummyClassA);
        var typeB = typeof(DummyClassB);

        var expectedAOneBProperty = typeA.GetProperty("OneB");
        var expectedBOneAProperty = typeB.GetProperty("OneA");

        var aOneBProperty = typeB.GetInverseProperty(expectedBOneAProperty);
        var bOneAProperty = typeA.GetInverseProperty(expectedAOneBProperty);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(aOneBProperty, Is.EqualTo(expectedAOneBProperty), "Type A -> Type B inverse relationship is not correct");
            Assert.That(bOneAProperty, Is.EqualTo(expectedBOneAProperty), "Type B -> Type A inverse relationship is not correct");
        }
    }

    [Test]
    public void TestNoInverse()
    {
        var typeC = typeof(DummyClassC);

        var cManyDProperty = typeC.GetProperty("ManyToOneD");

        var inverseProperty = typeC.GetInverseProperty(cManyDProperty);
        Assert.That(inverseProperty, Is.Null);

    }

    [Test]
    public void TestOneToOneRelationShipAttribute()
    {
        var typeA = typeof(DummyClassA);
        var property = typeA.GetProperty("OneB");

        var expectedAttributeType = typeof(OneToOneAttribute);
        var attribute = property.GetAttribute<RelationshipAttribute>();
        var attributeType = attribute.GetType();

        Assert.That(expectedAttributeType, Is.EqualTo(attributeType), "Relationship Attribute doesn't match expected type");
    }

    [Test]
    public void TestNoRelationShipAttribute()
    {
        var typeA = typeof(DummyClassA);
        var property = typeA.GetProperty("FooInt");

        var attribute = property.GetAttribute<RelationshipAttribute>();

        Assert.That(attribute, Is.Null);
    }

    [Test]
    public void TestEntityTypeObject()
    {
        var typeA = typeof(DummyClassA);
        var property = typeA.GetProperty("OneB");
        var expectedType = typeof(DummyClassB);
        const EnclosedType expectedContainerType = EnclosedType.None;

        var entityType = property.GetEntityType(out EnclosedType enclosedType);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(expectedType, Is.EqualTo(entityType));
            Assert.That(expectedContainerType, Is.EqualTo(enclosedType));
        }
    }

    [Test]
    public void TestEntityTypeArray()
    {
        var typeA = typeof(DummyClassA);
        var property = typeA.GetProperty("ManyToManyD");
        var expectedType = typeof(DummyClassD);
        const EnclosedType expectedContainerType = EnclosedType.Array;

        var entityType = property.GetEntityType(out EnclosedType enclosedType);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(expectedType, Is.EqualTo(entityType));
            Assert.That(expectedContainerType, Is.EqualTo(enclosedType));
        }
    }

    [Test]
    public void TestEntityTypeList()
    {
        var typeA = typeof(DummyClassA);
        var property = typeA.GetProperty("OneToManyC");
        var expectedType = typeof(DummyClassC);
        const EnclosedType expectedContainerType = EnclosedType.List;

        var entityType = property.GetEntityType(out EnclosedType enclosedType);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(expectedType, Is.EqualTo(entityType));
            Assert.That(expectedContainerType, Is.EqualTo(enclosedType));
        }
    }

    [Test]
    public void TestForeignKeyExplicitAttribute()
    {
        var typeC = typeof(DummyClassC);
        var typeD = typeof(DummyClassD);

        var property = typeC.GetProperty("ManyToOneD");
        var expectedForeignKeyProperty = typeD.GetProperty("ClassCKey");

        var foreignKeyProperty = typeC.GetForeignKeyProperty(property, inverse: true);

        Assert.That(expectedForeignKeyProperty, Is.EqualTo(foreignKeyProperty));
    }

    [Test]
    public void TestForeignKeyExplicitName()
    {
        var typeA = typeof(DummyClassA);
        var property = typeA.GetProperty("OneB");
        var expectedForeignKeyProperty = typeA.GetProperty("DummyBForeignKey");

        var foreignKeyProperty = typeA.GetForeignKeyProperty(property);

        Assert.That(expectedForeignKeyProperty, Is.EqualTo(foreignKeyProperty));
    }

    [Test]
    public void TestForeignKeyInverseExplicitName()
    {
        var typeA = typeof(DummyClassA);
        var typeB = typeof(DummyClassB);
        var property = typeB.GetProperty("OneA");
        var expectedForeignKeyProperty = typeA.GetProperty("DummyBForeignKey");

        var foreignKeyProperty = typeB.GetForeignKeyProperty(property, inverse: true);

        Assert.That(expectedForeignKeyProperty, Is.EqualTo(foreignKeyProperty));
    }

    [Test]
    public void TestForeignKeyConventionName()
    {
        var typeB = typeof(DummyClassB);
        var property = typeB.GetProperty("ObjectC");
        var expectedForeignKeyProperty = typeB.GetProperty("DummyClassCKey");

        var foreignKeyProperty = typeB.GetForeignKeyProperty(property);

        Assert.That(expectedForeignKeyProperty, Is.EqualTo(foreignKeyProperty));
    }

    [Test]
    public void TestForeignKeyUndefined()
    {
        var typeC = typeof(DummyClassC);
        var property = typeC.GetProperty("ManyToOneD");

        var foreignKeyProperty = typeC.GetForeignKeyProperty(property);

        Assert.That(foreignKeyProperty, Is.Null);
    }

    [Test]
    public void TestManyToManyMetaInfo()
    {
        var typeA = typeof(DummyClassA);
        var intermediateType = typeof(IntermediateDummyADummyD);

        var manyAToManyDProperty = typeA.GetProperty("ManyToManyD");
        var expectedTypeAForeignKeyProperty = intermediateType.GetProperty("DummyClassAForeignKey");
        var expectedTypeDForeignKeyProperty = intermediateType.GetProperty("ClassDKey");

        var metaInfo = typeA.GetManyToManyMetaInfo(manyAToManyDProperty);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(metaInfo.IntermediateType, Is.EqualTo(intermediateType));
            Assert.That(expectedTypeAForeignKeyProperty, Is.EqualTo(metaInfo.OriginProperty));
            Assert.That(expectedTypeDForeignKeyProperty, Is.EqualTo(metaInfo.DestinationProperty));
        }
    }

    [Test]
    public void TestExpressionProperty()
    {
        var typeB = typeof(DummyClassB);

        var expectedAOneBProperty = typeB.GetProperty("OneA");

        var aOneBProperty = ReflectionExtensions.GetProperty<DummyClassB>(a => a.OneA);

        Assert.That(expectedAOneBProperty, Is.EqualTo(aOneBProperty));
    }
}