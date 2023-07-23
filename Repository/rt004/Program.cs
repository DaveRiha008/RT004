using System.Numerics;
#nullable enable

using System.Diagnostics;
using System.Threading.Tasks;

namespace rt004
{

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
    static Vector3 RayTrace(Ray ray0, int currentRecursion, Scene scene)
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

      if (currentRecursion > 0)
      {
        //reflection
        //var reflTask = Task.Run(() =>
        //{
          Vector3 reflectVec = Vector3.Reflect(currentRay.vector, currentSolid.GetNormal(currentIntersection, currentRay.position));

          Ray newReflectedRay = new Ray(currentIntersection, reflectVec);

          Vector3 rayTracedColor = RayTrace(newReflectedRay, currentRecursion - 1, scene);

          outColor += (float)Math.Pow(currentSolid.Material.reflection, scene.imageParameters.RtRecursionDepth - currentRecursion + 1) * rayTracedColor; //+= doesn't expose shared memory
        //});

        //recursionTasks[0] = reflTask;

        //refraction


        //var refrTask = Task.Run(() =>
        //{
          Vector3 furtherIntersection = currentRay.PositionAfterT((float)(t + Constants.epsilon));

          Solid? originSolid = scene.solids.GetSolidOfAPoint(currentRay.position);
          double originRefrIndex = 1;
          if (originSolid is not null)
            originRefrIndex = originSolid.Material.refractionIndex;
          Solid? outsideSolid = scene.solids.GetSolidOfAPoint(furtherIntersection);
          double outsideRefrIndex = 1;
          if (outsideSolid is not null) { outsideRefrIndex = outsideSolid.Material.refractionIndex; }

          Vector3? refractVec = VectorCalculator.GetRefractedVector(originRefrIndex, outsideRefrIndex, currentSolid.GetNormal(currentIntersection, currentRay.position), currentRay.vector);
          if (refractVec is not null && currentSolid.Material.transparency > 0)
          {

            Ray newRefractedRay = new Ray(furtherIntersection, (Vector3)refractVec);

            Vector3 refractedColor = RayTrace(newRefractedRay, currentRecursion - 1, scene);

            outColor += (float)Math.Pow(currentSolid.Material.transparency, scene.imageParameters.RtRecursionDepth - currentRecursion + 1) * refractedColor; //+= doesn't expose shared memory

          }
        //});

        //recursionTasks[1] = refrTask;
        //Task.WaitAll(recursionTasks);
      }
      return outColor;
    }



    static Vector3 AntiAliasAsync(float x, float y, Random random, Scene scene)
    {
      int spp = scene.imageParameters.Spp;
      int segmentsInRow = (int)Math.Sqrt(spp);
      double pixelSizeX = 2d / (double)scene.imageParameters.Width;
      double pixelSizeY = 2d / (double)scene.imageParameters.Height;
      double segmentSizeX = pixelSizeX / segmentsInRow;
      double segmentSizeY = pixelSizeY / segmentsInRow;

      List<PixelSegment> segments = new List<PixelSegment>();
      for (double i = (double)-segmentsInRow / 2; i < (double)segmentsInRow / 2; i++)
      {
        for (double j = (double)-segmentsInRow / 2; j < (double)segmentsInRow / 2; j++)
        {
          segments.Add(new PixelSegment
          {
            left = new Vector2d { x = x + i * segmentSizeX, y = y + j * segmentSizeY + segmentSizeY / 2 },
            right = new Vector2d { x = x + i * segmentSizeX - segmentSizeX, y = y + j * segmentSizeY + segmentSizeY / 2 },
            top = new Vector2d { x = x + i * segmentSizeX + segmentSizeX / 2, y = y + j * segmentSizeY },
            bottom = new Vector2d { x = x + i * segmentSizeX + segmentSizeX / 2, y = y + j * segmentSizeY - segmentSizeY }
          });
        }
      }

      Vector3 outColor = Vector3.Zero;

      //Parallel version about 4x slower than sequential - too much pressure on ThreadPool? Or wrong parallelism implementation?

      //List<Task> tasks = new List<Task>();

      foreach (PixelSegment segment in segments)
      {
        //tasks.Add(Task.Run(() =>
        //{
          float newX = (float)(segment.left.x - random.NextDouble() * segmentSizeX);
          float newY = (float)(segment.bottom.y + random.NextDouble() * segmentSizeY);

          Ray ray = scene.camera.CreateRay(newX, newY);
          outColor += RayTrace(ray, scene.imageParameters.RtRecursionDepth, scene);
        //}));
      }

      //Task.WaitAll(tasks.ToArray());

      return outColor / (float)Math.Pow(segmentsInRow, 2);

    }

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
            Vector3 AAColor = AntiAliasAsync(normalizedX, normalizedY, random, scene);
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

      // Parameters.
      Scene scene = new()
      {
        imageParameters = new ImageParameters { Width = 600, Height = 450, RtRecursionDepth = 10, BackgroundColor = Vector3.One }
      };



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

      FloatImage fi = new FloatImage(scene.imageParameters.Width, scene.imageParameters.Height, 3);


      CreateHDRImage(fi, scene);


      //fi.SaveHDR(fileName);   // Doesn't work well yet...
      fi.SavePFM(Constants.outFileName);

      Console.WriteLine("HDR image is finished.");

      stopwatch.Stop();
      string elapsedTime = stopwatch.Elapsed.ToString();
      Console.WriteLine("Whole program time elapsed: " + elapsedTime);
    }
  }
}
