namespace rt004
{
  class PropertyNotDescribedException : Exception
  {
    public PropertyNotDescribedException() { }
    public PropertyNotDescribedException(string message) : base(message) { }
  }
}