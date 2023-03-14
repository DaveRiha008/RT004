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

    public Material()
    {
      diffuse = (2.0f / 3.0f);
      reflection = (11.0f / 36.0f);
      ambient = (1.0f / 36.0f);
      reflectionSize = 3;
  }
    public Material(float diffuse = (2.0f/3.0f), float reflection = (11.0f / 36.0f), float ambient = (1.0f/ 36.0f), float reflectionSize = 3)
    {
      this.diffuse = diffuse;
      this.reflection = reflection;
      this.ambient = ambient;
      this.reflectionSize = reflectionSize;
    }
  }
}


