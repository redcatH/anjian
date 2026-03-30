using System;

namespace Anjian;

public sealed class SkipCurrentRunException : Exception
{
    public SkipCurrentRunException(string message)
        : base(message)
    {
    }
}
