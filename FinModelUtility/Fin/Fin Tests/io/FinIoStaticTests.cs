using NUnit.Framework;

namespace fin.io;

class FinIoStaticTests {
  [Test]
  [TestCase("", ExpectedResult = "")]
  [TestCase("foo", ExpectedResult = "foo")]
  [TestCase("foo/bar", ExpectedResult = "bar")]
  [TestCase("foo/bar/", ExpectedResult = "bar")]
  public string TestGetName(string path)
    => FinIoStatic.GetName(path).ToString();

  [Test]
  [TestCase("", ExpectedResult = "")]
  [TestCase("foo", ExpectedResult = "")]
  [TestCase("foo/bar", ExpectedResult = "foo")]
  [TestCase("foo/bar/", ExpectedResult = "foo")]
  public string TestGetParentFullName(string path)
    => FinIoStatic.GetParentFullName(path).ToString();
}