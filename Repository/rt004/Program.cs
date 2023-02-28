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
      for (int i = 0; i < imPar.width; i++)
      {
        for (int j = 0; j < imPar.height; j++)
        {
          float red = i / (float)imPar.width;
          float green = j / (float)imPar.height;
          float blue = (red + green) / imPar.ratio;
          float[] color = new float[3] { red, green, blue };
          fi.PutPixel(i, j, color);
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
      Console.WriteLine("Input your config file name (path): ");
      string? configFileName = Console.ReadLine();
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

      Sphere sphere1 = new Sphere(new Vector3(4,0,0), 2);
      Sphere sphere2 = new Sphere(new Vector3(6,0,0), 2);
      Camera camera = new Camera(new Vector3(0, 0, 0), new Vector3(1, 0, 0));
      int rayIndex = camera.CreateRay(camera.GetViewVector());
      Vector3? intersection;
      sphere1.GetIntersection(camera.GetRay(rayIndex), out intersection, out _);
      sphere2.GetIntersection(camera.GetRay(rayIndex), out intersection, out _);
      // HDR image.

      FloatImage fi = new FloatImage(imPar.width, imPar.height, 3);

      // TODO: put anything interesting into the image.
      // TODO: use fi.PutPixel() function, pixel should be a float[3] array [R, G, B]

      CreateHDRImage(fi, imPar);

      //fi.SaveHDR(fileName);   // Doesn't work well yet...
      fi.SavePFM(fileName);

      Console.WriteLine("HDR image is finished.");
    }
  }
}
