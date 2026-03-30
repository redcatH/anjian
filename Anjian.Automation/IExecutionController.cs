namespace Anjian;

public interface IExecutionController
{
    ExecutionMode Mode { get; }

    void BeforeStep(string stepName, int index);

    void WaitForContinue(string reason);
}
