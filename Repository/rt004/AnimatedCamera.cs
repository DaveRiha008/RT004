using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Threading;
using System.Numerics;
using System.Timers;
using MathNet.Numerics.Distributions;

namespace rt004
{
  class KeyFrame
  {
    public double minTime;
    public double maxTime;
    public Vector3 pos;
    public Vector3 lookat;
    public Vector3 up;
    public KeyFrame(double time0, double time1, Vector3 pos, Vector3 lookat, Vector3 up)
    {
      this.minTime = time0;
      this.maxTime = time1;
      this.pos = pos;
      this.lookat = lookat;
      this.up = up;
    }
  }
  class AnimatedCamera
  {
    const float MinTime = 0;
    float MaxTime = 1;

    /// <summary>
    /// Updates all properties of animated camera - use when changing camera script or creating AnimatedCamera
    /// </summary>
    /// <param name="cameraFile">Name of camera script file</param>
    /// <exception cref="AnimationFailedException"></exception>
    /// <exception cref="NullReferenceException"></exception>
    void Update(string cameraFile)
    {
      keyFrames.Clear();
      if (cameraFile != null && cameraFile != "")
      {
        try
        {
          StreamReader camFileRead = new StreamReader(cameraFile);
          using (camFileRead)
          {
            try
            {
              LoadCamFileScript(camFileRead);
            }
            catch (NullReferenceException)
            {
              Console.WriteLine("Error: Incorrect camera script format");
              throw new AnimationFailedException();
            }
          }
        }
        catch 
        {
          Console.WriteLine("Error: Failed reading camera script");
          throw new AnimationFailedException();
        }
      }
      if (keyFrames.Count < 4)
      {
        List<KeyFrame> defaultKeyFrames = new List<KeyFrame>
        {
          new KeyFrame(MinTime, MaxTime / 4, new Vector3(0, 0, 0), Vector3.UnitZ, Vector3.UnitY),
          new KeyFrame(MaxTime / 4, MaxTime / 2, new Vector3(-1, 0, 0), Vector3.UnitZ, Vector3.UnitY),
          new KeyFrame(MaxTime / 2, 3 * MaxTime / 4, new Vector3(0, 0, 0), Vector3.UnitZ, Vector3.UnitY),
          new KeyFrame(3 * MaxTime / 4, MaxTime, new Vector3(1, 0, 0), Vector3.UnitZ, Vector3.UnitY)
        };
        keyFrames = defaultKeyFrames;
      }

      keyFrames[0].minTime = MinTime;

      if (keyFrames[keyFrames.Count - 1].maxTime != MaxTime)
        keyFrames.Add(new KeyFrame(keyFrames[keyFrames.Count - 1].maxTime, MaxTime, keyFrames[0].pos, keyFrames[0].lookat, keyFrames[0].up));

      void LoadCamFileScript(StreamReader input)
      {
        bool firstIteration = true;
        double lastTime = 0;
        double time = 0;
        Vector3 pos = new Vector3();
        Vector3 lookat = new Vector3();
        Vector3 up = new Vector3();
        Vector3 lastPos = new Vector3();
        Vector3 lastLookat = new Vector3();
        Vector3 lastUp = new Vector3();
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        string? line = input.ReadLine();
        while (!input.EndOfStream && line != "end" && line is not null)
        {
          if (line == "" || line[0] == '#') continue;
          Dictionary<string, string> pars = Util.ParseKeyValueList(line);
          if (pars.TryGetValue("t", out string? t))
          {
            if (t is null) throw new NullReferenceException();
            time = /*MaxTime * */double.Parse(t);
          }
          if (pars.TryGetValue("pos", out string? pos_))
          {
            if (pos_ is null) throw new NullReferenceException();
            pos_ = pos_.Substring(1, pos_.Length - 2);
            string[] posCoord = pos_.Split(';');
            pos = new Vector3(float.Parse(posCoord[0]), float.Parse(posCoord[1]), float.Parse(posCoord[2]));
          }
          if (pars.TryGetValue("lookat", out string? lookat_))
          {
            if (lookat_ is null) throw new NullReferenceException();
            lookat_ = lookat_.Substring(1, lookat_.Length - 2);
            string[] lookatCoord = lookat_.Split(';');
            lookat = new Vector3(float.Parse(lookatCoord[0]), float.Parse(lookatCoord[1]), float.Parse(lookatCoord[2]));
          }
          if (pars.TryGetValue("up", out string? up_))
          {
            if (up_ is null) throw new NullReferenceException();
            up_ = up_.Substring(1, up_.Length - 2);
            string[] upCoord = up_.Split(';');
            up = new Vector3(float.Parse(upCoord[0]), float.Parse(upCoord[1]), float.Parse(upCoord[2]));
          }
          if (!firstIteration) keyFrames.Add(new KeyFrame(lastTime, time, lastPos, lastLookat, lastUp));
          lastPos = pos;
          lastLookat = lookat;
          lastUp = up;
          lastTime = time;
          firstIteration = false;
          line = input.ReadLine();
        }
        keyFrames.Add(new KeyFrame(time, MaxTime, pos, lookat, up)); //Add the last keyframe with ending at an end of the whole animation

      }
    }

