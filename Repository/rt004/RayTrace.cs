using System.Numerics;

namespace rt004
{
  class RayTracer
  {
    /// <summary>
    /// Traces given ray through scene and computes its final color
    /// </summary>
    /// <remarks>Thread safe</remarks>

    /// <param name="ray0">Ray to be traced further</param>
    /// <param name="remainingRecursions">How many times should ray reflect and refract recursively</param>
    /// <param name="scene">Information about the whole scene world</param>
    /// <returns>Color which the given ray should display in its starting point</returns>
    public static Color RayTrace(Ray ray0, int remainingRecursions, Scene scene)
    {
      Ray currentRay = ray0;
      double? t;
      Solid? currentSolid;
      scene.solids.GetClosestIntersection(currentRay, out t, out currentSolid);
      if (t is null || t <= 0 || currentSolid is null) return scene.imageParameters.BackgroundColor;
      Vector3 currentIntersection = currentRay.PositionAfterT((float)t);

      Vector3 outColor = scene.lights.GetColor(currentSolid, currentIntersection, currentRay, scene.solids);


      //Parallel version is about 6x slower then sequential - too much pressure in ThreadPool? Wrong parallelism implementation?
      //Task[] recursionTasks = new Task[2];

      if (remainingRecursions > 0)
      {
        //reflection
        //var reflTask = Task.Run(() =>
        //{
        Vector3 reflectVec = Vector3.Reflect(currentRay.vector, currentSolid.GetNormal(currentIntersection, currentRay.position));

        Ray newReflectedRay = new Ray(currentIntersection, reflectVec);

        Vector3 rayTracedColor = RayTrace(newReflectedRay, remainingRecursions - 1, scene);

        outColor += (float)Math.Pow(currentSolid.Material.Reflection, scene.imageParameters.RtRecursionDepth - remainingRecursions + 1) * rayTracedColor; //+= doesn't expose shared memory
                                                                                                                                                          //});

        //recursionTasks[0] = reflTask;



        //refraction


        //var refrTask = Task.Run(() =>
        //{
        Vector3 furtherIntersection = currentRay.PositionAfterT((float)(t + Constants.epsilon));

        Solid? originSolid = scene.solids.GetSolidOfAPoint(currentRay.position);
        double originRefrIndex = 1;
        if (originSolid is not null)
          originRefrIndex = originSolid.Material.RefractionIndex;
        Solid? outsideSolid = scene.solids.GetSolidOfAPoint(furtherIntersection);
        double outsideRefrIndex = 1;
        if (outsideSolid is not null) { outsideRefrIndex = outsideSolid.Material.RefractionIndex; }

        Vector3? refractVec = VectorCalculator.GetRefractedVector(originRefrIndex, outsideRefrIndex, currentSolid.GetNormal(currentIntersection, currentRay.position), currentRay.vector);
        if (refractVec is not null && currentSolid.Material.Transparency > 0)
        {

          Ray newRefractedRay = new Ray(furtherIntersection, (Vector3)refractVec);

          Vector3 refractedColor = RayTrace(newRefractedRay, remainingRecursions - 1, scene);

          outColor += (float)Math.Pow(currentSolid.Material.Transparency, scene.imageParameters.RtRecursionDepth - remainingRecursions + 1) * refractedColor; //+= doesn't expose shared memory

        }
        //});

        //recursionTasks[1] = refrTask;
        //Task.WaitAll(recursionTasks);
      }
      return outColor;
    }
  }
}