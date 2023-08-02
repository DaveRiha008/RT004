using System.Numerics;
#nullable enable

using System.Diagnostics;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;
using System.Text.Json;

namespace rt004
{
  
  internal class Program
  {

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
        Console.WriteLine("Didn't find config file - ending program");
        return;
      }

      try
      {
        ConfigInputHandler.LoadConfig(configStream, scene);
      }
      catch(JsonException ex){
        Console.WriteLine(ex.Message + " - ending program"); return;
      }
      catch (PropertyNotDescribedException ex) {
        Console.WriteLine(ex.Message + " - ending program"); return;
      }


      //Create image, where colored pixels will be saved
      FloatImage fi = new FloatImage(scene.imageParameters.Width, scene.imageParameters.Height, 3);


      //Draw scene
      ImageCreator.CreateHDRImage(fi, scene);

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
