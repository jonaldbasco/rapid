using rapid.core.app.Models;

namespace rapid.core.app.Agents
{
    public interface IAgent
    {
        string Name { get; }
        string Execute(string task, AgentContext context);
    }
}
