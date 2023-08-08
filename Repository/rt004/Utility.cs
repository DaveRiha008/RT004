using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using System.Diagnostics.Metrics;
using System.Numerics;
using System.Text.Json.Serialization;

namespace rt004
{
  /// <summary>
  /// Simpler version of Vector3 describing only position in a 3D world
  /// </summary>
  /// <remarks>Implicitly convertible on Vector3</remarks>
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

  /// <summary>
  /// Simpler version of Vector3 describing only RGB Color
  /// </summary>
  /// <remarks>Implicitly convertible on Vector3</remarks>
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


  /// <summary>
  /// Holds all information about final image
  /// </summary>
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

  /// <summary>
  /// Support type in Shape hierarchy - holds all information about one node
  /// </summary>
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

  

  /// <summary>
  /// Holds all information about scene lighting
  /// <para>Unlike AllLights - compatible with JSON deserialization, but not with scene</para>
  /// </summary>
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

  /// <summary>
  /// Holds all information required to create an animation
  /// </summary>
  struct AnimationInfo
  {    
    //Warning: All property names dependent on json config
    /// <summary>
    /// Frames per second of the animation
    /// </summary>
    public int Fps { get; set; }
    /// <summary>
    /// How long will the animation be (in seconds)
    /// </summary>
    public float AnimationLength { get; set; }
    /// <summary>
    /// Name of the file containing correctly formatted camera script (positions in time)
    /// </summary>
    public string CameraScript { get; set; }
  }



  /// <summary>
  /// Double version of 2D vector
  /// </summary>
  struct Vector2d
  {
    public double x { get; set; }
    public double y { get; set; }
  }

  /// <summary>
  /// Contains info about pixel and pixel segment sizes based on given spp and screen size
  /// </summary>
  struct ScenePixelsAndSegmentsInfo
  {
    public int segmentsInRow;
    public double pixelSizeX;
    public double pixelSizeY;
    public double segmentSizeX;
    public double segmentSizeY;
    public ScenePixelsAndSegmentsInfo(int spp, float sceneWidth, float sceneHeight)
    {
      segmentsInRow = (int)Math.Sqrt(spp);
      pixelSizeX = 2d / (double)sceneWidth;
      pixelSizeY = 2d / (double)sceneHeight;
      segmentSizeX = pixelSizeX / segmentsInRow;
      segmentSizeY = pixelSizeY / segmentsInRow;
    }
  }

  /// <summary>
  /// Holds information about one pixel segment - positions of its 4 borders
  /// </summary>
  struct PixelSegment
  {
    public Vector2d left { get; set; }
    public Vector2d right { get; set; }
    public Vector2d top { get; set; }
    public Vector2d bottom { get; set; }
  }



  //Copied from other project - 086shader 
  class Util
  {
    /// <summary>
    /// Converts a comma-separated list into a dictionary of [key,value] tuples.
    /// </summary>
    /// <param name="str">String to parse.</param>
    /// <param name="separator">Optional specification of the separator character.</param>
    public static Dictionary<string, string> ParseKeyValueList(string str, char separator = ',')
    {
      int len = str.Length;
      int start = 0;
      Dictionary<string, string> result = new Dictionary<string, string>();

      while (start < len)
      {
        int end = str.IndexOf(separator, start);
        if (end == start)
        {
          start++;
          continue;
        }

        if (end < 0) end = len;
        int eq = str.IndexOf('=', start);
        if (eq != start)
        {
          if (eq < 0 || eq > end) // only key (tag) is present, assume empty value..
            eq = end;
          string value = (eq < end - 1) ? str.Substring(eq + 1, end - eq - 1) : "";
          string key = str.Substring(start, eq - start);
          result[key.Trim()] = value.Trim();
        }

        start = end + 1;
      }
      return result;
    }
  }

}

