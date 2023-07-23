//using System.Numerics;
#nullable enable

using System.Numerics;
using System.Text.Json.Serialization;

namespace rt004
{
  class AllLights
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

    public Vector3 GetColor(Solid solid, Vector3 intersectionPoint, Ray ray, AllSolids solids)
    {
      Vector3 lightsComponent = Vector3.Zero;
      foreach (var light in lightSources.Values)
      {
        lightsComponent += light.GetColor(solid, intersectionPoint, ray, solids);
      }
      if (solid.Color is null) throw new NullReferenceException("Color of a solid is null!");
      Vector3 ambientComponent = solid.Color * solid.Material.ambient * ambientLight.intensity;

      return lightsComponent + ambientComponent;
    }

  }

  struct AmbientLight
  {
    public float intensity { get; private set; }

    [JsonConstructor]
    public AmbientLight(float intensity)
    {
      this.intensity = intensity;
    }
  }

  class LightSource
  {
    //Warning: All property names dependent on json config
    public float Intensity { get; private set; }
    public Color Color { get; private set; }
    public Position3D Position { get; private set; }
    public LightSource(Position3D position, Color color, float intensity)
    {
      this.Color = color;
      this.Position = position;
      this.Intensity = intensity;
    }

    private float SecondShade(Vector3 intersectionPoint, AllSolids solids)
    {
      Vector3 intersectionLightVector = VectorCalculator.GetVectorBetween2Points(intersectionPoint, Position);
      Ray ray = new Ray(intersectionPoint, intersectionLightVector);
      double? t;
      Solid? solid;
      solids.GetClosestIntersection(ray, out t, out solid);
      if ((t is not null || solid is not null) && (t > Constants.epsilon || t < -Constants.epsilon))
      {
        return 0;
      }
      return 1f/(float)Math.Pow(DistanceCalculator.GetDistance(intersectionPoint, Position),1);
    }

    public Vector3 GetColor(Solid solid, Vector3 intersectionPoint, Ray ray, AllSolids solids)
    {
      Vector3 normal = solid.GetNormal(intersectionPoint, ray.position);
      Vector3 lightVector = VectorCalculator.GetVectorBetween2Points(intersectionPoint, Position);

      if (solid.Color is null) throw new NullReferenceException("Color of a solid is null!");
      Vector3 difuseComponent = solid.Color;
      difuseComponent *= Intensity;
      difuseComponent *= solid.Material.diffuse;
      difuseComponent *= Math.Max(0, Vector3.Dot(normal, lightVector));

      Vector3 reflectVec = -Vector3.Reflect(lightVector, normal);
      Vector3 reflectionComponent = Color;
      reflectionComponent *= Intensity;
      reflectionComponent *= solid.Material.reflection;
      float cosBeta = Math.Max(Vector3.Dot(normal, reflectVec), 0);
      float powerCosB = (float)Math.Pow(cosBeta, solid.Material.reflectionSize);
      reflectionComponent *= powerCosB;

      Vector3 returnVec = reflectionComponent + difuseComponent;

      returnVec *= SecondShade(intersectionPoint, solids);

      return returnVec;
    }
  }
}


