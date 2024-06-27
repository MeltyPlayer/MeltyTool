using System;
using System.Numerics;

using fin.math.floats;
using fin.util.asserts;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.math.rotations;

public class QuaternionUtilTests {
  [Test]
  [TestCase(0, 0, 0, 0, 0, 0, 1)]
  [TestCase(2 * MathF.PI, 2 * MathF.PI, 2 * MathF.PI, 0, 0, 0, -1)]
  [TestCase(1, 2, 3, -.7182f, .3106f, .4444f, .4359f)]
  [TestCase(2, 3, 4, -.5148f, -.1701f, .3840f, .7473f)]
  public void TestCreateZyx(float xRadians,
                            float yRadians,
                            float zRadians,
                            float expectedQX,
                            float expectedQY,
                            float expectedQZ,
                            float expectedQW) {
    var actualQuaternion = QuaternionUtil.CreateZyx(xRadians, yRadians, zRadians);

    if (!actualQuaternion.X.IsRoughly(expectedQX) ||
        !actualQuaternion.Y.IsRoughly(expectedQY) ||
        !actualQuaternion.Z.IsRoughly(expectedQZ) ||
        !actualQuaternion.W.IsRoughly(expectedQW)) {
      Assert.Fail($"Expected {actualQuaternion} to equal {new Quaternion(expectedQX, expectedQY, expectedQZ, expectedQW)}");
    }
  }

  [Test]
  public void ToEulerRadians() {
    var degToRad = MathF.PI / 180;

    var x = 1 * 20 * degToRad;
    var y = 2 * 20 * degToRad;
    var z = 3 * 20 * degToRad;

    var q = QuaternionUtil.CreateZyx(x, y, z);

    var v = QuaternionUtil.ToEulerRadians(q);

    Asserts.IsRoughly(x, v.X);
    Asserts.IsRoughly(y, v.Y);
    Asserts.IsRoughly(z, v.Z);
  }

  [Test]
  public void ToEulerRadiansIdentity() {
    var v = QuaternionUtil.ToEulerRadians(Quaternion.Identity);

    Asserts.IsRoughly(0, v.X);
    Asserts.IsRoughly(0, v.Y);
    Asserts.IsRoughly(0, v.Z);
  }
}