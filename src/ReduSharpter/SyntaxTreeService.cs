using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using System.Diagnostics.Tracing;

namespace RosylinHDD
{
  internal class SyntaxTreeService
  {
    private string FilePath { get; set; }
    public SyntaxTree SyntaxTree { get; set; }

    public SyntaxTreeService(string filePath)
    {
      if (!File.Exists(filePath))
        throw new FileNotFoundException($"File not found at {filePath}");
      FilePath = filePath;
      SyntaxTree = CSharpSyntaxTree.ParseText(System.IO.File.ReadAllText(FilePath));
      if (SyntaxTree == null) throw new InvalidDataException("Error creating the Syntax Tree");
    }

    public SyntaxNode ReplaceTreeNode(SyntaxNode newNode, SyntaxNode originalNode)
    {
      SyntaxNode rootNode = getRoot(originalNode);

      SyntaxNode modifiedRootNode = rootNode.ReplaceNode(originalNode, newNode);
      return modifiedRootNode;
    }

    public SyntaxNode ReplaceTreeChildren(SyntaxNode parentNode, IEnumerable<SyntaxNode> children)
    {
      var existingChildren = parentNode.ChildNodes().ToList();
      if (existingChildren.Count == 0) return parentNode;

      var newChildren = children.Take(existingChildren.Count()).ToList();

      // Replace the existing child nodes with the new children
      return parentNode.ReplaceNodes(existingChildren, (oldChild, _) => newChildren[existingChildren.IndexOf(oldChild)]);

    }

    public void RestoreTree()
    {
      File.WriteAllText(FilePath, SyntaxTree.GetRoot().NormalizeWhitespace().ToFullString());
    }

    public SyntaxNode getRoot(SyntaxNode node)
    {
      SyntaxNode cur = node;
      while (cur.Parent != null)
      {
        cur = cur.Parent;
      }
      return cur;
    }

    public SyntaxNode findDescendantNode(SyntaxNode startingNode, SyntaxNode searchNode) {
      var normalizedStart = startingNode.NormalizeWhitespace().ToFullString();
      var normalizedSearch = searchNode.NormalizeWhitespace().ToFullString();
      if (normalizedStart == normalizedSearch) {
        return startingNode;
      }
      if (startingNode.ChildNodes().Count() == 0) return null;

         foreach (var child in startingNode.ChildNodes()) {
        var result = findDescendantNode(child, searchNode);
        if (result != null) {
            return result; // Return the found descendant node
        }
    }
    return null; // No matching descendant found
    }
  }
}
