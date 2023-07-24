using MathNet.Numerics.LinearAlgebra;
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
  /// Holds all information about any solid transformation
  /// </summary>
  struct TransformInfo
  {
    //Warning: All property names dependent on json config

    public float TranslateX { get; set; }
    public float TranslateY { get; set; }
    public float TranslateZ { get; set; }
    public float RotateX { get; set; }
    public float RotateY { get; set; }
    public float RotateZ { get; set; }



    /// <returns>Translation matrix which when applied to shape translates it by desired values</returns>
    public static Matrix<float> TranslationMatrix(float x, float y, float z)
    {
      float[,] array = new float[4, 4]
      { {1,0,0,0},
        {0,1,0,0},
        {0,0,1,0},
        {x,y,z,1} };
      return Matrix<float>.Build.DenseOfArray(array);
    }

    /// <param name="angle">Angle in degrees</param>
    /// <returns>Rotation matrix which when applied to shape rotates it by desired angle around X axis</returns>
    public static Matrix<float> RotationXMatrix(float angle)
    {
      double angleRad = angle * (Math.PI / 180);

      float cos = (float)Math.Cos(angleRad);
      float sin = (float)Math.Sin(angleRad);
      float[,] array = new float[4, 4]
      { {1,0,0,0},
        {0,cos,-sin,0},
        {0,sin,cos,0},
        {0,0,0,1} };
      return Matrix<float>.Build.DenseOfArray(array);
    }


    /// <param name="angle">Angle in degrees</param>
    /// <returns>Rotation matrix which when applied to shape rotates it by desired angle around Y axis</returns>
    public static Matrix<float> RotationYMatrix(float angle)
    {
      double angleRad = angle * (Math.PI / 180);

      float cos = (float)Math.Cos(angleRad);
      float sin = (float)Math.Sin(angleRad);
      float[,] array = new float[4, 4]
      { {cos,0,sin,0},
        {0,1,0,0},
        {-sin,0,cos,0},
        {0,0,0,1} };
      return Matrix<float>.Build.DenseOfArray(array);
    }

    /// <param name="angle">Angle in degrees</param>
    /// <returns>Rotation matrix which when applied to shape rotates it by desired angle around Z axis</returns>
    public static Matrix<float> RotationZMatrix(float angle)
    {
      double angleRad = angle * (Math.PI / 180);

      float cos = (float)Math.Cos(angleRad);
      float sin = (float)Math.Sin(angleRad);
      float[,] array = new float[4, 4]
      { {cos,-sin,0,0},
        {sin,cos,0,0},
        {0,0,1,0},
        {0,0,0,1} };
      return Matrix<float>.Build.DenseOfArray(array);
    }

    /// <summary>Makes a matrix out of this transform info</summary>
    /// <returns>Transformation matrix modifying shapes when multiplying their position</returns>
    public Matrix<float> GetTransformMatNew()
    {
      Matrix<float> resultMatrix = Matrix<float>.Build.DenseIdentity(4, 4);


      resultMatrix *= TranslationMatrix(TranslateX, TranslateY, TranslateZ);
      resultMatrix *= RotationXMatrix(RotateX);
      resultMatrix *= RotationYMatrix(RotateY);
      resultMatrix *= RotationZMatrix(RotateZ);

      return resultMatrix;
    }

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
}

