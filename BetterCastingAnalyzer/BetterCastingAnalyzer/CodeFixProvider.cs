using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using static BetterCastingAnalyzer.AnalysisEngine;

namespace BetterCastingAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BetterCastingAnalyzerCodeFixProvider)), Shared]
    public class BetterCastingAnalyzerCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Make uppercase";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(BetterCastingAnalyzerAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<IfStatementSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => UseBetterCast(context.Document, declaration, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private async Task<Document> UseBetterCast(Document document, IfStatementSyntax ifStatement, CancellationToken cancellationToken)
        {
            var tuple = FindIdentifierBeingCastedTwice(ifStatement);

            var root = await document.GetSyntaxRootAsync(cancellationToken);

            var oldMemberAccessList = tuple.identifiers
                .Select(identifier => identifier.Ancestors().OfType<MemberAccessExpressionSyntax>().First())
                .Select(oldAccess =>
                    new
                    {
                        oldAccess,
                        newAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName("baz"), oldAccess.Name)
                    }).ToArray();

            root = root.TrackNodes(new SyntaxNode[] { ifStatement }.Concat(oldMemberAccessList.Select(pair => pair.oldAccess)));

            root = oldMemberAccessList.Aggregate(root, (r, pair) => r.ReplaceNode(r.GetCurrentNode(pair.oldAccess), pair.newAccess));

            var oldIfStatement = root.GetCurrentNode(ifStatement);
  
            var localDeclarationStatement =
                SyntaxFactory.ParseStatement($"var baz = {tuple.identifiers.First().Identifier.ValueText} as {tuple.predefinedType.Keyword.ValueText};")
                    .WithLeadingTrivia(root.GetCurrentNode(ifStatement).GetLeadingTrivia());

            root = root.InsertNodesBefore(oldIfStatement, new[] { localDeclarationStatement });
            oldIfStatement = root.GetCurrentNode(ifStatement);

            root = root.ReplaceNode(oldIfStatement.Condition, SyntaxFactory.ParseExpression($"null != baz"));
 
            return document.WithSyntaxRoot(root);
        }
    }
}