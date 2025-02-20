using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Xml.Linq;


namespace HaloWarsTools {
  public class HWGlsResource : HWXmlResource {
    public static new HWGlsResource FromFile(HWContext context, string filename) {
      return GetOrCreateFromFile(context, filename, HWResourceType.Gls) as HWGlsResource;
    }

    public Vector3 SunDirection => ValueCache.Get(() => {
      float sunInclination = (float) XmlData.Descendants("sunInclination").First();
      float sunRotation = (float) XmlData.Descendants("sunRotation").First();
      return GetDirectionVectorFromInclinationRotation(sunInclination, sunRotation);
    });

    public Color SunColor => ValueCache.Get(() => GetColor("setTerrainColor"));
    public Color BackgroundColor => ValueCache.Get(() => GetColor("backgroundColor"));

    private Color GetColor(string name) {
      return ConvertVector3ToColor(DeserializeVector3(XmlData.Descendants(name).First().Value));
    }

    private static Color ConvertVector3ToColor(Vector3 vector) {
      return Color.FromArgb(255, FloatTo8BitColorChannel(vector.X), FloatTo8BitColorChannel(vector.Y),
                            FloatTo8BitColorChannel(vector.Z));
    }

    private static int FloatTo8BitColorChannel(float value) {
      int convertedValue = (int) Math.Round(value);
      return Math.Clamp(convertedValue, 0, 255);
    }

    private static float DegreesToRadians = (float) Math.PI / 180f;

    private static Vector3 GetDirectionVectorFromInclinationRotation(float inclination, float rotation) {
      var cosInclination = (float) Math.Cos(inclination * DegreesToRadians);
      var direction = Vector3.Normalize(new Vector3((float) Math.Cos(rotation * DegreesToRadians) * cosInclination,
                                                    (float) Math.Sin(rotation * DegreesToRadians) * cosInclination,
                                                    (float) Math.Sin(inclination * DegreesToRadians)));

      return direction;
    }
  }
}