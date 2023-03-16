//using System.Numerics;
#nullable enable

using System.Drawing;
using System.Numerics;

namespace rt004
{

  class Lights
  {
    Dictionary<int, LightSource> lightSources = new();
    AmbientLight ambientLight = new AmbientLight(1);
    int currentIndex = 0;
    public int AddLight(Vector3 position, Vector3 color, float intensity)
    {
      lightSources.Add(currentIndex, new LightSource(position, color, intensity));
      currentIndex++;
      return currentIndex-1;
    }
    public void RemoveLight(int index)
    {
      lightSources.Remove(index);
    }

    public void AddAmbientLight(float intensity)
    {
      this.ambientLight = new AmbientLight(intensity);
    }

    public Vector3 GetColor(Solid solid, Vector3 intersectionPoint, Vector3 viewPoint)
    {
      Vector3 normal = solid.GetNormal(intersectionPoint);
      Vector3 lightsComponent = Vector3.Zero;
      foreach (var light in lightSources.Values)
      {
        Vector3 lightVector = VectorCalculator.GetVectorBetween2Points(intersectionPoint, light.position);
        Vector3 difuseComponent = solid.color;
        difuseComponent *= light.intensity;
        difuseComponent *= solid.material.diffuse;
        difuseComponent *= Math.Max(0, Vector3.Dot(normal, lightVector));

        Vector3 viewPointVec = VectorCalculator.GetVectorBetween2Points(viewPoint, intersectionPoint);
        Vector3 reflectVec = VectorCalculator.GetReflectVector(lightVector, normal);
        Vector3 reflectionComponent = light.color;
        reflectionComponent *= light.intensity;
        reflectionComponent *= solid.material.reflection;
        float cosBeta = Math.Max(Vector3.Dot(normal, reflectVec), 0);
        float powerCosB = (float)Math.Pow(cosBeta, solid.material.reflectionSize);
        reflectionComponent *= powerCosB;

        if (reflectionComponent.X > 0)
          cosBeta = 0;

        lightsComponent += reflectionComponent + difuseComponent;
      }

      Vector3 ambientComponent = solid.color * solid.material.ambient * ambientLight.intensity;

      return lightsComponent + ambientComponent;
    }

  }

  struct AmbientLight
  {
    public float intensity;

    public AmbientLight(float intensity)
    {
      this.intensity = intensity;
    }
  }

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


