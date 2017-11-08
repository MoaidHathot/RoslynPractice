using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using static CSharp6CastingAnalyzer.NullPropagationAnalysisEngine;

namespace CSharp6CastingAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CSharp6CastingAnalyzerCodeFixProvider)), Shared]
    public class CSharp6CastingAnalyzerCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Use Null Propagation";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(CSharp6CastingAnalyzerAnalyzer.DiagnosticId);

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
        
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => UseNullPropagation(context.Document, declaration, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private async Task<Document> UseNullPropagation(Document document, IfStatementSyntax ifStatement, CancellationToken cancellationToken)
        {
            var tuple = FindIdentifierBeingCheckedforNull(ifStatement);
  
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var oldAccessExpression = (MemberAccessExpressionSyntax)tuple.identifierUsed.Parent;
            var newAccessExpression = SyntaxFactory.ConditionalAccessExpression(oldAccessExpression.Expression, SyntaxFactory.MemberBindingExpression(oldAccessExpression.OperatorToken, oldAccessExpression.Name));

            var oldParentExpression = !oldAccessExpression.Parent.Parent.IsKind(SyntaxKind.LogicalAndExpression) ? oldAccessExpression.Parent.Parent : oldAccessExpression.Parent;
        
            var newParentExpression = oldParentExpression.ReplaceNode(oldAccessExpression, newAccessExpression);

            var oldIfStatement = (IfStatementSyntax) tuple.identifierUsed.Ancestors().First(node => node.IsKind(SyntaxKind.IfStatement));
            var newIfStatement = SyntaxFactory.IfStatement(newParentExpression as ExpressionSyntax, oldIfStatement.Statement);
            
            var newRoot = root.ReplaceNode(oldIfStatement, newIfStatement);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}