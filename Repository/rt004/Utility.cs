using System.Numerics;
using System.Text.Json.Serialization;

namespace rt004
{
  struct Position3D
  {
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }

    [JsonConstructor]
    public Position3D(float x, float y, float z)
    {
      this.x = x;
      this.y = y;
      this.z = z;
    }

    public static implicit operator Vector3(Position3D position) => new Vector3(position.x, position.y, position.z);
    public static implicit operator Position3D(Vector3 vector) => new Position3D { x = vector.X, y = vector.Y, z = vector.Z };

    public static Position3D operator *(Vector3 vector, Position3D pos) => vector * (Vector3)pos;
    public static Position3D operator *(Position3D pos, Vector3 vector) => vector * (Vector3)pos;
    public static Position3D operator *(Position3D pos1, Position3D pos2) => (Vector3)pos1 * (Vector3)pos2;
    public static Position3D operator *(float num, Position3D pos2) => num * (Vector3)pos2;
    public static Position3D operator *(Position3D pos2, float num) => num * (Vector3)pos2;

    public static Position3D operator +(Position3D pos1, Position3D pos2) => (Vector3)pos1 + (Vector3)pos2;
    public static Position3D operator +(Vector3 vector, Position3D pos) => vector + (Vector3)pos;
    public static Position3D operator +(Position3D pos, Vector3 vector) => vector + (Vector3)pos;
    public static Position3D operator /(Position3D pos, float num) => (Vector3)pos / num;

    public static Position3D operator -(Position3D pos1) => -(Vector3)pos1;

  }

  class Color
  {

    [JsonConstructor]
    public Color(float r, float g, float b)
    {
      this.r = r;
      this.g = g;
      this.b = b;
    }
    public float r { get; set; }
    public float g { get; set; }
    public float b { get; set; }

    public static implicit operator Vector3(Color color)
    {
      return new Vector3(color.r, color.g, color.b);
    }
    public static implicit operator Color(Vector3 vector)
    {
      return new Color (vector.X, vector.Y, vector.Z );
    }


    public static Color operator *(Vector3 vector, Color pos) => vector * (Vector3)pos;
    public static Color operator *(Color pos, Vector3 vector) => vector * (Vector3)pos;
    public static Color operator *(Color pos1, Color pos2) => (Vector3)pos1 * (Vector3)pos2;
    public static Color operator *(float num, Color pos2) => num * (Vector3)pos2;
    public static Color operator *(Color pos2, float num) => num * (Vector3)pos2;

    public static Color operator +(Color pos1, Color pos2) => (Vector3)pos1 + (Vector3)pos2;
    public static Color operator /(Color pos, float num) => (Vector3)pos / num;

    public static Color operator -(Color pos1) => -(Vector3)pos1;

  }


  //TODO: Warnings in code, that some property names are dependent on json
  struct ImageParameters
  {
    //Warning: All property names dependent on json config
    public int Width { get; set; } = 800; 
    public int Height { get; set; } = 600; 
    public int RtRecursionDepth { get; set; } = 10; 
    public int Spp { get; set; } = 10;  
    public bool Aa { get; set; } = true; 
    public Color BackgroundColor { get; set; } = new Color(0, 0, 0);
    public ImageParameters(int width, int height, int rtRecursionDepth, int spp, bool aa, Color backgroundColor)
    {
      this.Width = width;
      this.Height = height;
      this.RtRecursionDepth = rtRecursionDepth;
      this.Spp = spp;
      this.Aa = aa;
      this.BackgroundColor = backgroundColor;
    }
  }


  class ShapeNode
  {
    //Warning: All property names dependent on json config

    public TransformInfo? Transform { get; set; }

    public Sphere? Sphere { get; set; }
    public Plane? Plane { get; set; }
    public Material? Material { get; set; }
    public Color? Color { get; set; }
    public ShapeNode[] ChildNodes { get; set; } = new ShapeNode[0];
  }

  struct TransformInfo
  {
    //Warning: All property names dependent on json config

    public float TranslateX { get; set; }
    public float TranslateY { get; set; }
    public float TranslateZ { get; set; }
    public float RotateX { get; set; }
    public float RotateY { get; set; }
    public float RotateZ { get; set; }
  }


  struct LightsInfo
  {
    //Warning: All property names dependent on json config

    public AmbientLight AmbientLight { get; set; } = new AmbientLight(0);
    public LightSource[] LightSources { get; set; } = new LightSource[0];

    public LightsInfo() { }
    public LightsInfo(AmbientLight ambientLight, LightSource[] lightSources)
    {
      AmbientLight = ambientLight;
      LightSources = lightSources;
    }

  }






  struct Vector2d
  {
    public double x { get; set; }
    public double y { get; set; }
  }
  struct PixelSegment
  {
    public Vector2d left { get; set; }
    public Vector2d right { get; set; }
    public Vector2d top { get; set; }
    public Vector2d bottom { get; set; }
  }
}

