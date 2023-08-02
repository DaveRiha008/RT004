using System.Numerics;

namespace rt004
{
  class AntiAliaser
  {
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
    public static Vector3 AntiAlias(float x, float y, Random random, Scene scene)
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
        outColor += RayTracer.RayTrace(ray, scene.imageParameters.RtRecursionDepth, scene);
        //}));
      }

      //Task.WaitAll(tasks.ToArray());

      return outColor / (float)Math.Pow(pixelsInfo.segmentsInRow, 2);

    }
  }
}