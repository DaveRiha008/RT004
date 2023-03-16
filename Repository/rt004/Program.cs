using System.Numerics;
#nullable enable

using System.Security.Cryptography;

namespace rt004
{
  internal class Program
  {
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
    }

    static void LoadConfig(StreamReader configStream, Dictionary<string, string> configOptions)
    {
      using (configStream)
      {
        ConfigInputHandler config = new ConfigInputHandler(configStream);
        KeyValuePair<string, string>? pair = new KeyValuePair<string, string>();
        bool end = false;
        while (!end)
        {
          pair = config.GetPair(out end);
          if (pair == null) continue;
          configOptions.Add(pair.Value.Key, pair.Value.Value);
        }
      }
    }

    static void CreateHDRImage(FloatImage fi, ImageParameters imPar)
    {
      Sphere sphere1 = new Sphere(new Vector3(0.4f, 0.1f, 1.8f), 0.1f, new Vector3(0.5f, 0.5f, 0), new Material());

      Sphere sphere2 = new Sphere(new Vector3(0.4f, 0.1f, 1f), 0.1f, new Vector3(1f, 0f, 0), new Material());

      Plane plane1 = new Plane(new Vector3(-0.0f, 0.0f, 1f), Vector3.Normalize(new Vector3(0,1,-0.2f)), new Vector3(0, 0.3f, 1), new Material());

      Camera camera = new Camera(new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(0,1,0), aspectRatio:(imPar.width/imPar.height));

      double? t;
      int rayIndex;
      for (int i = 0; i < imPar.width; i++)
      {
        for (int j = 0; j < imPar.height; j++)
        {
          //camera.SetPositionX(i); camera.SetPositionY(j); // linear projection

          Lights lights = new Lights();
          lights.AddLight(new Vector3(-0.5f, 0.5f, 0.2f), Vector3.One, 2);
          lights.AddLight(new Vector3(-0.2f, 0.1f, 1.0f), Vector3.One, 1);
          lights.AddAmbientLight(0.5f);


          float normalizedX = (-2.0f * i) / imPar.width + 1f;  //normalize for x and y to go from -1 to +1
          float normalizedY = (-2.0f * j) / imPar.height + 1f;

          rayIndex = camera.CreateRay(normalizedX, normalizedY); 
          Ray ray = camera.GetRay(rayIndex);

          float[] color = new float[3] { 1, 1, 1 };

          plane1.GetIntersection(ray, out t);
          if (t is not null && t > 0)
          {
            Vector3 colorVec = lights.GetColor(plane1, ray.PositionAfterT((float)t), camera.GetPosition());
            color = new float[3] { colorVec.X, colorVec.Y, colorVec.Z, };
          }

          sphere1.GetIntersection(ray, out t);
          if (t is not null && t > 0)
          {
            Vector3 colorVec = lights.GetColor(sphere1, ray.PositionAfterT((float)t), camera.GetPosition());
            color = new float[3] { colorVec.X, colorVec.Y, colorVec.Z, };
          }

          sphere2.GetIntersection(ray, out t);
          if (t is not null && t > 0)
          {
            Vector3 colorVec = lights.GetColor(sphere2, ray.PositionAfterT((float)t), camera.GetPosition());
            color = new float[3] { colorVec.X, colorVec.Y, colorVec.Z, };
          }

          fi.PutPixel(i, j, color);
          camera.RemoveRay(rayIndex);
        }
      }
    }
    struct ImageParameters
    {
      public int width;
      public int height;
      public int ratio;
      public ImageParameters(int width, int height, int ratio)
      {
        this.width = width;
        this.height = height;
        this.ratio = ratio;
      }
    }
    static void Main(string[] args)
    {
      // Parameters.
      // TODO: parse command-line arguments and/or your config file.
      ImageParameters imPar = new ImageParameters(600, 450, 2);
      string fileName = "demo.pfm";
      //Console.WriteLine("Input your config file name (path): ");
      //string? configFileName = Console.ReadLine();
      string? configFileName = "config.txt";
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
      Dictionary<string, string> configOptions = new();
      LoadConfig(configStream, configOptions);

      if (configOptions.TryGetValue("width", out _)) int.TryParse(configOptions["width"], out imPar.width);
      if (configOptions.TryGetValue("height", out _)) int.TryParse(configOptions["height"], out imPar.height);
      if (configOptions.TryGetValue("ratio", out _)) int.TryParse(configOptions["ratio"], out imPar.ratio);
      // HDR image.

      FloatImage fi = new FloatImage(imPar.width, imPar.height, 3);

      // TODO: put anything interesting into the image.
      // TODO: use fi.PutPixel() function, pixel should be a float[3] array [r, G, B]

      CreateHDRImage(fi, imPar);

      //fi.SaveHDR(fileName);   // Doesn't work well yet...
      fi.SavePFM(fileName);

      Console.WriteLine("HDR image is finished.");
    }
  }
}
