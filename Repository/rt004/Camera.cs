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
  }
  class Camera
  {
    Vector3 position = new();
    Vector3 viewVector = new();
    public Camera(Vector3 position, Vector3 viewVector)
    {
      this.position = position;
      this.viewVector = viewVector;
    }
    public void SetPosition(float x, float y, float z) { position = new Vector3(x, y, z); }
    public void SetPosition(Vector3 vector) { position = vector; }

    public void SetPositionX(float x) { position.X = x; }

    public void SetPositionY(float y) { position.Y = y; }

    public void SetPositionZ(float z) { position.Z = z; }

    public Vector3 GetPosition() { return position; }

    public void SetViewVector(float x, float y, float z) { viewVector = new Vector3(x, y, z); }
    public void SetViewVector(Vector3 vector) { viewVector = vector; }
    public void SetViewVectorX(float x) { viewVector.X = x; }
    public void SetViewVectorY(float y) { viewVector.Y = y; }
    public void SetViewVectorZ(float z) { viewVector.Z = z; }
    public Vector3 GetViewVector() { return viewVector; }

    List <Ray> rays = new();
    List <int> rayIndeces = new();
    int currentIndex = 0;
    public int CreateRay(float x, float y, float z)
    {
      rays.Add(new Ray(position, new Vector3(x, y, z), currentIndex));
      rayIndeces.Add(currentIndex);
      currentIndex++;
      return currentIndex - 1;
    }
    public int CreateRay(Vector3 vector)
    {
      rays.Add(new Ray(position, vector, currentIndex));
      rayIndeces.Add(currentIndex);
      currentIndex++;
      return currentIndex - 1;
    }

    public Ray GetRay(int index)
    {
      return rays[rayIndeces.IndexOf(index)];
    }

    public void RemoveRay(int index)
    {
      rays.RemoveAt(rayIndeces.IndexOf(index));
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
