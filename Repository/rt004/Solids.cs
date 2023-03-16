//using System.Numerics;
#nullable enable

using Microsoft.VisualBasic;
using System;
using System.Drawing;
using System.Dynamic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace rt004
{
  class AllSolids
  {
    Dictionary<int, Solid> solids = new();
    int currentIndex = 0;
    public int AddSolid(Solid solid)
    {
      solids.Add(currentIndex, solid);
      currentIndex++;
      return currentIndex-1;
    }

    public void RemoveSolid(int index)
    {
      solids.Remove(index);
    }

    public void GetClosestIntersection(Ray ray, out double? outT, out Solid? outSolid)
    {
      outT = null;
      outSolid = null;

      foreach(Solid solid in solids.Values)
      {
        double? t;
        solid.GetIntersection(ray, out t);
        if (t is not null && t > 0)
        {
          if (outT is null)
          {
            outT = t;
            outSolid = solid;
          }
          else if (t < outT)
          {
            outT = t;
            outSolid = solid;
          }
        }
      }


    }
  }
  abstract class Solid
  {
    public Vector3 color;
    public Material material;
    public abstract void GetIntersection(Ray ray, out double? outT);
    public abstract Vector3 GetNormal(Vector3 intersectionPoint);
  }
  static class DistanceCalculator
  {
    static public double GetDistance(Vector3 v1, Vector3 v2)
    {
      double xDistance = Math.Abs(v1.X - v2.X);
      double yDistance = Math.Abs(v1.Y - v2.Y);
      double zDistance = Math.Abs(v1.Z - v2.Z);
      double distance = Math.Sqrt(xDistance*xDistance+yDistance*yDistance); 
      distance = Math.Sqrt(distance*distance+zDistance*zDistance);
      return distance;
    }
  }
  static class QuadraticEquationSolver
  {
    static public void Solve(double a, double b, double c, out double? x1, out double? x2)
    {
      if (a == 0)
      {
        x1 = (-c) / b;
        x2 = null;
        return;
      }
      double D = b * b - 4 * a * c;
      if(D == 0)
      {
        x1 = (-b) / (2*a);
        x2 = null; return;
      }
      if(D < 0) { x1 = null; x2 = null; return; }
      else
      {
        x1 = ((-b) + Math.Sqrt(D)) / (2 * a);
        x2 = ((-b) - Math.Sqrt(D)) / (2 * a);
        return;
      }
    }
  }
  static class VectorCalculator
  {
    public static Vector3 GetVectorBetween2Points(Vector3 point1, Vector3 point2)
    {
      Vector3 outVec = new Vector3(point2.X - point1.X, point2.Y - point1.Y, point2.Z - point1.Z);
      outVec = Vector3.Normalize(outVec);
      return outVec;
    }

    public static Vector3 GetReflectVector(Vector3 lightVector, Vector3 normal)
    {
      Vector3 reflectVec = 2 * (Vector3.Dot(lightVector, normal)) * normal - lightVector;
      reflectVec = Vector3.Normalize(reflectVec);
      return reflectVec;
    }
  }

  class Plane : Solid
  {
    Vector3 position = new();
    Vector3 normal = new();
    public Plane(Vector3 position, Vector3 normal, Vector3 color, Material material)
    {
      this.position = position;
      this.normal = normal;
      this.color = color;
      this.material = material;
    }

    public override Vector3 GetNormal(Vector3 intersectionPoint)
    {
      return normal;
    }

    public override void GetIntersection(Ray ray, out double? outT)
    {
      float a;
      float b;
      Vector3 planePoint = position;
      Vector3 planeNormal = normal;
      Vector3 linePoint = ray.position;
      Vector3 lineVec = ray.vector;
      outT = null;

      //calculate the distance between the linePoint and the line-plane point1 point
      a = Vector3.Dot(planePoint - linePoint, planeNormal);
      b = Vector3.Dot(lineVec, planeNormal);
      if (b == 0 && a == 0) outT = 1; //ray is going alongside plane - any T is true
      else if (b != 0) outT = a / b;
      return;
    }
  }

  class Sphere:Solid
  {
    Vector3 position = new();
    float r;
    public Sphere(Vector3 position, float r, Vector3 color, Material material)
    {
      this.position = position;
      this.r = r;
      this.color = color;
      this.material = material;
    }

    public override void GetIntersection(Ray ray, out double? outT)
    {
      outT = null;
      float cx = position.X;
      float cy = position.Y;
      float cz = position.Z;
      float dx = ray.vector.X;
      float dy = ray.vector.Y;
      float dz = ray.vector.Z;
      float x0 = ray.position.X;
      float y0 = ray.position.Y;
      float z0 = ray.position.Z;

      float a = dx * dx + dy * dy + dz * dz;
      float b = 2 * dx * (x0 - cx) + 2 * dy * (y0 - cy) + 2 * dz * (z0 - cz);
      float c = cx * cx + cy * cy + cz * cz + x0 * x0 + y0 * y0 + z0 * z0 +
                  -2 * (cx * x0 + cy * y0 + cz * z0) - r*r;
      
      float d = b * b - 4 * a * c;

      if (d < 0) return;
      if (d == 0)
      {
        outT = (-b) / (2 * a);
        return;
      }

      outT = (-b - Math.Sqrt(d)) / (2 * a);
      return;

    }

    public override Vector3 GetNormal(Vector3 intersectionPoint)
    {
      Vector3 normal = new Vector3((intersectionPoint.X-position.X)/r, (intersectionPoint.Y-position.Y)/r, (intersectionPoint.Z-position.Z)/r);
      normal = Vector3.Normalize(normal);
      return normal;
    }
  }
}


