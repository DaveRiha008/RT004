using System.Numerics;

namespace rt004
{
  class ImageCreator
  {
    /// <summary>
    /// Creates an image of given scene and saves it in given pfm file
    /// </summary>
    /// <param name="scene">Scene to be drawn in image</param>
    public static void CreateAndSaveHDRImage(Scene scene, string outputFileName)
    {
      //Create image, where colored pixels will be saved
      FloatImage fi = new FloatImage(scene.imageParameters.Width, scene.imageParameters.Height, 3);


      //Draw scene
      CreateHDRImage(fi, scene);

      //Save imageto file

      //fi.SaveHDR(fileName);   // Doesn't work well yet...
      fi.SavePFM(outputFileName);

      Console.WriteLine("HDR image is finished.");
    }

    /// <summary>
    /// Takes each pixel and traces a ray through it - result color is assigned to the pixel
    /// </summary>
    /// <param name="fi">Empty image to be overdrawn</param>
    /// <param name="scene">Scene to be drawn in image</param>
    public static void CreateHDRImage(FloatImage fi, Scene scene)
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
            Vector3 AAColor = AntiAliaser.AntiAlias(normalizedX, normalizedY, random, scene);
            color = new float[3] { AAColor.X, AAColor.Y, AAColor.Z };
          }
          else
          {
            Ray ray = camera.CreateRay(normalizedX, normalizedY);

            Vector3 colorVec = RayTracer.RayTrace(ray, imPar.RtRecursionDepth, scene);
            color = new float[3] { colorVec.X, colorVec.Y, colorVec.Z, };
          }

          fi.PutPixel(i, j, color);
        });
      });
    }
  }
}