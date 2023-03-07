//using System.Numerics;
#nullable enable

using System;
using System.Dynamic;
using System.Numerics;
using System.Security.Cryptography;

namespace rt004
{
  abstract class Solid
  {
    public abstract void GetIntersection(Ray ray, out double? outT);
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
  static class NormalVectorCalculator
  {
    public static Vector3 GetNormal(Vector3 point1, Vector3 point2) 
    {
      Vector3 vector = point2 - point1;
      Vector3 unitY = Vector3.UnitY;
      Vector3 unitZ = Vector3.UnitZ;
      if (vector.X != 0 || vector.Z != 0)
        return Vector3.Cross(vector, unitY);
      else
        return Vector3.Cross(vector, unitZ);
    }
  }
  class Sphere:Solid
  {
    Vector3 position = new();
    double r;
    public Sphere(Vector3 position, double r)
    {
      this.position = position;
      this.r = r;
    }

    public override void GetIntersection(Ray ray/*, out Vector3? outIntersection, out Vector3? outNormalVector*/, out double? outT)   //uncomment for T/Intersection return instead intersection
    {
      const double rayLength = 1000;
      double x0 = ray.position.X;
      double y0 = ray.position.Y;
      double z0 = ray.position.Z;
      double x1 = ray.position.X+ray.vector.X*rayLength;
      double y1 = ray.position.Y+ray.vector.Y*rayLength;
      double z1 = ray.position.Z+ray.vector.Z*rayLength;
      double distX = x1 - x0;
      double distY = y1 - y0;
      double distZ = z1 - z0;
      double xc = position.X;
      double yc = position.Y;
      double zc = position.Z;
      double AFunction()
      {
        return distX * distX + distY * distY + distZ * distZ;
      }
      double BFunction()
      {
        return 2.0 * (x0 * distX + y0 * distY + z0 * distZ - distX * xc - distY * yc - distZ * zc);
      }
      double CFunction()
      {
        return x0 * x0 - 2 * x0 * xc + xc * xc + y0 * y0 - 2 * y0 * yc + yc * yc +
                   z0 * z0 - 2 * z0 * zc + zc * zc - r * r;
      }
      double A = AFunction();
      double B = BFunction();
      double C = CFunction();
      double? root1;
      double? root2;
      double? t = null;
      QuadraticEquationSolver.Solve(A, B, C, out root1, out root2);
      Vector3 intersection1 = new Vector3();
      Vector3 intersection2 = new Vector3();
      Vector3 intersection = new Vector3();
      if (root1 is not null)
      {
        intersection1 = new Vector3((float)(x0 * (1 - root1) + root1 * x1), (float)(y0 * (1 - root1) + root1 * y1), (float)(z0 * (1 - root1) + root1 * z1));
        intersection = intersection1;
        t = root1 * rayLength;
      }
      if (root2 is not null)
      {
        intersection2 = new Vector3((float)(x0 * (1 - root2) + root2 * x1), (float)(y0 * (1 - root2) + root2 * y1), (float)(z0 * (1 - root2) + root2 * z1));
        intersection = intersection2;
        t= root2 * rayLength;
      }
      if (root1 is not null && root2 is not null)
      {
        if (DistanceCalculator.GetDistance(ray.position, intersection1) <= DistanceCalculator.GetDistance(ray.position, intersection2)) { intersection = intersection1; t = root1 * rayLength; }
        else { intersection = intersection2; t = root2 * rayLength; }
      }
      if (root1 is not null || root2 is not null)
      {
        //outIntersection = intersection;
        //outNormalVector = NormalVectorCalculator.GetNormal(position, intersection);
        outT = t;      //uncomment for T return instead intersection
      }
      else
      {
        //outIntersection = null;   //uncomment for Intersection return instead of T
        //outNormalVector = null;
        outT = null;    //uncomment for T return instead intersection
      }

    }

  }

  class Cylinder : Solid
  {
    Vector3 topCenter;
    Vector3 bottomCenter;
    float r;
    public Cylinder(Vector3 topCenter ,Vector3 bottomCenter, float r)
    {
      this.bottomCenter = bottomCenter;
      this.r = r;
      this.topCenter = topCenter;
    }


    //Intersection function source: https://gist.github.com/Half/5fba69ba467891a3a1a3d754ea8732d3
    public override void GetIntersection(Ray ray, out double? outT) 
    {
      outT = null;

      // Calculate cylinder bounds for optimization
      float cxmin, cymin, czmin, cxmax, cymax, czmax;

      if (bottomCenter.Z < topCenter.Z)
      {
        czmin = bottomCenter.Z - r;
        czmax = topCenter.Z + r;
      }
      else
      {
        czmin = topCenter.Z - r;
        czmax = bottomCenter.Z + r;
      }

      if (bottomCenter.Y < topCenter.Y)
      {
        cymin = bottomCenter.Y - r;
        cymax = topCenter.Y + r;
      }
      else
      {
        cymin = topCenter.Y - r;
        cymax = bottomCenter.Y + r;
      }

      if (bottomCenter.X < topCenter.X)
      {
        cxmin = bottomCenter.X - r;
        cxmax = topCenter.X + r;
      }
      else
      {
        cxmin = topCenter.X - r;
        cxmax = bottomCenter.X + r;
      }

      // Line out of bounds?
      if ((ray.position.Z >= czmax && (ray.position.Z + ray.vector.Z) > czmax)
          || (ray.position.Z <= czmin && (ray.position.Z + ray.vector.Z) < czmin)
          || (ray.position.Y >= cymax && (ray.position.Y + ray.vector.Y) > cymax)
          || (ray.position.Y <= cymin && (ray.position.Y + ray.vector.Y) < cymin)
          || (ray.position.X >= cxmax && (ray.position.X + ray.vector.X) > cxmax)
          || (ray.position.X <= cxmin && (ray.position.X + ray.vector.X) < cxmin))
      {
        return;
      }

      Vector3 AB = topCenter - bottomCenter;
      Vector3 AO = ray.position - bottomCenter;
      Vector3 AOxAB = Vector3.Cross(AO, AB);
      Vector3 VxAB = Vector3.Cross(ray.vector, AB);
      float ab2 = Vector3.Dot(AB, AB);
      float a = Vector3.Dot(VxAB, VxAB);
      float b = 2 * Vector3.Dot(VxAB, AOxAB);
      float c = Vector3.Dot(AOxAB, AOxAB) - (r * r * ab2);
      float d = b * b - 4f * a * c;

      if (d < 0f)
      {
        return;
      }

      float time = (float)((-b - Math.Sqrt(d)) / (2f * a));

      if (time < 0f)
      {
        return;
      }
      outT = time;
    }   

  }

}


