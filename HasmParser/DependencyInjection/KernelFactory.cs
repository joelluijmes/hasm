﻿using System;
using Ninject;
using Ninject.Parameters;

namespace hasm.Parsing.DependencyInjection
{
    public static class KernelFactory
    {
        private static readonly IKernel _kernel;

        static KernelFactory()
        {
            _kernel = new StandardKernel();
            _kernel.Load(AppDomain.CurrentDomain.GetAssemblies());
        }

        public static T Resolve<T>() => _kernel.Get<T>();

        public static T Resolve<T>(string name) => _kernel.Get<T>(name);

        public static T Resolve<T>(params IParameter[] parameters) => _kernel.Get<T>(parameters);
    }
}
