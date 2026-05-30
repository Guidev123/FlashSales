using System.Reflection;

namespace Modules.Catalog.Domain
{
    public static class AssemblyReference
    {
        public static readonly Assembly Assembly = Assembly.GetExecutingAssembly();
    }
}
