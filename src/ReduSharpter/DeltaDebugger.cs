using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RosylinHDD
{
    internal class DeltaDebugger
    {
        public List<StatementSyntax> Minimize(List<StatementSyntax> statements, Func<List<StatementSyntax>, bool> testFunction)
        {
            return MinimizeRecursive(statements, testFunction, 2);
        }

        private List<StatementSyntax> MinimizeRecursive(List<StatementSyntax> statements, Func<List<StatementSyntax>, bool> testFunction, int n)
        {
            if (statements.Count < 2)
                return statements;

            int subsetSize = statements.Count / n;
            List<List<StatementSyntax>> subsets = new List<List<StatementSyntax>>();

            // Partition the statements into n subsets
            for (int i = 0; i < statements.Count; i += subsetSize)
            {
                subsets.Add(statements.GetRange(i, Math.Min(subsetSize, statements.Count - i)));
            }

            // Test each subset
            foreach (var subset in subsets)
            {
                if (testFunction(subset))
                {
                    // Recursive call on the failing subset
                    return MinimizeRecursive(subset, testFunction, 2);
                }
            }

            // If no subset alone causes failure, test complements
            foreach (var subset in subsets)
            {
                var complement = statements.Except(subset).ToList();
                if (testFunction(complement))
                {
                    // Recursive call on the failing complement
                    return MinimizeRecursive(complement, testFunction, Math.Max(n - 1, 2));
                }
            }

            // Increase granularity if possible
            if (n < statements.Count)
            {
                return MinimizeRecursive(statements, testFunction, Math.Min(statements.Count, n * 2));
            }

            // Cannot minimize further
            return statements;
        }
    }
}
