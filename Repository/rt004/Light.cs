//using System.Numerics;
#nullable enable

using System.Drawing;
using System.Numerics;

namespace rt004
{
  struct LightSource
  {
    public float intensity;
    public Vector3 color;
    public Vector3 position;
    public LightSource(Vector3 position, Vector3 color, float intensity)
    {
      this.color = color;
      this.position = position;
      this.intensity = intensity;
    }
  }
}


