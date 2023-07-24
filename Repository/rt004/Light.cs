#nullable enable

using System.Numerics;
using System.Text.Json.Serialization;

namespace rt004
{
  /// <summary>
  /// Holds information about all lighting a scene needs
  /// </summary>
  class AllLights
  {
    Dictionary<int, LightSource> lightSources = new();
    AmbientLight ambientLight = new AmbientLight(0.5f);
    int currentIndex = 0;

    /// <summary>
    /// Adds a light source and stores it for future changes
    /// </summary>
    /// <param name="position">Posision of the light source</param>
    /// <param name="color">Color of the light source</param>
    /// <param name="intensity">Intensity of the light source</param>
    /// <returns>Index of the light source in local storage</returns>
    public int AddLight(Vector3 position, Vector3 color, float intensity)
    {
      lightSources.Add(currentIndex, new LightSource(position, color, intensity));
      currentIndex++;
      return currentIndex-1;
    }

    /// <summary>
    /// Removes light from local storage
    /// </summary>
    /// <param name="index">Index of the removed light</param>
    public void RemoveLight(int index)
    {
      lightSources.Remove(index);
    }

    /// <summary>
    /// Adds a unique light source which is present everywhere
    /// </summary>
    /// <param name="intensity">Intensity of the ambient light</param>
    public void AddAmbientLight(float intensity)
    {
      this.ambientLight = new AmbientLight(intensity);
    }


    /// <summary>
    /// Calculates color in given intersection point on given solid with all lights present
    /// </summary>
    /// <param name="solid">Solid with which the ray intersected</param>
    /// <param name="intersectionPoint">Position where ray intersected the solid</param>
    /// <param name="ray">Ray which intersected the solid</param>
    /// <param name="solids">All solids in current scene - for shadow purposes</param>
    /// <returns>Calculated color - solid appears this color in the intersection point</returns>
    /// <exception cref="NullReferenceException">Thrown when passed solid has no color</exception>
    public Vector3 GetColor(Solid solid, Position3D intersectionPoint, Ray ray, AllSolids solids)
    {
      Vector3 lightsComponent = Vector3.Zero;
      foreach (var light in lightSources.Values)
      {
        lightsComponent += light.GetColor(solid, intersectionPoint, ray, solids);
      }
      if (solid.Color is null) throw new NullReferenceException("Color of a solid is null!");
      Vector3 ambientComponent = solid.Color * solid.Material.Ambient * ambientLight.intensity;

      return lightsComponent + ambientComponent;
    }

  }

  /// <summary>
  /// Unique light in scene, which is present everywhere
  /// </summary>
  struct AmbientLight
  {
    public float intensity { get; private set; }

    [JsonConstructor]
    public AmbientLight(float intensity)
    {
      this.intensity = intensity;
    }
  }


  /// <summary>
  /// <para>
  /// One light source in scene.
  /// </para>
  /// Defined by: Position, color, intensity
  /// </summary>
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


    /// <summary>
    /// <para>Calculates coeficient of light based on distance from intersection to light source.</para>
    /// <para>If any solid is in the way - shadowed - coef is 0</para>
    /// </summary>
    /// <param name="intersectionPoint">Position of intersection of ray with solid</param>
    /// <param name="solids">All solids in current scene which can throw shadows</param>
    /// <returns>Coeficient of light, 0 if a solid is in the way</returns>
    private float SecondShade(Position3D intersectionPoint, AllSolids solids)
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
      return 1f/(float)Math.Pow(VectorCalculator.GetDistance(intersectionPoint, Position),1);
    }


    /// <summary>
    /// Calculates color in given intersection point on given solid with only this light present
    /// </summary>
    /// <param name="solid">Solid with which the ray intersected</param>
    /// <param name="intersectionPoint">Position where ray intersected the solid</param>
    /// <param name="ray">Ray which intersected the solid</param>
    /// <param name="solids">All solids in current scene - for shadow purposes</param>
    /// <returns>Calculated color - solid appears this color in the intersection point</returns>
    /// <exception cref="NullReferenceException">Thrown when passed solid has no color</exception>
    public Vector3 GetColor(Solid solid, Vector3 intersectionPoint, Ray ray, AllSolids solids)
    {
      Vector3 normal = solid.GetNormal(intersectionPoint, ray.position);
      Vector3 lightVector = VectorCalculator.GetVectorBetween2Points(intersectionPoint, Position);

      if (solid.Color is null) throw new NullReferenceException("Color of a solid is null!");
      Vector3 difuseComponent = solid.Color;
      difuseComponent *= Intensity;
      difuseComponent *= solid.Material.Diffuse;
      difuseComponent *= Math.Max(0, Vector3.Dot(normal, lightVector));

      Vector3 reflectVec = -Vector3.Reflect(lightVector, normal);
      Vector3 reflectionComponent = Color;
      reflectionComponent *= Intensity;
      reflectionComponent *= solid.Material.Reflection;
      float cosBeta = Math.Max(Vector3.Dot(normal, reflectVec), 0);
      float powerCosB = (float)Math.Pow(cosBeta, solid.Material.ReflectionSize);
      reflectionComponent *= powerCosB;

      Vector3 returnVec = reflectionComponent + difuseComponent;

      returnVec *= SecondShade(intersectionPoint, solids);

      return returnVec;
    }
  }
}


