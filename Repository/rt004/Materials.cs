#nullable enable

namespace rt004
{

  //diffuse 0..1
  //reflection 0..1
  //ambient 0..1
  //diffuse+reflection+ambient = 1
  //reflectionSize = 5..500
  //transparency = 0 ... 1
  /// <summary>
  /// <para>Defines a material of any object</para>
  /// </summary>
  class Material
  {
    ///<value>
    ///Diffuse coeficient of the material - how much does it consume light
    ///<para>Should have a value in range 0..1</para>
    ///</value>
    public float Diffuse { get; set; }
    ///<value>
    ///Reflection coeficient of the material - how much does it reflect light
    ///<para>Should have a value in range 0..1</para>
    ///</value>
    public float Reflection { get; set; }
    ///<value>
    ///Ambient coeficient of the material - how much is it affected by ambient light of the scene
    ///<para>Should have a value in range 0..1</para>
    ///</value>
    public float Ambient { get; set; }
    ///<value>
    ///Reflection size (exponent) of the material - how widely does it spread reflected light (higher value = sharper reflection)
    ///<para>Should have a value in range 5..500</para>
    ///</value>
    public float ReflectionSize { get; set; }
    ///<value>
    ///Transparency of the material - how much light can pass through 
    ///<para>Should have a value in range 0..1</para>
    ///</value>
    public float Transparency { get; set; }
    ///<value>
    ///Refraction index of the material - density of its environment
    ///<para>See online tables for specific values (vacuum = 1)</para>
    /// </value>
    public float RefractionIndex { get; set; }
    const float diffuseBase = 0.4f;
    const float reflectionBase = 0.55f;
    const float ambientBase = 0.05f;
    const float reflSizeBase = 155f;
    const float transpBase = 0;
    const float refractionIndexBase = 1;
    public Material(float diffuse = diffuseBase, float reflection = reflectionBase, float ambient = ambientBase, float reflectionSize = reflSizeBase, float transparency = transpBase, float refractionIndex = refractionIndexBase)
    {
      this.Diffuse = diffuse;
      this.Reflection = reflection;
      this.Ambient = ambient;
      this.ReflectionSize = reflectionSize;
      this.Transparency = transparency;
      this.RefractionIndex = refractionIndex;
    }
  }
}


