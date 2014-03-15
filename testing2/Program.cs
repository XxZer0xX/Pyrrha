using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace testing2
{
    class Program
    {
        public static void Main(string[] args)
        {
            var objCtr =
                    typeof (Action).Assembly.GetTypes().First(type => type.Name.Equals(string.Format("Action`{0}",3)));

            var ctr = objCtr.GetConstructors( ).First();
            ctr.Invoke(new[] { new object(), new object(), new object() });
        }
    }
}
