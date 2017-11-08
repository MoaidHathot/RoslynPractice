using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static BetterCastingAnalyzer.AnalysisEngine;

namespace BetterCastingAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class BetterCastingAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "BetterCastingAnalyzer";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Refactoring";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information

            context.RegisterSyntaxNodeAction(AnalyzeIfStatements, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatements(SyntaxNodeAnalysisContext context)
        {
            var tuple = FindIdentifierBeingCastedTwice(context.Node);

            if (null != tuple.identifiers)
            {
                var first = tuple.identifiers.First();
                var additionalLocations = tuple.identifiers.Skip(1).Select(identifier => identifier.GetLocation());

                var diagnostic = Diagnostic.Create(Rule, first.GetLocation(), additionalLocations, first.Identifier.ValueText);
                context.ReportDiagnostic(diagnostic);

                //foreach (var identifier in tuple.identifiers)
                //{
                //    var diagnostic = Diagnostic.Create(Rule, identifier.GetLocation(), identifier.Identifier.ValueText);
                //    Diagnostic.Create(Rule, )
                //    context.ReportDiagnostic(diagnostic);
                //}

                //var diagnostic = Diagnostic.Create(Rule, tuple.identifier.GetLocation(), tuple.identifier.Identifier.ValueText);

                //context.ReportDiagnostic(diagnostic);
            }

            //context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), tuple.identifier.Identifier.ValueText));
        }
    }
}
