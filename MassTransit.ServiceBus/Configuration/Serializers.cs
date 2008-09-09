namespace MassTransit.ServiceBus.Configuration
{
    public static class Serializers
    {
        public static SerializationOptions Binary
        {
            get { return new SerializationOptions{Serializer = typeof(object)}; }
        }

        public static SerializationOptions Xml
        {
            get { return new SerializationOptions{Serializer = typeof(object)}; }
        }

        public static SerializationOptions Custom<T>()
        {
            return new SerializationOptions{Serializer = typeof(T)};
        }
    }
}