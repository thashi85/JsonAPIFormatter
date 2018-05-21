using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JsonAPISerializer.Util
{
    public class Utility
    {
        private static Assembly[] _assembly;
        public static Assembly[] GetAssemblies()
        {
            if (_assembly == null)
                 _assembly=AppDomain.CurrentDomain.GetAssemblies();
                // _assembly = Assembly.GetExecutingAssembly();
               
            return _assembly;
        }
    }
}
