//using System.Numerics;
#nullable enable

using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;

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

        var givenImagePar = root.GetProperty("image parameters");
        imagePar.width = givenImagePar.GetProperty("width").GetInt32();
        imagePar.height = givenImagePar.GetProperty("height").GetInt32();


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
        camera = new Camera(newCameraPos, newCameraViewVec, newCameraUpVec, aspectRatio:newCameraAspectRatio);


        var givenSolids = root.GetProperty("solids");
        var givenSpheres = givenSolids.GetProperty("spheres").EnumerateArray();
        foreach (var sphere in givenSpheres)
        {
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
          solids.AddSolid(newSphere);
        }

        var givenPlanes = givenSolids.GetProperty("planes").EnumerateArray();
        foreach (var plane in givenPlanes)
        {
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
          solids.AddSolid(newPlane);
        }


        var givenLights = root.GetProperty("lights");
        var givenAmbient = givenLights.GetProperty("ambient light");
        lights.AddAmbientLight((float)givenAmbient.GetProperty("intensity").GetDouble());

        var givenLightSources = givenLights.GetProperty("light sources").EnumerateArray();
        foreach ( var light in givenLightSources)
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






        //KeyValuePair<string, string>? pair = new KeyValuePair<string, string>();
        //bool end = false;
        //while (!end)
        //{
        //  pair = config.GetPair(out end);
        //  if (pair == null) continue;
        //  configOptions.Add(pair.Value.Key, pair.Value.Value);
        //}
      }
    }
  }



}


