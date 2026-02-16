using rapid.core.app.Models;
using rapid.core.app.Source;

namespace rapid.core.app.Agents
{
    public interface IAgent
    {
        Task<string> ExecuteAsync(AgentContext context);
        //string Name { get; }
        //string Execute(string task, AgentContext context);
    }
}
