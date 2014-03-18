using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Pyrrha.Scripting.Util
{
    public class ParamsDelegate<T> where T : Delegate
    {
        private dynamic callable;

        public ParamsDelegate(MethodInfo method, T dele)
        {
            var typeNeeded = typeof(Delegate).Assembly.GetTypes().First(type
                => type.Name.Equals(string.Format("{0}`{1}", dele.GetType().Name, method.GetParameters().Count())));

            callable = typeNeeded.GetConstructor(method.GetParameters().Select(param => param.GetType()).ToArray()).Invoke(new []{dele.Method});
        }

        public void Invoke();
    }
}
