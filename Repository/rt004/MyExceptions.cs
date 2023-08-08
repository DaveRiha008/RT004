namespace rt004
{
  class PropertyNotDescribedException : Exception
  {
    public PropertyNotDescribedException() { }
    public PropertyNotDescribedException(string message) : base(message) { }
  }

  class AnimationFailedException : Exception
  {
    public AnimationFailedException() { }
    public AnimationFailedException(string message) : base(message) { }
  } 
}