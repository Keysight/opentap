using System;
using System.Collections.Generic;
using System.Xml.Linq;
using NUnit.Framework;
using OpenTap.Package;
using OpenTap.Plugins.BasicSteps;

namespace OpenTap.Engine.UnitTests
{
    [TestFixture]
    public class SerializerTests
    {
        public class TestPlanWithMetaData : TestPlan
        {
            [MetaData(Name= "X Setting")]
            public string X { get; set; }
            
            [MetaData]
            public string Y { get; set; }
        } 
        [Test]
        public void TestSerializeTestPlanMetaData()
        {
            var plan = new TestPlanWithMetaData();
            var xml = plan.SerializeToString();
            var xdoc = XDocument.Parse(xml);
            Assert.AreEqual("X Setting", xdoc.Root.Element("X").Attribute("Metadata").Value);
            Assert.AreEqual("Y", xdoc.Root.Element("Y").Attribute("Metadata").Value);
        }

        [Test]
        public void TestPackageDependencySerializer()
        {
            var plan = new TestPlan()
            {
                ChildTestSteps = { new DelayStep() }
            };

            var ser = new TapSerializer();
            { // verify that a serialized plan has package dependencies
                var str = ser.SerializeToString(plan);   
                CollectionAssert.IsEmpty(ser.Errors);
                var elem = XElement.Parse(str);
                Assert.AreEqual(1, elem.Elements("Package.Dependencies").Count());
            }
            { // verify that a serialized collection of plans has package dependencies
                var plans = new TestPlan[]
                {
                    plan,
                    plan
                };
                var str = ser.SerializeToString(plans);
                CollectionAssert.IsEmpty(ser.Errors);
                var elem = XElement.Parse(str);
                Assert.AreEqual(1, elem.Elements("Package.Dependencies").Count());            
            }
            { // verify that a serialized list of package versions does not have package dependencies
                var versions = new PackageVersion[]
                {
                    new PackageVersion("pkg", SemanticVersion.Parse("1.0.0"), "Linux", CpuArchitecture.AnyCPU, DateTime.Now, 
                        new List<string>()
                        {
                            "Lic1",
                            "Lic2"
                        })
                };
                var str = ser.SerializeToString(versions);
                CollectionAssert.IsEmpty(ser.Errors);
                var elem = XElement.Parse(str);
                Assert.AreEqual(0, elem.Elements("Package.Dependencies").Count());

                var deserialized = ser.DeserializeFromString(str);
                if (deserialized is PackageVersion[] versions2)
                {
                    Assert.AreEqual(1, versions2.Count());
                    CollectionAssert.AreEqual(versions, versions2, "Deserialized versions were different from the serialized versions.");
                    CollectionAssert.AreEqual(versions[0].Licenses, versions2[0].Licenses);
                }
                else
                    Assert.Fail($"Failed to deserialize serialized version array.");
            }
        }
    }
}
