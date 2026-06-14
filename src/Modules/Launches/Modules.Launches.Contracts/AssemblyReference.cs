using System.Reflection;

namespace Modules.Launches.Contracts
{
    public static class AssemblyReference
    {
        public static readonly Assembly Assembly = Assembly.GetExecutingAssembly();
    }
}
