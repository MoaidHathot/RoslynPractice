using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoslynPractice
{
    class Program
    {
        static void Main(string[] args)
        {
        }

        static void CastingTest()
        {
            object foo = Activator.CreateInstance(typeof(string));

            if (foo is string)
            {
                var length = ((string)foo).Length;
            }


            var foo2 = foo as string;
            if (foo2 != null)
            {
                var length = foo2.Length;
            }

        }
    }
}
