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

        void CastingTest()
        {
            object foo = Activator.CreateInstance(typeof(string));

            if (foo is string)
            {
                var length = ((string)foo).Length;

                var ignored = foo.ToString();

                var trimmed = ((string)foo).Trim();
            }
            
            if (foo is string foo2)
            {
                var length = foo2.Length;
            }
        }

        void NullPropagationTest()
        {
            var foo = "moaid";

            if(foo != null && foo.Length > 30)
            {
                
            }
        }
    }
}
