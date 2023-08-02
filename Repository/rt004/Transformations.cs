using MathNet.Numerics.LinearAlgebra;

namespace rt004
{
  /// <summary>
  /// Holds all information about any solid transformation
  /// </summary>
  struct TransformInfo
  {
    //Warning: All property names dependent on json config

    public float TranslateX { get; set; }
    public float TranslateY { get; set; }
    public float TranslateZ { get; set; }
    public float RotateX { get; set; }
    public float RotateY { get; set; }
    public float RotateZ { get; set; }



    /// <returns>Translation matrix which when applied to shape translates it by desired values</returns>
    public static Matrix<float> TranslationMatrix(float x, float y, float z)
    {
      float[,] array = new float[4, 4]
      { {1,0,0,0},
        {0,1,0,0},
        {0,0,1,0},
        {x,y,z,1} };
      return Matrix<float>.Build.DenseOfArray(array);
    }

    /// <param name="angle">Angle in degrees</param>
    /// <returns>Rotation matrix which when applied to shape rotates it by desired angle around X axis</returns>
    public static Matrix<float> RotationXMatrix(float angle)
    {
      double angleRad = angle * (Math.PI / 180);

      float cos = (float)Math.Cos(angleRad);
      float sin = (float)Math.Sin(angleRad);
      float[,] array = new float[4, 4]
      { {1,0,0,0},
        {0,cos,-sin,0},
        {0,sin,cos,0},
        {0,0,0,1} };
      return Matrix<float>.Build.DenseOfArray(array);
    }


    /// <param name="angle">Angle in degrees</param>
    /// <returns>Rotation matrix which when applied to shape rotates it by desired angle around Y axis</returns>
    public static Matrix<float> RotationYMatrix(float angle)
    {
      double angleRad = angle * (Math.PI / 180);

      float cos = (float)Math.Cos(angleRad);
      float sin = (float)Math.Sin(angleRad);
      float[,] array = new float[4, 4]
      { {cos,0,sin,0},
        {0,1,0,0},
        {-sin,0,cos,0},
        {0,0,0,1} };
      return Matrix<float>.Build.DenseOfArray(array);
    }

    /// <param name="angle">Angle in degrees</param>
    /// <returns>Rotation matrix which when applied to shape rotates it by desired angle around Z axis</returns>
    public static Matrix<float> RotationZMatrix(float angle)
    {
      double angleRad = angle * (Math.PI / 180);

      float cos = (float)Math.Cos(angleRad);
      float sin = (float)Math.Sin(angleRad);
      float[,] array = new float[4, 4]
      { {cos,-sin,0,0},
        {sin,cos,0,0},
        {0,0,1,0},
        {0,0,0,1} };
      return Matrix<float>.Build.DenseOfArray(array);
    }

    /// <summary>Makes a matrix out of this transform info</summary>
    /// <returns>Transformation matrix modifying shapes when multiplying their position</returns>
    public Matrix<float> GetTransformMatNew()
    {
      Matrix<float> resultMatrix = Matrix<float>.Build.DenseIdentity(4, 4);


      resultMatrix *= TranslationMatrix(TranslateX, TranslateY, TranslateZ);
      resultMatrix *= RotationXMatrix(RotateX);
      resultMatrix *= RotationYMatrix(RotateY);
      resultMatrix *= RotationZMatrix(RotateZ);

      return resultMatrix;
    }

  }
}