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
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using static BetterCastingAnalyzer.AnalysisEngine;

namespace BetterCastingAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BetterCastingAnalyzerCodeFixProvider)), Shared]
    public class BetterCastingAnalyzerCodeFixProvider : CodeFixProvider
    {
        private const string title = "Make uppercase";

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
                    title: title,
                    createChangedDocument: c => UseBetterCast(context.Document, declaration, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> UseBetterCast(Document document, IfStatementSyntax ifStatement, CancellationToken cancellationToken)
        {
            var tuple = FindIdentifierBeingCastedTwice(ifStatement);

            var root = await document.GetSyntaxRootAsync(cancellationToken);

            //var localDeclarationStatement = SyntaxFactory.LocalDeclarationStatement(
            //    declaration: SyntaxFactory.VariableDeclaration(
            //        type: SyntaxFactory.IdentifierName(SyntaxFactory.Token(SyntaxKind.TypeVarKeyword)),
            //        variables: SyntaxFactory.SeparatedList(new[] {
            //            SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier("foo2"))})));

            
            var localDeclarationStatement =
                SyntaxFactory.ParseStatement($"var baz = {tuple.identifier.Identifier.ValueText} as {tuple.predefinedType.Keyword.ValueText};").WithLeadingTrivia(ifStatement.GetLeadingTrivia());

            root = root.ReplaceNode(ifStatement, SyntaxFactory.IfStatement(SyntaxFactory.ParseExpression($"null != baz"), ifStatement.Statement));

            var oldMemberAccessExpression = tuple.identifier.Ancestors().OfType<MemberAccessExpressionSyntax>().First();
            var newMemberAccessExpression =
                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName("baz"),
                    oldMemberAccessExpression.Name);

            root = root.ReplaceNode(oldMemberAccessExpression, newMemberAccessExpression);


            //tuple = FindIdentifierBeingCastedTwice(tuple.ifStatement);

            root = root.InsertNodesBefore(ifStatement, new[] { localDeclarationStatement });

            return document.WithSyntaxRoot(root);
            //var declarationStatement = SyntaxFactory.LocalDeclarationStatement(, )

            //return document;
        }
    }
}