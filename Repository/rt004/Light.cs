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

    public Vector3 GetColor(Solid solid, Vector3 intersectionPoint, Ray ray)
    {
      Vector3 lightsComponent = Vector3.Zero;
      foreach (var light in lightSources.Values)
      {
        lightsComponent += light.GetColor(solid, intersectionPoint, ray);
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

  class LightSource
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

    private float SecondShade(Vector3 intersectionPoint)
    {
      const double epsilon = 1.0e-4;
      Vector3 intersectionLightVector = VectorCalculator.GetVectorBetween2Points(intersectionPoint, position);
      Ray ray = new Ray(intersectionPoint, intersectionLightVector);
      double? t;
      Solid? solid;
      Scene.solids.GetClosestIntersection(ray, out t, out solid);
      if ((t is not null || solid is not null) && (t > epsilon || t < -epsilon))
      {
        return 0;
      }
      return 1f/(float)Math.Pow(DistanceCalculator.GetDistance(intersectionPoint, position),1);
    }

    public Vector3 GetColor(Solid solid, Vector3 intersectionPoint, Ray ray)
    {
      Vector3 normal = solid.GetNormal(intersectionPoint, ray.position);
      Vector3 lightVector = VectorCalculator.GetVectorBetween2Points(intersectionPoint, position);
      Vector3 difuseComponent = solid.color;
      difuseComponent *= intensity;
      difuseComponent *= solid.material.diffuse;
      difuseComponent *= Math.Max(0, Vector3.Dot(normal, lightVector));

      Vector3 reflectVec = -Vector3.Reflect(lightVector, normal);
      Vector3 reflectionComponent = color;
      reflectionComponent *= intensity;
      reflectionComponent *= solid.material.reflection;
      float cosBeta = Math.Max(Vector3.Dot(normal, reflectVec), 0);
      float powerCosB = (float)Math.Pow(cosBeta, solid.material.reflectionSize);
      reflectionComponent *= powerCosB;

      Vector3 returnVec = reflectionComponent + difuseComponent;

      returnVec *= SecondShade(intersectionPoint);

      return returnVec;
    }
  }
}


