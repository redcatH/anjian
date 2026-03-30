namespace Anjian;

public interface IAutomationStep
{
    string Name { get; }

    void Execute(AutomationContext context);
}
