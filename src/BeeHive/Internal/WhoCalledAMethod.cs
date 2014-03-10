using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Internal
{
    internal class WhoCalledAMethod
    {
        private string _fullClassName;
        private string _methodPrefix;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fullClassName">With namespace</param>
        /// <param name="methodPrefix"></param>
        public WhoCalledAMethod(string fullClassName, string methodPrefix)
        {
            _methodPrefix = methodPrefix;
            _fullClassName = fullClassName;
        }

        public MethodBase GetTheMethod()
        {
            var stackTrace = new StackTrace();
            bool getTheNextGuy = false;
            foreach (var frame in stackTrace.GetFrames())
            {
                var method = frame.GetMethod();
                if (getTheNextGuy && !method.DeclaringType.Name.Contains("<"))
                    return method;
                if (method.DeclaringType.ToString() == _fullClassName
                    && method.Name.StartsWith(_methodPrefix))
                    getTheNextGuy = true;
            }

            return null;
        }
    }
}
