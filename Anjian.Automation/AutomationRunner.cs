using System;
using System.Collections.Generic;

namespace Anjian;

public sealed class AutomationRunner
{
    public void Run(IReadOnlyList<IAutomationStep> steps, AutomationContext context)
    {
        ArgumentNullException.ThrowIfNull(steps);
        ArgumentNullException.ThrowIfNull(context);

        for (var i = 0; i < steps.Count; i++)
        {
            var step = steps[i] ?? throw new InvalidOperationException($"步骤列表中第 {i + 1} 项为空。");
            context.ExecutionController.BeforeStep(step.Name, i);

            try
            {
                if (step is WaitStep waitStep)
                {
                    context.ExecutionController.WaitForContinue(
                        context.ExecutionController.Mode == ExecutionMode.StepByStep
                            ? $"准备执行步骤 {i + 1}：{step.Name}"
                            : waitStep.Reason);
                    continue;
                }

                if (context.ExecutionController.Mode == ExecutionMode.StepByStep)
                {
                    context.ExecutionController.WaitForContinue($"准备执行步骤 {i + 1}：{step.Name}");
                }

                step.Execute(context);
            }
            catch (AutomationStepExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AutomationStepExecutionException(i, step.Name, ex);
            }
        }
    }
}
