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
        public static (IdentifierNameSyntax identifier, IfStatementSyntax ifStatement) FindIdentifierBeingCastedTwice(SyntaxNode node)
        {
            if (node is IfStatementSyntax ifStatement && ifStatement.Condition.IsKind(SyntaxKind.IsExpression))
            {
                var castedIdentifier = ifStatement.Condition.DescendantNodes().OfType<IdentifierNameSyntax>().First();

                if (ifStatement.Statement.DescendantNodes().Any(expression => expression.IsKind(SyntaxKind.CastExpression) && expression.DescendantNodes().OfType<IdentifierNameSyntax>().Any(nameSyntax => nameSyntax.Identifier.ValueText.Equals(castedIdentifier.Identifier.ValueText))))
                {
                    return (castedIdentifier, ifStatement);
                }
            }

            return (null, null);
        }
    }
}
