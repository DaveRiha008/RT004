using System.Numerics;
#nullable enable

using System.Security.Cryptography;

namespace rt004
{
   static class Scene
  {
    public static AllSolids solids = new AllSolids();

    public static Camera camera = new Camera(Vector3.Zero, Vector3.UnitZ);

    public static Lights lights = new Lights();
  }
  internal class Program
  {
    static Vector3 RayTrace(Ray ray0, Vector3 intersection0, Solid solid0, int recursionDepth)
    {
      Vector3 outColor = Vector3.Zero;
      Vector3 currentIntersection = intersection0;
      Solid currentSolid = solid0;
      Ray currentRay = ray0;
      
      for (int i = 0; i < recursionDepth; i++)
      {
        Vector3 reflectVec = Vector3.Reflect(currentRay.vector, currentSolid.GetNormal(currentIntersection));
        currentRay = new Ray(currentIntersection, reflectVec);
        double? t;
        Solid? newSolid;
        Scene.solids.GetClosestIntersection(currentRay, out t, out newSolid);
        if (newSolid is null || t is null) return outColor;
        currentIntersection = currentRay.PositionAfterT((float)t);
        outColor += (float)Math.Pow(5*currentSolid.material.reflection, 1) * Scene.lights.GetColor(currentSolid, currentIntersection);
        currentSolid = newSolid;
      }
      return outColor;
    }


    static void CreateHDRImage(FloatImage fi, ImageParameters imPar, Camera camera, AllSolids solids, Lights lights)
    {
      double? t;
      Solid? closestSolid;
      int rayIndex;
      for (int i = 0; i < imPar.width; i++)
      {
        for (int j = 0; j < imPar.height; j++)
        {
          //camera.SetPositionX(i); camera.SetPositionY(j); // linear projection

          float normalizedX = (-2.0f * i) / imPar.width + 1f;  //normalize for x and y to go from -1 to +1
          float normalizedY = (-2.0f * j) / imPar.height + 1f;

          rayIndex = camera.CreateRay(normalizedX, normalizedY); 
          Ray ray = camera.GetRay(rayIndex);

          float[] color = new float[3] { 1, 1, 1 };

          solids.GetClosestIntersection(ray, out t, out closestSolid);

          if (t is not null && t > 0 && closestSolid is not null)
          {
            Vector3 colorVec = lights.GetColor(closestSolid, ray.PositionAfterT((float)t));
            colorVec += RayTrace(ray, ray.PositionAfterT((float)t), closestSolid, 10);
            color = new float[3] { colorVec.X, colorVec.Y, colorVec.Z, };
          }

          fi.PutPixel(i, j, color);
          camera.RemoveRay(rayIndex);
        }
      }
    }

    static void Main(string[] args)
    {
      // Parameters.
      // TODO: parse command-line arguments and/or your config file.
      ImageParameters imPar = new ImageParameters(600, 450);
      string fileName = "demo.pfm";
      //Console.WriteLine("Input your config file name (path): ");
      //string? configFileName = Console.ReadLine();
      string? configFileName = "config.json";
      StreamReader configStream = new StreamReader(Console.OpenStandardInput());
      try
      {
        if (configFileName is null) throw new IOException(); 
        configStream = new StreamReader(configFileName);
      }
      catch
      {
        Console.WriteLine("Invalid config file name, reading from console - type end to finalize picture");
      }


      // HDR image.


      // TODO: put anything interesting into the image.
      // TODO: use fi.PutPixel() function, pixel should be a float[3] array [r, G, B]

      //     MANUAL PARAMETERS (IGNORE CONFIG)
      //imPar.width = 1920;
      //imPar.height = 1080;
      //solids.AddSolid(new Sphere(new Vector3(0.4f, 0.4f, 1.8f), 0.1f, new Vector3(0.5f, 0.5f, 0), new Material()));
      //solids.AddSolid(new Sphere(new Vector3(0.4f, 0.1f, 1f), 0.1f, new Vector3(1f, 0f, 0), new Material()));
      //solids.AddSolid(new Plane(new Vector3(-0.0f, 0.0f, 1f), Vector3.Normalize(new Vector3(0, 1, -0.2f)), new Vector3(0, 0.3f, 1), new Material(0.9f, 0.05f, 0.05f)));
      //lights.AddLight(new Vector3(-0.5f, 0.5f, 0.2f), Vector3.One, 2);
      //lights.AddLight(new Vector3(-0.2f, 0.1f, 1.0f), Vector3.One, 1);
      //lights.AddAmbientLight(0.5f);

      ConfigInputHandler.LoadConfig(configStream, ref imPar, out Scene.camera, Scene.solids, Scene.lights);

      FloatImage fi = new FloatImage(imPar.width, imPar.height, 3);

      CreateHDRImage(fi, imPar, Scene.camera, Scene.solids, Scene.lights);

      //fi.SaveHDR(fileName);   // Doesn't work well yet...
      fi.SavePFM(fileName);

      Console.WriteLine("HDR image is finished.");
    }
  }
}
