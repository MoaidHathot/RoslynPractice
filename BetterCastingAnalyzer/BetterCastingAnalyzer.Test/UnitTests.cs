using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using BetterCastingAnalyzer;

namespace BetterCastingAnalyzer.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        //No diagnostics expected to show up
        [TestMethod]
        public void BetterCasting_EmptyString_NothingHappens()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void BetterCasting_1Casting_Success()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {               
            void TestMethod()
            {
                object foo = Activator.CreateInstance(typeof(string));

                if (foo is string)
                {
                    var length = ((string)foo).Length;
                }
            }
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "BetterCastingAnalyzer",
                Message = $"Type name 'foo' can be casted using the 'as statement'",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 19, 43),
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {               
            void TestMethod()
            {
                object foo = Activator.CreateInstance(typeof(string));

                var baz = foo as string;
                if (null != baz)
                {
                    var length = baz.Length;
                }
            }
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        [TestMethod]
        public void BetterCasting_1Casting1Ignored_Success()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {               
            void TestMethod()
            {
                object foo = Activator.CreateInstance(typeof(string));

                if (foo is string)
                {
                    var length = ((string)foo).Length;

                    var ignored = foo.ToString();
                }
            }
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "BetterCastingAnalyzer",
                Message = $"Type name 'foo' can be casted using the 'as statement'",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 19, 43),
                    }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {               
            void TestMethod()
            {
                object foo = Activator.CreateInstance(typeof(string));

                var baz = foo as string;
                if (null != baz)
                {
                    var length = baz.Length;

                    var ignored = foo.ToString();
                }
            }
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        [TestMethod]
        public void BetterCasting_2Casting_Success()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {               
            void TestMethod()
            {
                object foo = Activator.CreateInstance(typeof(string));

                if (foo is string)
                {
                    var length = ((string)foo).Length;

                    var trimmed = ((string)foo).Trim();
                }
            }
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "BetterCastingAnalyzer",
                Message = $"Type name 'foo' can be casted using the 'as statement'",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 19, 43),
                            new DiagnosticResultLocation("Test0.cs", 21, 44)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {               
            void TestMethod()
            {
                object foo = Activator.CreateInstance(typeof(string));

                var baz = foo as string;
                if (null != baz)
                {
                    var length = baz.Length;

                    var trimmed = baz.Trim();
                }
            }
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        [TestMethod]
        public void BetterCasting_2Casting1Ignored_Success()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {               
            void TestMethod()
            {
                object foo = Activator.CreateInstance(typeof(string));

                if (foo is string)
                {
                    var length = ((string)foo).Length;

                    var ignored = foo.ToString();

                    var trimmed = ((string)foo).Trim();
                }
            }
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "BetterCastingAnalyzer",
                Message = $"Type name 'foo' can be casted using the 'as statement'",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 19, 43),
                            new DiagnosticResultLocation("Test0.cs", 23, 44)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {               
            void TestMethod()
            {
                object foo = Activator.CreateInstance(typeof(string));

                var baz = foo as string;
                if (null != baz)
                {
                    var length = baz.Length;

                    var ignored = foo.ToString();

                    var trimmed = baz.Trim();
                }
            }
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        [TestMethod]
        public void BetterCasting_EmptyIf_NothingHappens()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {               
            void TestMethod()
            {
                object foo = Activator.CreateInstance(typeof(string));

                if (foo is string)
                {
                }
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void BetterCasting_NoCasting1Ignored_NothingHappens()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {               
            void TestMethod()
            {
                object foo = Activator.CreateInstance(typeof(string));

                if (foo is string)
                {
                    var ignored = foo.ToString();
                }
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new BetterCastingAnalyzerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new BetterCastingAnalyzerAnalyzer();
        }
    }
}