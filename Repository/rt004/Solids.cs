//using System.Numerics;
#nullable enable

using System.Dynamic;
using System.Numerics;
using System.Security.Cryptography;

namespace rt004
{
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
  class Sphere
  {
    Vector3 position = new();
    double R;
    public Sphere(Vector3 position, double r)
    {
      this.position = position;
      R = r;
    }

    public Vector3? GetIntersection(Ray ray)
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
                   z0 * z0 - 2 * z0 * zc + zc * zc - R * R;
      }
      double A = AFunction();
      double B = BFunction();
      double C = CFunction();
      double? root1;
      double? root2;
      QuadraticEquationSolver.Solve(A, B, C, out root1, out root2);
      Vector3 intersection1 = new Vector3();
      Vector3 intersection2 = new Vector3();
      Vector3 intersection = new Vector3();
      if (root1 is not null)
      {
        intersection1 = new Vector3((float)(x0 * (1 - root1) + root1 * x1), (float)(y0 * (1 - root1) + root1 * y1), (float)(z0 * (1 - root1) + root1 * z1));
        intersection = intersection1;
      }
      if (root2 is not null)
      {
        intersection2 = new Vector3((float)(x0 * (1 - root2) + root2 * x1), (float)(y0 * (1 - root2) + root2 * y1), (float)(z0 * (1 - root2) + root2 * z1));
        intersection = intersection2;
      }
      if (root1 is not null && root2 is not null)
      {
        if (DistanceCalculator.GetDistance(ray.position, intersection1) <= DistanceCalculator.GetDistance(ray.position, intersection2)) intersection = intersection1;
        else intersection = intersection2;
      }
      if (root1 is not null || root2 is not null) return intersection;
      else return null;
    }

  }
}
