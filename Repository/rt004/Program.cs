using System.Numerics;
#nullable enable

using System.Diagnostics;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace rt004
{
  /// <summary>
  /// Contains all information about scene - base for drawing
  /// </summary>
   class Scene
  {
    public AllSolids solids = new AllSolids();

    public Camera camera = new Camera(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);

    public AllLights lights = new AllLights();

    public ImageParameters imageParameters = new ImageParameters();

    public Scene() { }
    public Scene(AllSolids solids, Camera camera, AllLights lights, ImageParameters imageParameters)
    {
      this.solids = solids;
      this.camera = camera;
      this.lights = lights;
      this.imageParameters = imageParameters;
    }
  }
  internal class Program
  {
    /// <summary>
    /// Traces given ray through scene and computes its final color
    /// </summary>
    /// <remarks>Thread safe</remarks>

    /// <param name="ray0">Ray to be traced further</param>
    /// <param name="remainingRecursions">How many times should ray reflect and refract recursively</param>
    /// <param name="scene">Information about the whole scene world</param>
    /// <returns>Color which the given ray should display in its starting point</returns>
    static Color RayTrace(Ray ray0, int remainingRecursions, Scene scene)
    {
      Ray currentRay = ray0;
      double? t;
      Solid? currentSolid;
      scene.solids.GetClosestIntersection(currentRay, out t, out currentSolid);
      if (t is null || t <= 0 || currentSolid is null) return scene.imageParameters.BackgroundColor;
      Vector3 currentIntersection = currentRay.PositionAfterT((float)t);

      Vector3 outColor = scene.lights.GetColor(currentSolid, currentIntersection, currentRay, scene.solids);


      //Parallel version is about 6x slower then sequential - too much pressure in ThreadPool? Wrong parallelism implementation?
      //Task[] recursionTasks = new Task[2];

      if (remainingRecursions > 0)
      {
        //reflection
        //var reflTask = Task.Run(() =>
        //{
          Vector3 reflectVec = Vector3.Reflect(currentRay.vector, currentSolid.GetNormal(currentIntersection, currentRay.position));

          Ray newReflectedRay = new Ray(currentIntersection, reflectVec);

          Vector3 rayTracedColor = RayTrace(newReflectedRay, remainingRecursions - 1, scene);

          outColor += (float)Math.Pow(currentSolid.Material.Reflection, scene.imageParameters.RtRecursionDepth - remainingRecursions + 1) * rayTracedColor; //+= doesn't expose shared memory
        //});

        //recursionTasks[0] = reflTask;



        //refraction


        //var refrTask = Task.Run(() =>
        //{
          Vector3 furtherIntersection = currentRay.PositionAfterT((float)(t + Constants.epsilon));

          Solid? originSolid = scene.solids.GetSolidOfAPoint(currentRay.position);
          double originRefrIndex = 1;
          if (originSolid is not null)
            originRefrIndex = originSolid.Material.RefractionIndex;
          Solid? outsideSolid = scene.solids.GetSolidOfAPoint(furtherIntersection);
          double outsideRefrIndex = 1;
          if (outsideSolid is not null) { outsideRefrIndex = outsideSolid.Material.RefractionIndex; }

          Vector3? refractVec = VectorCalculator.GetRefractedVector(originRefrIndex, outsideRefrIndex, currentSolid.GetNormal(currentIntersection, currentRay.position), currentRay.vector);
          if (refractVec is not null && currentSolid.Material.Transparency > 0)
          {

            Ray newRefractedRay = new Ray(furtherIntersection, (Vector3)refractVec);

            Vector3 refractedColor = RayTrace(newRefractedRay, remainingRecursions - 1, scene);

            outColor += (float)Math.Pow(currentSolid.Material.Transparency, scene.imageParameters.RtRecursionDepth - remainingRecursions + 1) * refractedColor; //+= doesn't expose shared memory

          }
        //});

        //recursionTasks[1] = refrTask;
        //Task.WaitAll(recursionTasks);
      }
      return outColor;
    }


    /// <returns>List of created segments</returns>
    static List<PixelSegment> SplitPixelIntoSegments(float x, float y, ScenePixelsAndSegmentsInfo pixelsInfo)
    { 
      //Split the pixel into segments
      List<PixelSegment> segments = new List<PixelSegment>();
      for (double i = (double)-pixelsInfo.segmentsInRow / 2; i < (double)pixelsInfo.segmentsInRow / 2; i++)
      {
        for (double j = (double)-pixelsInfo.segmentsInRow / 2; j < (double)pixelsInfo.segmentsInRow / 2; j++)
        {
          segments.Add(new PixelSegment
          {
            left = new Vector2d { x = x + i * pixelsInfo.segmentSizeX, y = y + j * pixelsInfo.segmentSizeY + pixelsInfo.segmentSizeY / 2 },
            right = new Vector2d { x = x + i * pixelsInfo.segmentSizeX - pixelsInfo.segmentSizeX, y = y + j * pixelsInfo.segmentSizeY + pixelsInfo.segmentSizeY / 2 },
            top = new Vector2d { x = x + i * pixelsInfo.segmentSizeX + pixelsInfo.segmentSizeX / 2, y = y + j * pixelsInfo.segmentSizeY },
            bottom = new Vector2d { x = x + i * pixelsInfo.segmentSizeX + pixelsInfo.segmentSizeX / 2, y = y + j * pixelsInfo.segmentSizeY - pixelsInfo.segmentSizeY }
          });
        }
      }
      return segments;
    }

    /// <summary>
    /// Creates multiple rays around given pixel and raytraces them.
    /// </summary>
    /// <param name="x">Given pixel X position</param>
    /// <param name="y">Given pixel X position</param>
    /// <param name="random">Random generator</param>
    /// <param name="scene">Scene representing the whole world</param>
    /// <returns>A mean of all colors calculated around the pixel</returns>
    static Vector3 AntiAlias(float x, float y, Random random, Scene scene)
    {
      ScenePixelsAndSegmentsInfo pixelsInfo = new ScenePixelsAndSegmentsInfo(scene.imageParameters.Spp, scene.imageParameters.Width, scene.imageParameters.Height);
      List<PixelSegment> segments = SplitPixelIntoSegments(x, y, pixelsInfo);

      Color outColor = Vector3.Zero;

      //Take random point in each segment and raytrace it.

      //Parallel version about 4x slower than sequential - too much pressure on ThreadPool? Or wrong parallelism implementation?

      //List<Task> tasks = new List<Task>();

      foreach (PixelSegment segment in segments)
      {
        //tasks.Add(Task.Run(() =>
        //{
          float newX = (float)(segment.left.x - random.NextDouble() * pixelsInfo.segmentSizeX);
          float newY = (float)(segment.bottom.y + random.NextDouble() * pixelsInfo.segmentSizeY);

          Ray ray = scene.camera.CreateRay(newX, newY);
          outColor += RayTrace(ray, scene.imageParameters.RtRecursionDepth, scene);
        //}));
      }

      //Task.WaitAll(tasks.ToArray());

      return outColor / (float)Math.Pow(pixelsInfo.segmentsInRow, 2);

    }



    /// <summary>
    /// Takes each pixel and traces a ray through it - result color is assigned to the pixel
    /// </summary>
    /// <param name="fi">Empty image to be overdrawn</param>
    /// <param name="scene">Scene to be drawn in image</param>
    static void CreateHDRImage(FloatImage fi, Scene scene)
    {
      ImageParameters imPar = scene.imageParameters;
      Camera camera = scene.camera;

      Random random = new Random();

      Parallel.For(0, imPar.Width, i =>
      {
        Parallel.For(0, imPar.Height, j =>
        {
          //camera.SetPositionX(i); camera.SetPositionY(j); // linear projection

          float normalizedX = (-2.0f * i) / imPar.Width + 1f;  //normalize for x and y to go from -1 to +1
          float normalizedY = (-2.0f * j) / imPar.Height + 1f;

          float[] color;

          if (imPar.Aa)
          {
            Vector3 AAColor = AntiAlias(normalizedX, normalizedY, random, scene);
            color = new float[3] { AAColor.X, AAColor.Y, AAColor.Z };
          }
          else
          {
            Ray ray = camera.CreateRay(normalizedX, normalizedY);

            Vector3 colorVec = RayTrace(ray, imPar.RtRecursionDepth, scene);
            color = new float[3] { colorVec.X, colorVec.Y, colorVec.Z, };
          }

          fi.PutPixel(i, j, color);
        });
      });
    }


    static void Main(string[] args)
    {

      // Stopwatch - parallel testing
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();

      // Init parameters
      Scene scene = new()
      {
        imageParameters = new ImageParameters { Width = 600, Height = 450, RtRecursionDepth = 10, BackgroundColor = Vector3.One }
      };


      //Get config input and load
      StreamReader configStream;
      try
      {
        configStream = new StreamReader(Constants.configFileName);
      }
      catch(IOException)
      {
        Console.WriteLine("Invalid config file name - ending program");
        return;
      }


      ConfigInputHandler.LoadConfig(configStream, scene);


      //Create image, where colored pixels will be saved
      FloatImage fi = new FloatImage(scene.imageParameters.Width, scene.imageParameters.Height, 3);


      //Draw scene
      CreateHDRImage(fi, scene);

      //Save imageto file

      //fi.SaveHDR(fileName);   // Doesn't work well yet...
      fi.SavePFM(Constants.outFileName);

      Console.WriteLine("HDR image is finished.");

      stopwatch.Stop();
      string elapsedTime = stopwatch.Elapsed.ToString();
      Console.WriteLine("Whole program time elapsed: " + elapsedTime);
    }
  }
}
