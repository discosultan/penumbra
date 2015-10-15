// Polyfill for .NET 4.5 caller attributes.
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal class CallerMemberNameAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal class CallerFilePathAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal class CallerLineNumberAttribute : Attribute
    {
    }
}