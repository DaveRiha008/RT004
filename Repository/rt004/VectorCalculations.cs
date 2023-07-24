using System.Numerics;

namespace rt004
{

  /// <summary>
  /// Class <c>VectorCalculator</c> contains functions working with vectors and positions
  /// </summary>
  static class VectorCalculator
  {

    /// <summary>
    /// Counts distance between 2 points
    /// </summary>
    /// <param name="point1">First position</param>
    /// <param name="point2">Second position</param>
    static public double GetDistance(Position3D point1, Position3D point2)
    {
      double xDistance = Math.Abs(point1.x - point2.x);
      double yDistance = Math.Abs(point1.y - point2.y);
      double zDistance = Math.Abs(point1.z - point2.z);
      double distance = Math.Sqrt(xDistance * xDistance + yDistance * yDistance);
      distance = Math.Sqrt(distance * distance + zDistance * zDistance);
      return distance;
    }

    /// <summary>
    /// Calculates vector leading from the first point to the second
    /// </summary>
    /// <param name="point1">First position</param>
    /// <param name="point2">Second position</param>
    public static Vector3 GetVectorBetween2Points(Position3D point1, Position3D point2)
    {
      Vector3 outVec = new Position3D(point2.x - point1.x, point2.y - point1.y, point2.z - point1.z);
      outVec = Vector3.Normalize(outVec);
      return outVec;
    }


    /// <summary>
    /// Calculates refracted vector of a surface between 2 enviroments
    /// </summary>
    /// <param name="refractionIndex1">Refraction index of the first enviroment (from which the ray came)</param>
    /// <param name="refractionIndex2">Refraction index of the second enviroment (to which the ray continues)</param>
    /// <param name="normal">Normal of surface in the point of refraction</param>
    /// <param name="vector">Vector which is being refracted</param>
    /// <returns>Null if the refracted angle exceeds the critical angle</returns>
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


    /// <summary>
    /// Calculates simple vector magnitude
    public static double GetMagnitude(Vector3 vector)
    {
      return Math.Sqrt(Math.Pow(vector.X, 2) + Math.Pow(vector.Y, 2) + Math.Pow(vector.Z, 2));
    }
  }
}