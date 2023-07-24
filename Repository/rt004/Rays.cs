using System.Numerics;

namespace rt004
{
 /// <summary>
 /// Describes a ray going from its position in a direction described by a vector
 /// </summary>
  struct Ray
  {
    public Position3D position = new();
    public Vector3 vector = new();
    public int index;
    public Ray(Position3D position, Vector3 vector, int index = 0)
    {
      this.position = position;
      this.vector = vector;
      this.index = index;
    }

    /// <summary>
    /// Calculates a position, where ray arrives after t steps in its direction
    /// </summary>
    public Vector3 PositionAfterT(float t)
    {
      return this.position + this.vector * t;
    }
  }
}