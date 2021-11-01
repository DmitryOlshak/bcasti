using System;

namespace Bcasti.CodeAnalysis.Binding
{
    internal sealed class BoundVariableExpression : BoundExpression
    {
        public string Name { get; }
        public override BoundNodeKind Kind => BoundNodeKind.VariableExpression;
        public override Type Type { get; }
        
        public BoundVariableExpression(string name, Type type)
        {
            Name = name;
            Type = type;
        }
    }
}