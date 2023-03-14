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

  class Plane : Solid
  {
    Vector3 position = new();
    Vector3 normal = new();
    public Plane(Vector3 position, Vector3 normal)
    {
      this.position = position;
      this.normal = normal;
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

      //calculate the distance between the linePoint and the line-plane intersection point
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
    double r;
    public Sphere(Vector3 position, double r)
    {
      this.position = position;
      this.r = r;
    }

    //source: https://gamedev.stackexchange.com/questions/96459/fast-ray-sphere-collision-code - DMGregory
    public override void GetIntersection(Ray ray, out double? outT)
    {
      outT = null;
      Vector3 s = ray.position;
      Vector3 c = position;
      Vector3 p = s - c;
      Vector3 d = ray.vector;

      double rSquared = r * r;
      float p_d = Vector3.Dot(p, d);

      // The sphere is behind or surrounding the start point.
      if (p_d > 0 || Vector3.Dot(p, p) < rSquared)
        return;

      // Flatten p into the plane passing through c perpendicular to the ray.
      // This gives the closest approach of the ray to the center.
      Vector3 a = p - p_d * d;

      float aSquared = Vector3.Dot(a, a);

      // Closest approach is outside the sphere.
      if (aSquared > rSquared)
        return;

      // Calculate distance from plane where ray enters/exits the sphere.    
      float h = (float)Math.Sqrt(rSquared - aSquared);
      outT = h;

      // Calculate intersection point relative to sphere center.
      //Vector3 i = a - h * d;

      //Vector3 intersection = c + i;
      //Vector3 normal = i / (float)r;
      // We've taken a shortcut here to avoid a second square root.
      // Note numerical errors can make the normal have length slightly different from 1.
      // If you need higher precision, you may need to perform a conventional normalization.
    }
  }

  class Cylinder : Solid
  {
    Vector3 topCenter;
    Vector3 bottomCenter;
    float r;
    public Cylinder(Vector3 bottomCenter ,Vector3 topCenter, float r)
    {
      this.bottomCenter = bottomCenter;
      this.r = r;
      this.topCenter = topCenter;
    }


    //Cylinder intersection function source: https://gist.github.com/Half/5fba69ba467891a3a1a3d754ea8732d3
    public override void GetIntersection(Ray ray, out double? outT) 
    {
      outT = null;
      const int rayLength = 1000;
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

      Vector3 rayDirection = ray.vector*rayLength;

      // Line out of bounds?
      if ((ray.position.Z >= czmax && (ray.position.Z + rayDirection.Z) > czmax)
          || (ray.position.Z <= czmin && (ray.position.Z + rayDirection.Z) < czmin)
          || (ray.position.Y >= cymax && (ray.position.Y + rayDirection.Y) > cymax)
          || (ray.position.Y <= cymin && (ray.position.Y + rayDirection.Y) < cymin)
          || (ray.position.X >= cxmax && (ray.position.X + rayDirection.X) > cxmax)
          || (ray.position.X <= cxmin && (ray.position.X + rayDirection.X) < cxmin))
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

      float time1 = (float)((-b + Math.Sqrt(d)) / (2f * a));
      float time2 = (float)((-b - Math.Sqrt(d)) / (2f * a));

      if (time1 < 0f && time2 < 0f)
      {
        return;
      }
      if (time1 >= 0f)
      {
        if (time1 <= time2) outT = time1;
        else if (time2 >= 0f) outT = time2;
        else outT = time1;
      }
      else if (time2 >= 0f)
        outT = time2;
      //if(time >= 0f)
      //{
      //  outT =time;
      //}
    }   

  }

}


