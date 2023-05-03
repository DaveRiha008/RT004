//using System.Numerics;
#nullable enable

using Microsoft.VisualBasic;
using System;
using System.Drawing;
using System.Dynamic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace rt004
{

  //diffuse 0..1
  //reflection 0..1
  //ambient 0..1
  //diffuse+reflection+ambient = 1
  //reflectionSize = 5..500
  //transparency = 0 ... 1
  struct Material
  {
    public float diffuse;
    public float reflection;
    public float ambient;
    public float reflectionSize;
    public float transparency;
    public float refractionIndex;
    const float diffuseBase = 0.4f;
    const float reflectionBase = 0.55f;
    const float ambientBase = 0.05f;
    const float reflSizeBase = 155f;
    const float transpBase = 0;
    const float refractionIndexBase = 1;
    public Material()
    {
      diffuse = diffuseBase;
      reflection = reflectionBase;
      ambient = ambientBase;
      reflectionSize = reflSizeBase;
      transparency = transpBase;
      refractionIndex = refractionIndexBase;
    }
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


