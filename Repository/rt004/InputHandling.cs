#nullable enable

using System.Text.Json;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;

namespace rt004
{
  /// <summary>
  /// Data holder for JSON deserializer - holds all information required for scene creation
  /// </summary>
  class ConfigData
  {
    //Warning: All property names dependent on json config
    public ImageParameters? ImageParameters { get; set; } 
    public Camera? Camera { get; set; }

    public ShapeNode[] Shapes { get; set; } = new ShapeNode[0];

    public LightsInfo? Lights { get; set; }

    public AnimationInfo? Animation { get; set; }

  }

  /// <summary>
  /// Handles all json config deserialization process
  /// </summary>
  static class ConfigInputHandler
  {

    /// <summary>
    /// Main method - loads desired config from stream and saves them in scene
    /// </summary>
    /// <param name="configStream">Input config stream - json format</param>
    /// <param name="scene">Scene in which all the information is saved</param>
    /// <exception cref="JsonException">Thrown when failed to deserialize json document</exception>
    /// <exception cref="PropertyNotDescribedException">Thrown when some crucial information is missing in the config file</exception>
    public static void LoadConfig(StreamReader configStream, Scene scene)
    {
      string configText = configStream.ReadToEnd();
      using JsonDocument doc = JsonDocument.Parse(configText);

      ConfigData? data = (ConfigData?)JsonSerializer.Deserialize(doc, typeof(ConfigData), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true, IncludeFields = true });
      if (data is null) throw new JsonException("Error when deserializing json document");
      JsonElement root = doc.RootElement;


      if (data.ImageParameters is null) Console.WriteLine("Warning: No image parameters found - using default values");
      LoadImagePar(data.ImageParameters, scene);


      if (data.Camera is not null) LoadCamera(data.Camera, scene);
      else if (data.Animation is null)throw new PropertyNotDescribedException("Camera data not found");

      if (data.Shapes.Length <= 0) Console.WriteLine("Warning: No shapes found - intended?");
      ShapeHierarchy.LoadShapes(data.Shapes, scene);

      if (data.Lights is null) Console.WriteLine("Warning: No lights information found - intended?");
      LoadLight(data.Lights, scene);

      if (data.Animation is null) Console.WriteLine("No animation info found - creating only one image");
      LoadAnimation(scene, data.Animation);
    }
  

    /// <summary>
    /// Simply passes animation info to scene
    /// </summary>
    static void LoadAnimation(Scene scene, AnimationInfo? animationInfo)
    {
      scene.animationInfo = animationInfo;
    }

    /// <summary>
    /// Simply passes image parameters to scene
    /// </summary>
    static void LoadImagePar(ImageParameters? imageParameters, Scene scene)
    {
      if (imageParameters is null) scene.imageParameters = new ImageParameters();
      else scene.imageParameters = (ImageParameters)imageParameters;
    }

    /// <summary>
    /// Simply passes camera to scene and edits crucial camera parameters based on Image parameters
    /// </summary>
    static void LoadCamera(Camera camera, Scene scene)
    {
      scene.camera = camera;
      scene.camera.AspectRatio = (float)scene.imageParameters.Width / (float)scene.imageParameters.Height;
    }

    /// <summary>
    /// Handles all tranformations and material passing in shape hierarchy
    /// </summary>
    class ShapeHierarchy
    {
      /// <summary>
      /// Main method -
      /// Loads all given shapes -
      /// Each shape represents one hierarchy tree
      /// </summary>
      /// <param name="shapes">Trees to be loaded</param>
      /// <param name="scene">Scene to which they are loaded</param>
      public static void LoadShapes(ShapeNode[] shapes, Scene scene)
      {
        foreach (ShapeNode shape in shapes)
        {
          LoadShape(shape, scene);
        }
      }

      /// <summary>
      /// Loads one shape tree
      /// </summary>
      static void LoadShape(ShapeNode rootNode, Scene scene)
      {
        Matrix<float> transMatrix = Matrix<float>.Build.DenseIdentity(4, 4);
        if (rootNode.Transform is not null) transMatrix *= ((TransformInfo)rootNode.Transform).GetTransformMatNew(); 
        if (rootNode.Sphere is not null)
          LoadSphere(rootNode, transMatrix, scene);
        if (rootNode.Plane is not null)
          LoadPlane(rootNode, transMatrix, scene);
        foreach (ShapeNode child in rootNode.ChildNodes)
        {
          LoadNodeChild(rootNode, child, transMatrix, scene);
        }
      }

      /// <summary>
      /// Loads one child node - can inherit material, color, transform matrix
      /// </summary>
      /// <param name="parent">Parent node</param>
      /// <param name="child">Node to be loaded</param>
      /// <param name="tMat">Tranformation matrix from parent</param>
      /// <param name="scene">Scene to which all is loaded</param>
      static void LoadNodeChild(ShapeNode parent, ShapeNode child, Matrix<float> tMat, Scene scene)
      {

        Matrix<float> transMatrix = tMat;
        if(child.Transform is not null) transMatrix *= ((TransformInfo)child.Transform).GetTransformMatNew();
        if (child.Material is null)
          child.Material = parent.Material;
        if (child.Color is null)
          child.Color = parent.Color;
        if (child.Sphere is not null)
          LoadSphere(child, transMatrix, scene);
        if (child.Plane is not null)
          LoadPlane(child, transMatrix, scene);
        foreach (ShapeNode child2 in child.ChildNodes)
        {
          LoadNodeChild(child, child2, transMatrix, scene);
        }
      }

      /// <summary>
      /// Loads sphere to scene based on node information
      /// </summary>
      /// <param name="node">Node in which Sphere is described</param>
      /// <param name="tMat">Transform matrix applied to sphere</param>
      /// <param name="scene">Scene to which sphere is loaded</param>
      /// <exception cref="NullReferenceException">Thrown when passed node doesn't have any Sphere</exception>
      /// <exception cref="PropertyNotDescribedException">Thrown when a crucial information about sphere is missing</exception>
      static void LoadSphere(ShapeNode node, Matrix<float> tMat, Scene scene)
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

      /// <summary>
      /// Loads plane to scene based on node information
      /// </summary>
      /// <param name="node">Node in which Plane is described</param>
      /// <param name="tMat">Transform matrix applied to plane</param>
      /// <param name="scene">Scene to which plane is loaded</param>
      /// <exception cref="NullReferenceException">Thrown when passed node doesn't have any Plane</exception>
      /// <exception cref="PropertyNotDescribedException">Thrown when a crucial information about plane is missing</exception>
      static void LoadPlane(ShapeNode node, Matrix<float> tMat, Scene scene)
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

    /// <summary>
    /// Loads all lighting to scene
    /// </summary>

    static void LoadLight(LightsInfo? lightsInfo, Scene scene)
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




  }
}



