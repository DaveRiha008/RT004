//using System.Numerics;
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

    static void CreateHDRImage(FloatImage fi, int wid, int hei, int ratio)
    {
      for (int i = 0; i < wid; i++)
      {
        for (int j = 0; j < hei; j++)
        {
          float red = i / (float)wid;
          float green = j / (float)hei;
          float blue = (red + green) / ratio;
          float[] color = new float[3] { red, green, blue };
          fi.PutPixel(i, j, color);
        }
      }
    }
    static void Main(string[] args)
    {
      // Parameters.
      // TODO: parse command-line arguments and/or your config file.
      int wid = 600;
      int hei = 450;
      int ratio = 2;
      string fileName = "demo.pfm";
      Console.WriteLine("Input your config file name (path): ");
      string? configFileName = Console.ReadLine();
      StreamReader configStream = new StreamReader(Console.OpenStandardInput());
      try
      {
        configStream = new StreamReader(configFileName);
      }
      catch
      {
        Console.WriteLine("Invalid config file name, reading from console - type end to finalize picture");
      }
      Dictionary<string, string> configOptions = new();
      LoadConfig(configStream, configOptions);

      if (configOptions.TryGetValue("width", out _)) int.TryParse(configOptions["width"], out wid);
      if (configOptions.TryGetValue("height", out _)) int.TryParse(configOptions["height"], out hei);
      if (configOptions.TryGetValue("ratio", out _)) int.TryParse(configOptions["ratio"], out ratio);


      // HDR image.

      FloatImage fi = new FloatImage(wid, hei, 3);

      // TODO: put anything interesting into the image.
      // TODO: use fi.PutPixel() function, pixel should be a float[3] array [R, G, B]

      CreateHDRImage(fi, wid, hei, ratio);

      //fi.SaveHDR(fileName);   // Doesn't work well yet...
      fi.SavePFM(fileName);

      Console.WriteLine("HDR image is finished.");
    }
  }
}
