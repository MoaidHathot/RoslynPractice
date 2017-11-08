using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharp6CastingAnalyzer
{
    internal static class NullPropagationAnalysisEngine
    {
        public static (IdentifierNameSyntax identifierChecked, IdentifierNameSyntax identifierUsed, Location location) FindIdentifierBeingCheckedforNull(SyntaxNode node)
        {
            if ((node as IfStatementSyntax)?.Condition is BinaryExpressionSyntax conditionExpression)
            {
                if (!conditionExpression.IsKind(SyntaxKind.LogicalAndExpression))
                {
                    return (null, null, null);
                }

                var leftIdentifierChecked = FindIdentifierCheckedForNull(conditionExpression.Left);
                var rightIdentifierChecked = FindIdentifierCheckedForNull(conditionExpression.Right);

                var rightIdentifierUsed = FindIdentifierUsedAftercheckingForNull(conditionExpression.Right, leftIdentifierChecked);
                var leftIdentifierUsed = FindIdentifierUsedAftercheckingForNull(conditionExpression.Left, rightIdentifierUsed);

                return
                    null != leftIdentifierChecked && null != rightIdentifierUsed
                        ? (leftIdentifierChecked, rightIdentifierUsed, conditionExpression.GetLocation())
                        : null != rightIdentifierChecked && null != leftIdentifierUsed
                        ? (rightIdentifierChecked, leftIdentifierUsed, conditionExpression.GetLocation())
                        : (null, null, null);

            }

            return (null, null, null);
        }

        private static IdentifierNameSyntax FindIdentifierUsedAftercheckingForNull(ExpressionSyntax expressionSyntax, IdentifierNameSyntax identifierNameSyntax)
            => expressionSyntax.DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .FirstOrDefault(identifier => identifier.Identifier.ValueText.Equals(identifierNameSyntax.Identifier.ValueText));

        private static IdentifierNameSyntax FindIdentifierCheckedForNull(ExpressionSyntax expressionSyntax)
        {
            if (expressionSyntax is BinaryExpressionSyntax expression && expression.IsKind(SyntaxKind.NotEqualsExpression))
            {
                return expression.Left is IdentifierNameSyntax && expression.Right.IsKind(SyntaxKind.NullLiteralExpression)
                    ? expression.Left as IdentifierNameSyntax
                    : expression.Right is IdentifierNameSyntax && expression.Left.IsKind(SyntaxKind.NullLiteralExpression)
                        ? expression.Right as IdentifierNameSyntax
                        : null;
            }

            return null;

        }
    }
}
