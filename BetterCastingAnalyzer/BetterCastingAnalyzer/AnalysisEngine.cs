﻿using System;
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
        public static (IfStatementSyntax ifStatement, PredefinedTypeSyntax predefinedType, List<IdentifierNameSyntax> identifiers) FindIdentifierBeingCastedTwice(SyntaxNode node)
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
                                                    .Where(nameSyntax => nameSyntax.Identifier.ValueText.Equals(castedIdentifier.Identifier.ValueText)))
                        .ToList();

                //var variableFound = foundCastedVariables.FirstOrDefault();

                if (0 < foundCastedVariables.Count)
                    {
                        return (ifStatement, (PredefinedTypeSyntax)((BinaryExpressionSyntax)ifStatement.Condition).Right, foundCastedVariables);
                    }
                //}
            }

            return (null, null, null);
        }
    }
}
