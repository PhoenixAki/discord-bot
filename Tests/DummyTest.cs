using CompatBot.Utils;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class DummyTest
    {
        [Test]
        public void TestVariables()
        {
            Assert.That("Group".GetVisibleLength(), Is.EqualTo(5));
        }

        [Test]
        public void TestAcronymIsCorrect()
        {
            Assert.That("Bridgewater State University".GetAcronym(), Is.EqualTo("BSU"));
        }
    }
}