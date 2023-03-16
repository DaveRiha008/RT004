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
  struct Material
  {
    public float diffuse;
    public float reflection;
    public float ambient;
    public float reflectionSize;
    const float diffuseBase = 0.5f;
    const float reflectionBase = 0.45f;
    const float ambientBase = 0.05f;
    const float reflSizeBase = 155f;
    public Material()
    {
      diffuse = diffuseBase;
      reflection = reflectionBase;
      ambient = ambientBase;
      reflectionSize = reflSizeBase;
  }
    public Material(float diffuse = diffuseBase, float reflection = reflectionBase, float ambient = ambientBase, float reflectionSize = reflSizeBase)
    {
      this.diffuse = diffuse;
      this.reflection = reflection;
      this.ambient = ambient;
      this.reflectionSize = reflectionSize;
    }
  }
}


