namespace Bcasti.CodeAnalysis
{
    public readonly struct TextSpan
    {
        public int Start { get; }
        public int Length { get; }
        public int End { get; }

        public TextSpan(int start, int length)
        {
            Start = start;
            Length = length;
            End = Start + Length;
        }
    }
}