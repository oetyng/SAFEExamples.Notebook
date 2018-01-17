using System;
using System.Collections.Generic;

namespace SAFE.DotNET.Auth
{
    public class DependencyService
    {
        static Dictionary<string, object> _instances = new Dictionary<string, object>();
        static Dictionary<string, object> Instances
        {
            get
            {
                if (_instances == null)
                    _instances = new Dictionary<string, object>();
                return _instances;
            }
        }

        static DependencyService()
        {
            Register<Native.INativeBindings, Native.NativeBindings>(new Native.NativeBindings());
            Register<Utils.IFileOps, Utils.FileOps>(new Utils.FileOps());
            Register<Services.AuthService, Services.AuthService>(new Services.AuthService());
        }

        public static T Get<T>()
        {
            var key = typeof(T).Name;
            if (!Instances.ContainsKey(key))
                Instances[key] = Activator.CreateInstance<T>();
            return (T)Instances[key];
        }

        public static void Register<TInterface, TInstance>()
        {
            var key = typeof(TInterface).Name;
            if (!Instances.ContainsKey(key))
                Instances[key] = Activator.CreateInstance<TInstance>();
        }

        public static void Register<TInterface, TInstance>(TInstance instance)
        {
            var key = typeof(TInterface).Name;
            if (!Instances.ContainsKey(key))
                Instances[key] = instance;
        }
    }
}