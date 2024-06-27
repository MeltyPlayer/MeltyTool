using System.Numerics;

using NUnit.Framework;


namespace fin.math.matrix.four;

internal class Matrix4x4StackTests {
  [Test]
  public void TestFirstStackDefaultsToIdentity() {
    var impl = new Matrix4x4Stack();
    Assert.AreEqual(Matrix4x4.Identity, impl.Top);
  }

  [Test]
  public void TestPushNew() {
    var impl = new Matrix4x4Stack();

    var matrix = SystemMatrix4x4Util.FromTranslation(1, 2, 3);
    impl.Push(matrix);

    Assert.AreEqual(matrix, impl.Top);
  }

  [Test]
  public void TestPushSame() {
    var impl = new Matrix4x4Stack();

    var matrix = SystemMatrix4x4Util.FromTranslation(1, 2, 3);
    impl.Push(matrix);
    impl.Push();

    Assert.AreEqual(matrix, impl.Top);
  }

  [Test]
  public void TestPop() {
    var impl = new Matrix4x4Stack();
    var first = SystemMatrix4x4Util.FromTranslation(1, 2, 3);
    impl.Push(first);
    var second = SystemMatrix4x4Util.FromTranslation(2, 3, 4);
    impl.Push(second);

    impl.Pop();

    Assert.AreEqual(first, impl.Top);
  }

  [Test]
  public void TestSetTop() {
    var impl = new Matrix4x4Stack();
    var first = SystemMatrix4x4Util.FromTranslation(1, 2, 3);
    impl.Push(first);

    var second = SystemMatrix4x4Util.FromTranslation(2, 3, 4);
    impl.Top = second;

    Assert.AreEqual(second, impl.Top);
  }

  [Test]
  public void TestSetIdentity() {
    var impl = new Matrix4x4Stack();
    var first = SystemMatrix4x4Util.FromTranslation(1, 2, 3);
    impl.Push(first);

    impl.SetIdentity();

    Assert.AreEqual(Matrix4x4.Identity, impl.Top);
  }

  [Test]
  public void TestMultiplyInPlace() {
    var impl = new Matrix4x4Stack();

    var first = SystemMatrix4x4Util.FromTranslation(1, 2, 3);
    impl.MultiplyInPlace(first);
    var second = SystemMatrix4x4Util.FromTranslation(2, 3, 4);
    impl.MultiplyInPlace(second);

    Assert.AreEqual(first * second, impl.Top);
  }
}