    /// <summary>
    /// Radius of camera trajectory.
    /// </summary>
    float radius = 1.0f;
    List<KeyFrame> keyFrames = new List<KeyFrame>();
    int currentPhase = 0;

    /// <param name="cameraFile">Optional file-name of your custom camera definition (camera script?).</param>
    public AnimatedCamera(float maxTime, string cameraFile = "")
    { 
      MaxTime = maxTime;
      Update(cameraFile);
    }

    public static void MakeAnimation(AnimationInfo animInfo, Scene scene)
    {
      int frameCounter = 0;
      AnimatedCamera animatedCamera;
      try
      {
        animatedCamera = new AnimatedCamera(animInfo.AnimationLength, animInfo.CameraScript);
      }
      catch (AnimationFailedException)
      {
        Console.WriteLine("Error: Failed to create animated camera - stopping animation creation");
        return;
      }
      float timeBetweenFrames = 1f / animInfo.Fps;
      float currentTime = MinTime;
      while (currentTime <= animInfo.AnimationLength)
      {

        animatedCamera.GetCameraInExactTime(currentTime, out Position3D newPos, out Position3D newLookAt, out Position3D newUpVec);
        scene.camera.Position = newPos;
        scene.camera.ViewVector = VectorCalculator.GetVectorBetween2Points(newPos, newLookAt);
        scene.camera.UpVector = newUpVec;

        Console.WriteLine("Making frame number " + frameCounter.ToString());
        ImageCreator.CreateAndSaveHDRImage(scene, "Animation/Frame" + frameCounter.ToString() + ".pfm");


        currentTime += timeBetweenFrames;
        frameCounter++;
      }
    }

    /// <summary>
    /// I'm using internal ModelView matrix computation.
    /// </summary>
    public void GetCameraInExactTime(float Time, out Position3D pos, out Position3D lookat, out Position3D up)
    {
      while (Time > keyFrames[currentPhase].maxTime && currentPhase <= keyFrames.Count - 1) currentPhase++;
      while (Time < keyFrames[currentPhase].minTime && currentPhase >= 0) currentPhase--;

      KeyFrame prevKeyFrame;
      if (currentPhase == 0)
        prevKeyFrame = keyFrames[keyFrames.Count - 1];
      else
        prevKeyFrame = keyFrames[currentPhase - 1];

      KeyFrame curKeyFrame = keyFrames[currentPhase];

      KeyFrame nextKeyFrame;
      if (currentPhase == keyFrames.Count - 1) nextKeyFrame = keyFrames[0];
      else nextKeyFrame = keyFrames[currentPhase + 1];

      KeyFrame nextNextKeyFrame;
      if (currentPhase == keyFrames.Count - 2)
        nextNextKeyFrame = keyFrames[0];
      else if (currentPhase == keyFrames.Count - 1)
        nextNextKeyFrame = keyFrames[1];
      else
        nextNextKeyFrame = keyFrames[currentPhase + 2];

      float t = (float)((Time - curKeyFrame.minTime) / (curKeyFrame.maxTime - curKeyFrame.minTime));

      Position3D p0 = prevKeyFrame.pos;
      Position3D p1 = curKeyFrame.pos;
      Position3D p2 = nextKeyFrame.pos;
      Position3D p3 = nextNextKeyFrame.pos;
      pos = GetCatmullRomPosition(t, p0, p1, p2, p3);

      Position3D l0 = prevKeyFrame.lookat;
      Position3D l1 = curKeyFrame.lookat;
      Position3D l2 = nextKeyFrame.lookat;
      Position3D l3 = nextNextKeyFrame.lookat;
      lookat = GetCatmullRomPosition(t, l0, l1, l2, l3);

      Position3D u0 = prevKeyFrame.up;
      Position3D u1 = curKeyFrame.up;
      Position3D u2 = nextKeyFrame.up;
      Position3D u3 = nextNextKeyFrame.up;
      up = GetCatmullRomPosition(t, u0, u1, u2, u3);

    }
    static Position3D GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
      //The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
      Position3D a = 2f * p1;
      Position3D b = p2 - p0;
      Position3D c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
      Position3D d = -p0 + 3f * p1 - 3f * p2 + p3;

      //The cubic polynomial: a + b * t + c * t^2 + d * t^3
      Position3D pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

      return pos;
    }
  }
}
