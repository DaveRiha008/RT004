#nullable enable

namespace rt004
{

  //diffuse 0..1
  //reflection 0..1
  //ambient 0..1
  //diffuse+reflection+ambient = 1
  //reflectionSize = 5..500
  //transparency = 0 ... 1
  class Material
  {
    public float diffuse { get; set; }
    public float reflection { get; set; }
    public float ambient { get; set; }
    public float reflectionSize { get; set; }
    public float transparency { get; set; }
    public float refractionIndex { get; set; }
    const float diffuseBase = 0.4f;
    const float reflectionBase = 0.55f;
    const float ambientBase = 0.05f;
    const float reflSizeBase = 155f;
    const float transpBase = 0;
    const float refractionIndexBase = 1;
    public Material(float diffuse = diffuseBase, float reflection = reflectionBase, float ambient = ambientBase, float reflectionSize = reflSizeBase, float transparency = transpBase, float refractionIndex = refractionIndexBase)
    {
      this.diffuse = diffuse;
      this.reflection = reflection;
      this.ambient = ambient;
      this.reflectionSize = reflectionSize;
      this.transparency = transparency;
      this.refractionIndex = refractionIndex;
    }
  }
}


