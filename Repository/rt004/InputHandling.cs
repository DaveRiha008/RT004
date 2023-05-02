#nullable enable

using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace rt004
{
  struct ImageParameters
  {
    public int width;
    public int height;
    public ImageParameters(int width, int height)
    {
      this.width = width;
      this.height = height;
    }
  }

  struct Shape
  {
    public Material material;

    public Shape(Material material)
    {
      this.material = material;
    }
  }



  class ConfigInputHandler
  {
    public StreamReader input;
    public ConfigInputHandler(StreamReader input)
    {
      this.input = input;
    }
    public KeyValuePair<string, string>? GetPair(out bool end)
    {
      
      string? line = input.ReadLine();
      if (line is null)
      {
        end = true;
        return null;
      }
      string[] splitLine = line.Split('=');
      if (splitLine[0][0] == '#')
      {
        end = false;
        return null;
      }
      if (splitLine.Length != 2)
      {
        if (splitLine.Length == 1 && splitLine[0] == "end") end = true;
        else end = false;
        return null;
      }
      var pair = new KeyValuePair<string, string>(splitLine[0], splitLine[1]);
      end = false;
      return pair;
    }
    public static void LoadConfig(StreamReader configStream, ref ImageParameters imagePar, out Camera camera, AllSolids solids, Lights lights)
    {

      using (configStream)
      {
        //ConfigInputHandler config = new ConfigInputHandler(configStream);
        string configText = configStream.ReadToEnd();
        using JsonDocument doc = JsonDocument.Parse(configText);
        JsonElement root = doc.RootElement;

        LoadImagePar(root, imagePar);

        LoadCamera(root, out camera, imagePar);

        static void LoadNode(Shape shape, Matrix<float> tMat, JsonElement parentElement, AllSolids solids)
        {
          var M = Matrix<float>.Build;

          var matrix = M.Dense(4, 4);
          
          bool is_leaf = parentElement.GetProperty("is leaf").GetBoolean();
          if (is_leaf)
          {
            JsonElement sphere;
            if (parentElement.TryGetProperty("sphere", out sphere))
            {
              LoadSphere(solids, sphere, tMat);
            }
                                                                           //Leaf = load solids inside
            JsonElement plane;
            if (parentElement.TryGetProperty("plane", out plane))
            {
              LoadPlane(solids, plane, tMat);
            }
          }

          else
          {
            var transform = parentElement.GetProperty("transform");
            var newTMat = tMat * GetTransformMat(transform);

            var childNodes = parentElement.GetProperty("child nodes").EnumerateArray();
            foreach ( var childNode in childNodes )
            {
              LoadNode(shape, newTMat, childNode, solids);
            }




          }
        }

        var givenShapes = root.GetProperty("shapes").EnumerateArray();
        foreach (var givenShape in givenShapes)
        {

        }

        var givenSolids = root.GetProperty("solids");
        var givenSpheres = givenSolids.GetProperty("spheres").EnumerateArray();
        foreach (var sphere in givenSpheres)
        {
          LoadSphere(solids, sphere);
        }

        var givenPlanes = givenSolids.GetProperty("planes").EnumerateArray();
        foreach (var plane in givenPlanes)
        {
          LoadPlane(solids, plane);
        }


        var givenLights = root.GetProperty("lights");
        var givenAmbient = givenLights.GetProperty("ambient light");
        lights.AddAmbientLight((float)givenAmbient.GetProperty("intensity").GetDouble());

        var givenLightSources = givenLights.GetProperty("light sources").EnumerateArray();
        foreach ( var light in givenLightSources)
        {
          LoadLight(lights, light);
        }
      }
    }

    static void LoadImagePar(JsonElement root, ImageParameters imagePar)
    {
      var givenImagePar = root.GetProperty("image parameters");
      imagePar.width = givenImagePar.GetProperty("width").GetInt32();
      imagePar.height = givenImagePar.GetProperty("height").GetInt32();
    }

    static void LoadCamera(JsonElement root, out Camera camera, ImageParameters imagePar)
    {
      var givenCamera = root.GetProperty("camera");
      var givenCameraPos = givenCamera.GetProperty("position");
      Vector3 newCameraPos = new Vector3(
        (float)givenCameraPos.GetProperty("x").GetDouble(),
        (float)givenCameraPos.GetProperty("y").GetDouble(),
        (float)givenCameraPos.GetProperty("z").GetDouble()
        );

      var givenCameraViewVec = givenCamera.GetProperty("view vector");
      Vector3 newCameraViewVec = new Vector3(
        (float)givenCameraViewVec.GetProperty("x").GetDouble(),
        (float)givenCameraViewVec.GetProperty("y").GetDouble(),
        (float)givenCameraViewVec.GetProperty("z").GetDouble()
        );

      var givenCameraUpVec = givenCamera.GetProperty("up vector");
      Vector3 newCameraUpVec = new Vector3(
        (float)givenCameraUpVec.GetProperty("x").GetDouble(),
        (float)givenCameraUpVec.GetProperty("y").GetDouble(),
        (float)givenCameraUpVec.GetProperty("z").GetDouble()
        );
      double newCameraAspectRatio = (double)imagePar.width / (double)imagePar.height;
      camera = new Camera(newCameraPos, newCameraViewVec, newCameraUpVec, aspectRatio: newCameraAspectRatio);
    }

    static void LoadSphere(AllSolids solids, JsonElement sphere, Matrix<float>? transformMatrix = null)
    {
      if (transformMatrix is null) transformMatrix = Matrix<float>.Build.DiagonalIdentity(4, 4);

      var pos = sphere.GetProperty("position");
      Vector3 newSpherePos = new Vector3(
        (float)pos.GetProperty("x").GetDouble(),
        (float)pos.GetProperty("y").GetDouble(),
        (float)pos.GetProperty("z").GetDouble()
        );

      float radius = (float)sphere.GetProperty("radius").GetDouble();

      var color = sphere.GetProperty("color");
      Vector3 newSphereColor = new Vector3(
        (float)color.GetProperty("r").GetDouble(),
        (float)color.GetProperty("g").GetDouble(),
        (float)color.GetProperty("b").GetDouble()
        );

      var material = sphere.GetProperty("material");
      Material newSphereMaterial = new Material(
        (float)material.GetProperty("diffuse coef").GetDouble(),
        (float)material.GetProperty("reflection coef").GetDouble(),
        (float)material.GetProperty("ambient coef").GetDouble(),
        (float)material.GetProperty("reflection size (exponent)").GetDouble()
        );
      Sphere newSphere = new Sphere(newSpherePos, radius, newSphereColor, newSphereMaterial);
      newSphere.Transform(transformMatrix);
      solids.AddSolid(newSphere);
    }

    static void LoadPlane(AllSolids solids, JsonElement plane, Matrix<float>? transformMatrix = null)
    {
      if (transformMatrix is null) transformMatrix = Matrix<float>.Build.DiagonalIdentity(4, 4);

      var pos = plane.GetProperty("position");
      Vector3 newPlanePos = new Vector3(
        (float)pos.GetProperty("x").GetDouble(),
        (float)pos.GetProperty("y").GetDouble(),
        (float)pos.GetProperty("z").GetDouble()
        );

      var normal = plane.GetProperty("normal");
      Vector3 newPlaneNormal = new Vector3(
        (float)normal.GetProperty("x").GetDouble(),
        (float)normal.GetProperty("y").GetDouble(),
        (float)normal.GetProperty("z").GetDouble()
        );

      var color = plane.GetProperty("color");
      Vector3 newPlaneColor = new Vector3(
        (float)color.GetProperty("r").GetDouble(),
        (float)color.GetProperty("g").GetDouble(),
        (float)color.GetProperty("b").GetDouble()
        );

      var material = plane.GetProperty("material");
      Material newPlaneMaterial = new Material(
        (float)material.GetProperty("diffuse coef").GetDouble(),
        (float)material.GetProperty("reflection coef").GetDouble(),
        (float)material.GetProperty("ambient coef").GetDouble(),
        (float)material.GetProperty("reflection size (exponent)").GetDouble()
        );

      Plane newPlane = new Plane(newPlanePos, newPlaneNormal, newPlaneColor, newPlaneMaterial);
      newPlane.Transform(transformMatrix);
      solids.AddSolid(newPlane);
    }

    static void LoadLight(Lights lights, JsonElement light)
    {
      var pos = light.GetProperty("position");
      Vector3 newLightPos = new Vector3(
        (float)pos.GetProperty("x").GetDouble(),
        (float)pos.GetProperty("y").GetDouble(),
        (float)pos.GetProperty("z").GetDouble()
        );

      var color = light.GetProperty("color");
      Vector3 newLightColor = new Vector3(
        (float)color.GetProperty("r").GetDouble(),
        (float)color.GetProperty("g").GetDouble(),
        (float)color.GetProperty("b").GetDouble()
        );

      float newLightIntensity = (float)light.GetProperty("intensity").GetDouble();

      lights.AddLight(newLightPos, newLightColor, newLightIntensity);
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
      float cos = (float)Math.Cos(angle);
      float sin = (float)Math.Sin(angle);
      float[,] array = new float[4, 4]
      { {1,0,0,0},
        {0,cos,-sin,0},
        {0,sin,cos,0},
        {0,0,0,1} };
      return Matrix<float>.Build.DenseOfArray(array);
    }

    static Matrix<float> RotationYMatrix(float angle)
    {
      float cos = (float)Math.Cos(angle);
      float sin = (float)Math.Sin(angle);
      float[,] array = new float[4, 4]
      { {cos,0,sin,0},
        {0,1,0,0},
        {-sin,0,cos,0},
        {0,0,0,1} };
      return Matrix<float>.Build.DenseOfArray(array);
    }

    static Matrix<float> RotationZMatrix(float angle)
    {
      float cos = (float)Math.Cos(angle);
      float sin = (float)Math.Sin(angle);
      float[,] array = new float[4, 4]
      { {cos,-sin,0,0},
        {sin,cos,0,0},
        {0,0,1,0},
        {0,0,0,1} };
      return Matrix<float>.Build.DenseOfArray(array);
    }

    static Matrix<float> GetTransformMat(JsonElement transform)
    {
      float translX = (float)transform.GetProperty("translate x").GetDouble();
      float translY = (float)transform.GetProperty("translate y").GetDouble();
      float translZ = (float)transform.GetProperty("translate z").GetDouble();
      float rotX = (float)transform.GetProperty("rotate x").GetDouble();
      float rotY = (float)transform.GetProperty("rotate x").GetDouble();
      float rotZ = (float)transform.GetProperty("rotate x").GetDouble();

      Matrix<float> resultMatrix = Matrix<float>.Build.DenseIdentity(4, 4);
      resultMatrix *= TranslationMatrix(translX, translY, translZ);
      resultMatrix *= RotationXMatrix(rotX);
      resultMatrix *= RotationYMatrix(rotY);
      resultMatrix *= RotationZMatrix(rotZ);

      return resultMatrix;
    }


  }
}



