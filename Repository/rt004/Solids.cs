//using System.Numerics;
#nullable enable

using Microsoft.VisualBasic;
using System;
using System.Drawing;
using System.Dynamic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Distributions;

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
        if (t is not null && t > Constants.epsilon)
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

    public Solid? GetSolidOfAPoint(Vector3 point)
    {
      Solid? returnSolid = null;
      foreach(Solid solid in solids.Values)
      {
        if (solid.IsPointInside(point))
        {
          returnSolid = solid;
        }
      }
      return returnSolid;
    }
  }
  abstract class Solid
  {
    public Vector3 color;
    public Material material;
    public abstract void GetIntersection(Ray ray, out double? outT);
    public abstract bool IsPointInside(Vector3 point);
    public abstract Vector3 GetNormal(Vector3 intersectionPoint, Vector3 origin);
    public abstract void Transform(Matrix<float> tranformMatrix);
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

    public static Vector3? GetRefractedVector(double refractionIndex1, double refractionIndex2, Vector3 normal, Vector3 vector)
    {
      if(refractionIndex1 > refractionIndex2)       //Check total internal reflection
      {
        double critAngle = Math.Asin(refractionIndex2 / refractionIndex1);
        double incidentAngle = Math.Acos(Vector3.Dot(vector, normal) / (GetMagnitude(vector) * GetMagnitude(normal)));      
  
        if(incidentAngle >= critAngle ) { return null; }
      }

      double n12 = refractionIndex1 / refractionIndex2;
      Vector3 n = normal;
      Vector3 l = vector;
      Vector3 returnVec = (float)(n12 * Vector3.Dot(n, l) - Math.Sqrt(1 - (Math.Pow(n12, 2)) * (1 - Math.Pow(Vector3.Dot(n, l), 2)))) * n - (float)n12 * l;
      return returnVec;
    }

    public static double GetMagnitude(Vector3 vector)
    {
      return Math.Sqrt(Math.Pow(vector.X, 2) + Math.Pow(vector.Y, 2) + Math.Pow(vector.Z, 2));
    }
  }

  class Plane : Solid
  {
    Vector3 position = new();
    Vector3 normal = new();
    public Plane(Vector3 position, Vector3 normal, Vector3 color, Material material)
    {
      this.position = position;
      this.normal = Vector3.Normalize(normal);
      this.color = color;
      this.material = material;
    }



    //TODO propper equation function (for total internal reflection suffices false)
    public override bool IsPointInside(Vector3 point)
    {
      return false;
    }

    bool ShouldReverseNormal(Vector3 point)
    {
      Ray ray = new Ray(point, VectorCalculator.GetVectorBetween2Points((position + normal), point));
      double? T;
      GetIntersection(ray, out T);
      if (T is null) return false;
      else return true;
    }

    public Vector3 GetNormal(Vector3 intersectionPoint)
    {
      return normal;
    }
    public override Vector3 GetNormal(Vector3 intersectionPoint, Vector3 origin)
    {
      if (ShouldReverseNormal(origin)) return normal;
      return -normal;
    }

    public override void GetIntersection(Ray ray, out double? outT)
    {
      outT = null;
      Vector3 p_0 = position;
      Vector3 n = -normal;
      Vector3 l_0 = ray.position;
      Vector3 l = ray.vector;
      float denominator = Vector3.Dot(l, n);


      if (denominator > 0.00001f)
      {
        //The distance to the plane
        outT = Vector3.Dot(p_0 - l_0, n) / denominator;
      }
    }

    public override void Transform(Matrix<float> tranformMatrix)
    {
      float[] arrayPos = new float[4] { position.X, position.Y, position.Z, 1 };
      var position4d = MathNet.Numerics.LinearAlgebra.Vector<float>.Build.DenseOfArray(arrayPos);
      var newPos4d = position4d * tranformMatrix;
      position.X = newPos4d[0] / newPos4d[3];
      position.Y = newPos4d[1] / newPos4d[3];
      position.Z = newPos4d[2] / newPos4d[3];

      float[] arrayNor = new float[4] { normal.X, normal.Y, normal.Z, 1 };
      var normal4d = MathNet.Numerics.LinearAlgebra.Vector<float>.Build.DenseOfArray(arrayNor);
      var newNor4d = normal4d * tranformMatrix;
      normal.X = newNor4d[0] / newNor4d[3];
      normal.Y = newNor4d[1] / newNor4d[3];
      normal.Z = newNor4d[2] / newNor4d[3];
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

    public override bool IsPointInside(Vector3 point)
    {
      return DistanceCalculator.GetDistance(point, position) < r - Constants.epsilon;
    }

    public Vector3 GetNormal(Vector3 intersectionPoint)
    {
      Vector3 normal = new Vector3((intersectionPoint.X-position.X)/r, (intersectionPoint.Y-position.Y)/r, (intersectionPoint.Z-position.Z)/r);
      normal = Vector3.Normalize(normal);
      return normal;
    }
    public override Vector3 GetNormal(Vector3 intersectionPoint, Vector3 origin)
    {
      Vector3 normal = GetNormal(intersectionPoint);
      if (IsPointInside(origin)) return -normal;
      return normal;
    }

    public override void Transform(Matrix<float> tranformMatrix)
    {
      float[] arrayPos = new float[4] { position.X, position.Y, position.Z, 1 };
      var position4d = MathNet.Numerics.LinearAlgebra.Vector<float>.Build.DenseOfArray(arrayPos);
      var newPos4d = position4d * tranformMatrix;
      position.X = newPos4d[0] / newPos4d[3];
      position.Y = newPos4d[1] / newPos4d[3];
      position.Z = newPos4d[2] / newPos4d[3];
    }
  }
}


