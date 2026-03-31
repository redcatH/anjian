namespace Anjian;

using System.Threading;

public enum ContinueDecision
{
    Continue,
    SkipCurrentRun
}

public interface IGlobalContinueSignal
{
    ContinueDecision WaitForContinueDecision(string reason, CancellationToken cancellationToken = default);
}
