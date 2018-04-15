using System;
using System.Linq;
using System.Text;
using static System.Math;

namespace AsciiTree
{

    // Generates an Ascii tree with labels that is displayed in the console
    internal class TreeGenerator
    {
        private const int MaxHeight = 1000;
        private int[] _leftProfile;
        private int[] _rightProfile;
        private readonly StringBuilder _builder = new StringBuilder();

        private const int Gap = 5;
        private const string Indent = "\t";

        private int _printNext;


        private void ComputeLeftProfile(TextNode labelNode, int x, int y)
        {
            if (labelNode == null)
                return;
            int inLeft = (labelNode.BranchDir == Branch.Left) ? 1 : 0;

            _leftProfile[y] = Min(_leftProfile[y], x - ((labelNode.LabelLength - inLeft) / 2));
            if (labelNode.LeftNode != null)
            {
                for (int i = 1; i <= labelNode.EdgeLength && y + i < MaxHeight; i++)
                {
                    _leftProfile[y + i] = Min(_leftProfile[y + i], x - i);
                }
            }
            ComputeLeftProfile(labelNode.LeftNode, x - labelNode.EdgeLength - 1, y + labelNode.EdgeLength + 1);
            ComputeLeftProfile(labelNode.RightNode, x + labelNode.EdgeLength + 1, y + labelNode.EdgeLength + 1);
        }

        private void ComputeRightProfile(TextNode labelNode, int x, int y)
        {
            if (labelNode == null)
                return;

            int notLeft = (labelNode.BranchDir != Branch.Left) ? 1 : 0;

            _rightProfile[y] = Max(_rightProfile[y], x + ((labelNode.LabelLength - notLeft) / 2));
            if (labelNode.RightNode != null)
            {
                for (int i = 1; i <= labelNode.EdgeLength && y + i < MaxHeight; i++)
                {
                    _rightProfile[y + i] = Max(_rightProfile[y + i], x + i);
                }
            }
            ComputeRightProfile(labelNode.LeftNode, x - labelNode.EdgeLength - 1, y + labelNode.EdgeLength + 1);
            ComputeRightProfile(labelNode.RightNode, x + labelNode.EdgeLength + 1, y + labelNode.EdgeLength + 1);
        }

        private void ComputeEdgeLengths(TextNode labelNode)
        {
            int delta = 4;

            if (labelNode == null)
                return;
            ComputeEdgeLengths(labelNode.LeftNode);
            ComputeEdgeLengths(labelNode.RightNode);

            if (labelNode.RightNode == null && labelNode.LeftNode == null)
            {
                labelNode.EdgeLength = 0;
            }
            else
            {
                int minHeight;
                int i;
                if (labelNode.LeftNode != null)
                {
                    _rightProfile = Enumerable.Repeat(int.MinValue, labelNode.LeftNode.Height).ToArray();
                    ComputeRightProfile(labelNode.LeftNode, 0, 0);
                    minHeight = labelNode.LeftNode.Height;
                }
                else
                {
                    minHeight = 0;
                }
                if (labelNode.RightNode != null)
                {
                    _leftProfile = Enumerable.Repeat(int.MaxValue, labelNode.RightNode.Height).ToArray();
                    ComputeLeftProfile(labelNode.RightNode, 0, 0);
                    minHeight = Min(labelNode.RightNode.Height, minHeight);
                }
                else
                {
                    minHeight = 0;
                }
                for (i = 0; i < minHeight; i++)
                {
                    delta = Max(delta, Gap + 1 + _rightProfile[i] - _leftProfile[i]);
                }

                if (((labelNode.LeftNode != null && labelNode.LeftNode.Height == 1) || (labelNode.RightNode != null && labelNode.RightNode.Height == 1)) &&
                    delta > 4)
                {
                    delta--;
                }
                labelNode.EdgeLength = ((delta + 1) / 2) - 1;
            }

            int height = 1;
            if (labelNode.LeftNode != null)
            {
                height = Max(labelNode.LeftNode.Height + labelNode.EdgeLength + 1, height);
            }
            if (labelNode.RightNode != null)
            {
                height = Max(labelNode.RightNode.Height + labelNode.EdgeLength + 1, height);
            }
            labelNode.Height = height;
        }
        
        private void PrintLevel(TextNode labelNode, int x, int level, int n = 0)
        {
            int i;
            if (labelNode == null)
                return;
            int isleft = (labelNode.BranchDir == Branch.Left) ? 1 : 0;

            if (level == 0)
            {
                for (i = 0; i < (x - _printNext - ((labelNode.LabelLength - isleft) / 2)) + n; i++)
                {
                    _builder.Append(' ');
                }

                _printNext += i;
                _builder.Append(labelNode.Label);
                _printNext += labelNode.LabelLength;
            }
            else if (labelNode.EdgeLength >= level)
            {
                if (labelNode.Branches == Branch.Left)
                {
                    for (i = 0; i < (x - _printNext) + n; i++)
                    {
                        _builder.Append(' ');
                    }
                    _printNext += i;
                    _builder.Append('|');
                    _printNext++;
                }
                else
                {
                    if (labelNode.LeftNode != null)
                    {
                        for (i = 0; i < (x - _printNext - (level)) + n; i++)
                        {
                            _builder.Append(' ');
                        }

                        _printNext += i;
                        _builder.Append('/');
                        _printNext++;
                    }

                    if (labelNode.RightNode != null)
                    {
                        for (i = 0; i < (x - _printNext + (level)) + n; i++)
                        {
                            _builder.Append(' ');
                        }

                        _printNext += i;
                        _builder.Append('\\');
                        _printNext++;
                    }
                }
            }
            else
            {
                if (labelNode.Branches == Branch.Left)
                    n += 2;
                PrintLevel(labelNode.LeftNode, x - labelNode.EdgeLength - 1, level - labelNode.EdgeLength - 1, n);
                PrintLevel(labelNode.RightNode, x + labelNode.EdgeLength + 1, level - labelNode.EdgeLength - 1, n);
            }
        }

        public string CreateTree(TextNode proot)
        {
            if (proot == null) return string.Empty;
            int xmin = 0;
            ComputeEdgeLengths(proot);
            _leftProfile = Enumerable.Repeat(int.MaxValue, proot.Height).ToArray();
         
            ComputeLeftProfile(proot, 0, 0);

            xmin = _leftProfile.Min();
            for (int i = 0; i < proot.Height; i++)
            {
                _printNext = 0;
                _builder.Append(Indent);
                
                PrintLevel(proot, -xmin, i);
                _builder.Append(Environment.NewLine);
            }
            if (proot.Height >= MaxHeight)
            {
                throw new ArgumentOutOfRangeException(nameof(MaxHeight), "Tree has outgrown its maximum height and may be incorrectly drawn. Try increasing the MaxHeight constant.");
            }
            return _builder.ToString();
        }

    }
    public class TextNode // -(-5 + -4)
    {
        public TextNode(string lbl, TextNode left = null, TextNode right = null)
        {
            Label = lbl;
            LabelLength = lbl.Length;
            if (left != null)
            {
                LeftNode = left;
                Branches |= left.BranchDir = Branch.Left;
            }
            if (right != null)
            {
                RightNode = right;
                Branches |= right.BranchDir = Branch.Right;
            }
        }

        public string Label { get; }
        public int EdgeLength { get; set; }
        public int Height { get; set; }
        public int LabelLength { get; }
        public Branch BranchDir { get; private set; }
        public TextNode LeftNode { get;}
        public TextNode RightNode { get;}
        public Branch Branches { get; }
    }
    
    [Flags]
    public enum Branch { None, Left, Right, All }
}
