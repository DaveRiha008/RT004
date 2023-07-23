#nullable enable

using System.Text.Json;
using MathNet.Numerics.LinearAlgebra;

namespace rt004
{

  class ConfigData
  {
    //Warning: All property names dependent on json config
    public ImageParameters? ImageParameters { get; set; } 
    public Camera? Camera { get; set; }

    public ShapeNode[] Shapes { get; set; } = new ShapeNode[0];

    public LightsInfo? Lights { get; set; }


  }


  class ConfigInputHandler
  {

    public static void LoadConfig(StreamReader configStream, Scene scene)
    {
      string configText = configStream.ReadToEnd();
      using JsonDocument doc = JsonDocument.Parse(configText);

      ConfigData? data = (ConfigData?)JsonSerializer.Deserialize(doc, typeof(ConfigData), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true, IncludeFields = true });
      if (data is null) throw new JsonException("Error when deserializing json document");
      JsonElement root = doc.RootElement;


      if (data.ImageParameters is null) Console.WriteLine("Warning: No image parameters found - using default values");
      LoadImageParNew(data.ImageParameters, scene);


      if (data.Camera is not null) LoadCameraNew(data.Camera, scene);
      else throw new PropertyNotDescribedException("Camera data not found");

      if (data.Shapes.Length <= 0) Console.WriteLine("Warning: No shapes found - intended?");
      ShapeHierarchy.LoadShapes(data.Shapes, scene);

      if (data.Lights is null) Console.WriteLine("Warning: No lights information found - intended?");
      LoadLightNew(data.Lights, scene);
    }
  


    static void LoadImageParNew(ImageParameters? imageParameters, Scene scene)
    {
      if (imageParameters is null) scene.imageParameters = new ImageParameters();
      else scene.imageParameters = (ImageParameters)imageParameters;
    }


    static void LoadCameraNew(Camera camera, Scene scene)
    {
      scene.camera = camera;
      scene.camera.AspectRatio = (float)scene.imageParameters.Width / (float)scene.imageParameters.Height;
    }


    class ShapeHierarchy
    {
      public static void LoadShapes(ShapeNode[] shapes, Scene scene)
      {
        foreach (ShapeNode shape in shapes)
        {
          LoadShape(shape, scene);
        }
      }

      static void LoadShape(ShapeNode rootNode, Scene scene)
      {
        Matrix<float> transMatrix = Matrix<float>.Build.DenseIdentity(4, 4);
        transMatrix *= GetTransformMatNew(rootNode.Transform);
        if (rootNode.Sphere is not null)
          LoadSphereNew(rootNode, transMatrix, scene);
        if (rootNode.Plane is not null)
          LoadPlaneNew(rootNode, transMatrix, scene);
        foreach (ShapeNode child in rootNode.ChildNodes)
        {
          LoadNodeChild(rootNode, child, transMatrix, scene);
        }
      }

      static void LoadNodeChild(ShapeNode parent, ShapeNode child, Matrix<float> tMat, Scene scene)
      {
        Matrix<float> transMatrix = tMat * GetTransformMatNew(child.Transform);
        if (child.Material is null)
          child.Material = parent.Material;
        if (child.Color is null)
          child.Color = parent.Color;
        if (child.Sphere is not null)
          LoadSphereNew(child, transMatrix, scene);
        if (child.Plane is not null)
          LoadPlaneNew(child, transMatrix, scene);
        foreach (ShapeNode child2 in child.ChildNodes)
        {
          LoadNodeChild(child, child2, transMatrix, scene);
        }
      }


      static void LoadSphereNew(ShapeNode node, Matrix<float> tMat, Scene scene)
      {
        if (node.Sphere is null) throw new NullReferenceException("Sphere is null in LoadSphere function");

        if (node.Sphere.Color is null)
        {
          if (node.Color is not null)
            node.Sphere.Color = node.Color;
          else throw new PropertyNotDescribedException("Sphere color not found");
        }
        if (node.Sphere.Material is null)
        {
          if (node.Material is not null)
            node.Sphere.Material = node.Material;
          else throw new PropertyNotDescribedException("Sphere material not found");
        }
        node.Sphere.Transform(tMat);
        scene.solids.AddSolid(node.Sphere);
      }


      static void LoadPlaneNew(ShapeNode node, Matrix<float> tMat, Scene scene)
      {
        if (node.Plane is null) throw new NullReferenceException("Plane is null in LoadSphere function");


        if (node.Plane.Color is null)
        {
          if (node.Color is not null)
            node.Plane.Color = node.Color;
          else throw new PropertyNotDescribedException("Plane color not found");
        }
        if (node.Plane.Material is null)
        {
          if (node.Material is not null)
            node.Plane.Material = node.Material;
          else throw new PropertyNotDescribedException("Plane material not found");
        }
        node.Plane.Transform(tMat);
        scene.solids.AddSolid(node.Plane);
      }

    }


    static void LoadLightNew(LightsInfo? lightsInfo, Scene scene)
    {
      LightsInfo nonNullInfo;
      if (lightsInfo is null) nonNullInfo = new LightsInfo(); 
      else nonNullInfo = (LightsInfo)lightsInfo;

      scene.lights.AddAmbientLight(nonNullInfo.AmbientLight.intensity);

      if (nonNullInfo.LightSources.Length <= 0) Console.WriteLine("Warning: no light sources found - intended?");
      foreach (LightSource light in nonNullInfo.LightSources)
      {
        scene.lights.AddLight(light.Position, light.Color, light.Intensity);
      }
    }


    static Matrix<float> TranslationMatrix(float x, float y, float z)
    {
      float[,] array = new float[4, 4]
      { {1,0,0,0},
        {0,1,0,0},
        {0,0,1,0},
        {x,y,z,1} };
      return Matrix<float>.Build.DenseOfArray(array);
    }
    static Matrix<float> RotationXMatrix(float angle)
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

    static Matrix<float> RotationYMatrix(float angle)
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

    static Matrix<float> RotationZMatrix(float angle)
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

    static Matrix<float> GetTransformMatNew(TransformInfo? transformInfo)
    {
      Matrix<float> resultMatrix = Matrix<float>.Build.DenseIdentity(4, 4);
      if (transformInfo is null) return resultMatrix;

      TransformInfo nonNullInfo = (TransformInfo)transformInfo;

      resultMatrix *= TranslationMatrix(nonNullInfo.TranslateX, nonNullInfo.TranslateY, nonNullInfo.TranslateZ);
      resultMatrix *= RotationXMatrix(nonNullInfo.RotateX);
      resultMatrix *= RotationYMatrix(nonNullInfo.RotateY);
      resultMatrix *= RotationZMatrix(nonNullInfo.RotateZ);

      return resultMatrix;
    }


  }
}



