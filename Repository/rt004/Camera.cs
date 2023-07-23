#nullable enable

using System.Numerics;
using System.Text.Json.Serialization;

namespace rt004
{
  class Camera
  {
    const double aspectRatioDefault = 16d / 9d;
    const double fovDefault = 25 * Math.PI / 180;


    //Aspect ratio property with custom setter
    private double aspectRatio = aspectRatioDefault;
    public double AspectRatio
    {
      get { return aspectRatio; }
      set
      {
        aspectRatio = value;
        width = h * (float)aspectRatio;
      }
    }


    //FOV property with custom setter
    private double fov = fovDefault;
    public double Fov { get { return fov; }
      set
      {
        fov = value;
        h = (float)Math.Tan(fov);
        width = h * (float)aspectRatio;
      }
    }

    private float width;
    private float h;

    Position3D Right { get; set; } = new();
    public Position3D Position { get; set; } = new();     //Warning: Property name dependent on json config

    //View vector property witch custom setter
    Position3D viewVector = Vector3.UnitZ;
    public Position3D ViewVector { get { return viewVector; }     //Warning: Property name dependent on json config
      set 
      {
        viewVector = value;
        SetRightVec(); NormalizeAll();
      }
    }

    //Up vector property with custom setter
    Position3D upVector = Vector3.UnitY;
    public Position3D UpVector { get { return upVector; }     //Warning: Property name dependent on json config
      set 
      {
        upVector = value;
        SetRightVec(); NormalizeAll();
      } 
    }

    public Camera(Position3D position, Position3D viewVector, Position3D upVector, double aspectRatio = aspectRatioDefault, double visionAngle = fovDefault)
    {
      this.Position = position;
      this.ViewVector = viewVector;
      this.UpVector = upVector;
      this.Fov = visionAngle;
      this.AspectRatio = aspectRatio;
    }

    [JsonConstructor]
    public Camera(Position3D position, Position3D viewVector, Position3D upVector)
    {
      this.Position = position;
      this.ViewVector = viewVector;
      this.UpVector = upVector;
      this.AspectRatio = aspectRatioDefault;
      this.Fov = fovDefault;
    }

    void SetRightVec()
    {
      Right = Vector3.Cross(ViewVector, UpVector);
    }

    void NormalizeAll()
    {
      Right = Vector3.Normalize(Right);
      viewVector = Vector3.Normalize(ViewVector); //Has to set the field! - otherwise stack overflow
      upVector = Vector3.Normalize(UpVector);     //
    }

    List <Ray> rays = new();
    Dictionary <int, Ray> rayIndeces = new();
    int currentIndex = 0;
    public int CreateAndStoreRay(float x, float y)
    {
      Vector3 rayVec = ViewVector + x * width * Right + y * h * UpVector;
      rayVec = Vector3.Normalize(rayVec);
      Ray newRay = new Ray(Position, rayVec, currentIndex);
      rays.Add(newRay);
      rayIndeces.Add(currentIndex, newRay);
      currentIndex++;
      return currentIndex - 1;
    }

    public Ray CreateRay(float x, float y) //This doesn't store rays anywhere - paralell friendly
    {
      Vector3 rayVec = ViewVector + x * width * Right + y * h * UpVector;
      rayVec = Vector3.Normalize(rayVec);
      Ray newRay = new Ray(Position, rayVec, currentIndex);
      return newRay;
    }

    public Ray GetRay(int index)
    {
      return rayIndeces[index];
    }

    public void RemoveRay(int index)
    {
      rays.Remove(rayIndeces[index]);
      rayIndeces.Remove(index);
      return;
    }
    public void RemoveRay(Ray ray)
    {
      rays.Remove(ray);
      rayIndeces.Remove(ray.index);
      return;
    }
  }
}
