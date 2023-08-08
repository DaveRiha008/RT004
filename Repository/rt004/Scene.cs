using System.Numerics;

namespace rt004
{
  /// <summary>
  /// Contains all information about scene - base for drawing
  /// </summary>
  class Scene
  {
    public AllSolids solids = new AllSolids();

    public Camera camera = new Camera(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);

    public AllLights lights = new AllLights();

    public ImageParameters imageParameters = new ImageParameters();

    public AnimationInfo? animationInfo;
    public Scene() { }
    public Scene(AllSolids solids, Camera camera, AllLights lights, ImageParameters imageParameters)
    {
      this.solids = solids;
      this.camera = camera;
      this.lights = lights;
      this.imageParameters = imageParameters;
    }
  }
}