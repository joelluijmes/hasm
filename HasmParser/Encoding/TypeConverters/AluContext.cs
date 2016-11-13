using System;
using System.ComponentModel;

namespace hasm.Parsing.Encoding.TypeConverters
{
    internal sealed class AluContext : ITypeDescriptorContext
    {
        public AluContext(object instance, string propertyName)
        {
            Instance = instance;
            PropertyDescriptor = TypeDescriptor.GetProperties(instance)[propertyName];
        }

        public object GetService(Type serviceType) => null;

        public bool OnComponentChanging() => true;

        public void OnComponentChanged() {}

        public IContainer Container { get; private set; }
        public object Instance { get; }
        public PropertyDescriptor PropertyDescriptor { get; }
    }
}
