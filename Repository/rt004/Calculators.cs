using System.Numerics;

namespace rt004
{
  static class DistanceCalculator
  {
    static public double GetDistance(Vector3 v1, Vector3 v2)
    {
      double xDistance = Math.Abs(v1.X - v2.X);
      double yDistance = Math.Abs(v1.Y - v2.Y);
      double zDistance = Math.Abs(v1.Z - v2.Z);
      double distance = Math.Sqrt(xDistance * xDistance + yDistance * yDistance);
      distance = Math.Sqrt(distance * distance + zDistance * zDistance);
      return distance;
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
      if (refractionIndex1 > refractionIndex2)       //Check total internal reflection
      {
        double critAngle = Math.Asin(refractionIndex2 / refractionIndex1);
        double incidentAngle = Math.Acos(Vector3.Dot(vector, normal) / (GetMagnitude(vector) * GetMagnitude(normal)));

        if (incidentAngle >= critAngle) { return null; }
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
}