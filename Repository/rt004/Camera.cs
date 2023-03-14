//using System.Numerics;
#nullable enable

using System.Numerics;
using System.Security.Cryptography;

namespace rt004
{
  struct Ray
  {
    public Vector3 position = new();
    public Vector3 vector = new();
    public int index;
    public Ray(Vector3 position, Vector3 vector, int index)
    {
      this.position = position;
      this.vector = vector;
      this.index = index;
    }

    public Vector3 PositionAfterT(float t)
    {
      return this.position + this.vector * t;
    }
  }
  class Camera
  {
    public double aspectRatio;
    public double fov;
    Vector3 right = new();
    Vector3 position = new();
    Vector3 forward = new();
    Vector3 up = Vector3.UnitY;
    float width;
    float h;
    public Camera(Vector3 position, Vector3 viewVector, Vector3 upVector = default, double aspectRatio = 16d/9d, double visionAngle = 25*Math.PI/180)
    {
      this.position = position;
      this.forward = viewVector;
      this.up = upVector;
      this.fov = visionAngle;
      this.aspectRatio = aspectRatio;

      forward = viewVector;
      forward = Vector3.Normalize(forward);
      up = upVector;
      up = Vector3.Normalize(up);
      right = Vector3.Cross(forward, up);
      right = Vector3.Normalize(right);

      h = (float)Math.Tan(fov);
      width = h * (float)aspectRatio;
    }
    public void SetPosition(float x, float y, float z) { position = new Vector3(x, y, z); }
    public void SetPosition(Vector3 vector) { position = vector; }

    public void SetPositionX(float x) { position.X = x; }

    public void SetPositionY(float y) { position.Y = y; }

    public void SetPositionZ(float z) { position.Z = z; }

    public Vector3 GetPosition() { return position; }

    public void SetViewVector(float x, float y, float z) { forward = new Vector3(x, y, z); }
    public void SetViewVector(Vector3 vector) { forward = vector; }
    public void SetViewVectorX(float x) { forward.X = x; }
    public void SetViewVectorY(float y) { forward.Y = y; }
    public void SetViewVectorZ(float z) { forward.Z = z; }
    public Vector3 GetViewVector() { return forward; }
    public void SetUpVector(float x, float y, float z) { up = new Vector3(x, y, z); }
    public void SetUpVector(Vector3 vector) { up = vector; }
    public void SetUpVectorX(float x) { up.X = x; }
    public void SetUpVectorY(float y) { up.Y = y; }
    public void SetUpVectorZ(float z) { up.Z = z; }
    public Vector3 GetUpVector() { return up; }

    List <Ray> rays = new();
    Dictionary <int, Ray> rayIndeces = new();
    int currentIndex = 0;
    public int CreateRay(float x, float y)
    {
      Vector3 rayVec = forward + x * width * right + y * h * up;
      rayVec = Vector3.Normalize(rayVec);
      Ray newRay = new Ray(position, rayVec, currentIndex);
      rays.Add(newRay);
      rayIndeces.Add(currentIndex, newRay);
      currentIndex++;
      return currentIndex - 1;
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
