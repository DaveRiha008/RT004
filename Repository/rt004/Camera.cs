﻿//using System.Numerics;
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

    public Vector3 PositionAfterT(double t)
    {
      return this.position + this.vector * (float)t;
    }
  }
  class Camera
  {
    public double visionAngle;
    Vector3 position = new();
    Vector3 viewVector = new();
    Vector3 upVector = Vector3.UnitY;
    public Camera(Vector3 position, Vector3 viewVector, Vector3 upVector = default, double visionAngle = 120)
    {
      this.position = position;
      this.viewVector = viewVector;
      this.upVector = upVector;
      this.visionAngle = visionAngle;
      this.upVector = upVector;
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
    public void SetUpVector(float x, float y, float z) { upVector = new Vector3(x, y, z); }
    public void SetUpVector(Vector3 vector) { upVector = vector; }
    public void SetUpVectorX(float x) { upVector.X = x; }
    public void SetUpVectorY(float y) { upVector.Y = y; }
    public void SetUpVectorZ(float z) { upVector.Z = z; }
    public Vector3 GetUpVector() { return upVector; }

    List <Ray> rays = new();
    Dictionary <int, Ray> rayIndeces = new();
    int currentIndex = 0;
    public int CreateRay(float x, float y, float z)
    {
      Vector3 rayVec = new Vector3(x, y, z) - position;
      rayVec = Vector3.Normalize(rayVec);
      Ray newRay = new Ray(position, rayVec, currentIndex);
      rays.Add(newRay);
      rayIndeces.Add(currentIndex, newRay);
      currentIndex++;
      return currentIndex - 1;
    }
    public int CreateRay(Vector3 vector)
    {
      Vector3 rayVec = vector - position;
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
