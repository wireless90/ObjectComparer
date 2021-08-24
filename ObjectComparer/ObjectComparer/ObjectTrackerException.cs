using System;

namespace ObjectComparer
{
    [Serializable]
    public class ObjectTrackerException : Exception
    {
        public ObjectTrackerException(string message) : base(message) { }
        protected ObjectTrackerException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
