using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BetterCastingAnalyzer
{
    internal static class AnalysisEngine
    {
        public static (IdentifierNameSyntax identifier, IfStatementSyntax ifStatement, PredefinedTypeSyntax predefinedType) FindIdentifierBeingCastedTwice(SyntaxNode node)
        {
            if (node is IfStatementSyntax ifStatement && ifStatement.Condition.IsKind(SyntaxKind.IsExpression))
            {
                var castedIdentifier = ifStatement.Condition.DescendantNodes().OfType<IdentifierNameSyntax>().First();

                //var list = new List<IdentifierNameSyntax>();

                //foreach (var castedIdentifier in ifStatement.Condition.DescendantNodes().OfType<IdentifierNameSyntax>())
                //{
                    var foundCastedVariables = ifStatement.Statement
                        .DescendantNodes()
                        .Where(expression => expression.IsKind(SyntaxKind.CastExpression))
                        .SelectMany(expression => expression
                                                    .DescendantNodes()
                                                    .OfType<IdentifierNameSyntax>()
                                                    .Where(nameSyntax => nameSyntax.Identifier.ValueText.Equals(castedIdentifier.Identifier.ValueText)));

                    var variableFound = foundCastedVariables.FirstOrDefault();
               
                    if (null != variableFound)
                    {
                        return (variableFound, ifStatement, (PredefinedTypeSyntax)((BinaryExpressionSyntax)ifStatement.Condition).Right);
                    }
                //}
            }

            return (null, null, null);
        }
    }
}
