﻿using MathNet.Numerics.Random;
using System.Numerics;
#nullable enable

using System.Security.Cryptography;


namespace rt004
{
  public static class Constants
  {
    static public double epsilon = 1.0e-5;
  }
   static class Scene
  {
    public static AllSolids solids = new AllSolids();

    public static Camera camera = new Camera(Vector3.Zero, Vector3.UnitZ);

    public static Lights lights = new Lights();

    public static ImageParameters imageParameters = new ImageParameters();
  }
  internal class Program
  {
    static Vector3 RayTrace(Ray ray0, int currentRecursion)
    {      
      Ray currentRay = ray0;
      double? t;
      Solid? currentSolid;
      Scene.solids.GetClosestIntersection(currentRay, out t, out currentSolid);
      if (t is null || t <= 0 || currentSolid is null) return Scene.imageParameters.backgroundColor;
      Vector3 currentIntersection = currentRay.PositionAfterT((float)t);
      
      Vector3 outColor = Scene.lights.GetColor(currentSolid, currentIntersection, currentRay);



      if (currentRecursion > 0)
      {
        Vector3 reflectVec = Vector3.Reflect(currentRay.vector, currentSolid.GetNormal(currentIntersection, currentRay.position));

        Ray newReflectedRay = new Ray(currentIntersection, reflectVec);

        Vector3 rayTracedColor = RayTrace(newReflectedRay, currentRecursion-1);

        outColor += (float)Math.Pow(currentSolid.material.reflection, Scene.imageParameters.recursionDepth - currentRecursion + 1) * rayTracedColor;

        Vector3 furtherIntersection = currentRay.PositionAfterT((float)(t + Constants.epsilon));

        Solid? originSolid = Scene.solids.GetSolidOfAPoint(currentRay.position);
        double originRefrIndex = 1;
        if (originSolid is not null)
          originRefrIndex = originSolid.material.refractionIndex; 
        Solid? outsideSolid = Scene.solids.GetSolidOfAPoint(furtherIntersection);
        double outsideRefrIndex = 1;
        if (outsideSolid is not null) { outsideRefrIndex = outsideSolid.material.refractionIndex; }

        Vector3? refractVec = VectorCalculator.GetRefractedVector(originRefrIndex, outsideRefrIndex, currentSolid.GetNormal(currentIntersection, currentRay.position), currentRay.vector);
        if (refractVec is not null && currentSolid.material.transparency > 0)
        {

          Ray newRefractedRay = new Ray(furtherIntersection, (Vector3)refractVec);

          Vector3 refractedColor = RayTrace(newRefractedRay, currentRecursion-1);

          outColor += (float)Math.Pow(currentSolid.material.transparency, Scene.imageParameters.recursionDepth - currentRecursion + 1) * refractedColor;

        }


      }
      return outColor;
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

    static Vector3 AntiAlias(float x,  float y, Random random)
    {
      int spp = Scene.imageParameters.spp;
      int segmentsInRow = (int)Math.Sqrt(spp);
      double pixelSizeX = 2d / (double)Scene.imageParameters.width;
      double pixelSizeY = 2d / (double)Scene.imageParameters.height;
      double segmentSizeX = pixelSizeX / segmentsInRow;
      double segmentSizeY = pixelSizeY / segmentsInRow;

      List<PixelSegment> segments = new List<PixelSegment>();
      for (double i = (double)-segmentsInRow/2; i < (double)segmentsInRow/2; i++)
      {
        for (double j = (double)-segmentsInRow/2; j < (double)segmentsInRow/2; j++)
        {
          segments.Add(new PixelSegment
          {
            left = new Vector2d { x = x + i * segmentSizeX, y = y + j * segmentSizeY + segmentSizeY/2 },
            right = new Vector2d { x = x + i * segmentSizeX - segmentSizeX, y = y + j * segmentSizeY + segmentSizeY/2 },
            top = new Vector2d { x = x + i * segmentSizeX + segmentSizeX/2, y = y + j * segmentSizeY },
            bottom = new Vector2d { x = x + i * segmentSizeX + segmentSizeX / 2, y = y + j * segmentSizeY - segmentSizeY}
          });
        }
      }

      Vector3 outColor = Vector3.Zero;

      foreach(PixelSegment segment in segments)
      {
        float newX = (float)(segment.left.x - random.NextDouble() * segmentSizeX);
        float newY = (float)(segment.bottom.y + random.NextDouble() * segmentSizeY);

        int rayIndex = Scene.camera.CreateRay(newX, newY);
        Ray ray = Scene.camera.GetRay(rayIndex);
        outColor += RayTrace(ray, Scene.imageParameters.recursionDepth);
        Scene.camera.RemoveRay(rayIndex);
      }

      return outColor/(float)Math.Pow(segmentsInRow,2);
    }

    static void CreateHDRImage(FloatImage fi, ImageParameters imPar, Camera camera)
    {
      Random random = new Random();
      for (int i = 0; i < imPar.width; i++)
      {
        for (int j = 0; j < imPar.height; j++)
        {
          //camera.SetPositionX(i); camera.SetPositionY(j); // linear projection

          float normalizedX = (-2.0f * i) / imPar.width + 1f;  //normalize for x and y to go from -1 to +1
          float normalizedY = (-2.0f * j) / imPar.height + 1f;

          float[] color;

          if (imPar.aa)
          {
            Vector3 AAColor = AntiAlias(normalizedX, normalizedY, random);
            color = new float[3] { AAColor.X, AAColor.Y, AAColor.Z };
          }
          else
          {
            int rayIndex = camera.CreateRay(normalizedX, normalizedY);
            Ray ray = camera.GetRay(rayIndex);

            Vector3 colorVec = RayTrace(ray, imPar.recursionDepth);
            color = new float[3] { colorVec.X, colorVec.Y, colorVec.Z, };
            camera.RemoveRay(rayIndex);
          }

          fi.PutPixel(i, j, color);
        }
      }
    }

    static void Main(string[] args)
    {
      // Parameters.
      // TODO: parse command-line arguments and/or your config file.
      ImageParameters imPar = new ImageParameters { width = 600, height=450, recursionDepth=10, backgroundColor = Vector3.One };
      Scene.imageParameters = imPar;
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

      ConfigInputHandler.LoadConfig(configStream, out Scene.camera, Scene.solids, Scene.lights);

      FloatImage fi = new FloatImage(imPar.width, imPar.height, 3);

      CreateHDRImage(fi, imPar, Scene.camera);

      //fi.SaveHDR(fileName);   // Doesn't work well yet...
      fi.SavePFM(fileName);

      Console.WriteLine("HDR image is finished.");
    }
  }
}
