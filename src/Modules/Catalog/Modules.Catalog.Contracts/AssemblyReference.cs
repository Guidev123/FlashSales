using System.Reflection;

namespace Modules.Catalog.Contracts
{
    public static class AssemblyReference
    {
        public static readonly Assembly Assembly = Assembly.GetExecutingAssembly();
    }
}
