using System;

namespace Anjian;

public sealed class AutomationStepExecutionException : Exception
{
    public AutomationStepExecutionException(int stepIndex, string stepName, Exception innerException)
        : base($"步骤执行失败，索引={stepIndex + 1}，名称={stepName}，原因={innerException.Message}", innerException)
    {
        StepIndex = stepIndex;
        StepName = stepName;
    }

    public int StepIndex { get; }

    public string StepName { get; }
}
