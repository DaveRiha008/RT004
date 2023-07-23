#nullable enable

using System.Numerics;
using MathNet.Numerics.LinearAlgebra;

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
    //Warning: All property names dependent on json config

    public Color? Color { get; set; }
    public Material Material { get; set; } = new Material(); 
    public abstract void GetIntersection(Ray ray, out double? outT);
    public abstract bool IsPointInside(Vector3 point);
    public abstract Vector3 GetNormal(Vector3 intersectionPoint, Vector3 origin);
    public abstract void Transform(Matrix<float> tranformMatrix);
  }


  class Plane : Solid
  {
    //Warning: All property names dependent on json config

    public Position3D Position { get; set; } = Vector3.Zero;
    public Position3D Normal { get; set; } = Vector3.UnitY;
    
    public Plane(Position3D position, Position3D normal, Color color, Material material)
    {
      this.Position = position;
      this.Normal = Vector3.Normalize(normal);
      this.Color = color;
      this.Material = material;
    }



    //TODO propper equation function (for total internal reflection suffices false)
    public override bool IsPointInside(Vector3 point)
    {
      return false;
    }

    bool ShouldReverseNormal(Vector3 point)
    {
      Ray ray = new Ray(point, VectorCalculator.GetVectorBetween2Points((Position + Normal), point));
      double? T;
      GetIntersection(ray, out T);
      if (T is null) return false;
      else return true;
    }

    public Vector3 GetNormal(Vector3 intersectionPoint)
    {
      return Normal;
    }
    public override Vector3 GetNormal(Vector3 intersectionPoint, Vector3 origin)
    {
      if (ShouldReverseNormal(origin)) return Normal;
      return -Normal;
    }

    public override void GetIntersection(Ray ray, out double? outT)
    {
      outT = null;
      Vector3 p_0 = Position;
      Vector3 n = -Normal;
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
      float[] arrayPos = new float[4] { Position.x, Position.y, Position.z, 1 };
      var position4d = MathNet.Numerics.LinearAlgebra.Vector<float>.Build.DenseOfArray(arrayPos);
      var newPos4d = position4d * tranformMatrix;
      Position = new Position3D(newPos4d[0], newPos4d[1], newPos4d[2]);
      Position = Position / newPos4d[3];


      float[] arrayNor = new float[4] { Normal.x, Normal.y, Normal.z, 1 };
      var normal4d = MathNet.Numerics.LinearAlgebra.Vector<float>.Build.DenseOfArray(arrayNor);
      var newNor4d = normal4d * tranformMatrix;
      Normal = new Position3D(newNor4d[0], newNor4d[1], newNor4d[2]);
      Normal = Normal / newNor4d[3];

    }
  }

  class Sphere:Solid
  {
    //Warning: All property names dependent on json config

    public Position3D Position { get; private set; } = Vector3.Zero;
    public float Radius { get; private set; } = 0;
    public Sphere(Position3D position, float radius, Color color, Material material)
    {
      this.Position = position;
      this.Radius = radius;
      this.Color = color;
      this.Material = material;
    }

    public override void GetIntersection(Ray ray, out double? outT)
    {
      outT = null;
      float cx = Position.x;
      float cy = Position.y;
      float cz = Position.z;
      float dx = ray.vector.X;
      float dy = ray.vector.Y;
      float dz = ray.vector.Z;
      float x0 = ray.position.x;
      float y0 = ray.position.y;
      float z0 = ray.position.z;

      float a = dx * dx + dy * dy + dz * dz;
      float b = 2 * dx * (x0 - cx) + 2 * dy * (y0 - cy) + 2 * dz * (z0 - cz);
      float c = cx * cx + cy * cy + cz * cz + x0 * x0 + y0 * y0 + z0 * z0 +
                  -2 * (cx * x0 + cy * y0 + cz * z0) - Radius*Radius;
      
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
      return DistanceCalculator.GetDistance(point, Position) < Radius - Constants.epsilon;
    }

    public Vector3 GetNormal(Vector3 intersectionPoint)
    {
      Vector3 normal = new Vector3((intersectionPoint.X-Position.x)/Radius, (intersectionPoint.Y-Position.y)/Radius, (intersectionPoint.Z-Position.z)/Radius);
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
      float[] arrayPos = new float[4] { Position.x, Position.y, Position.z, 1 };
      var position4d = MathNet.Numerics.LinearAlgebra.Vector<float>.Build.DenseOfArray(arrayPos);
      var newPos4d = position4d * tranformMatrix;
      Position = new Position3D(newPos4d[0], newPos4d[1], newPos4d[2]);
      Position /= newPos4d[3];
    }
  }
}


