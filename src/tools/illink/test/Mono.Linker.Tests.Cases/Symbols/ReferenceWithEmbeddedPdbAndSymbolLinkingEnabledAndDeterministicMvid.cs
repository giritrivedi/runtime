using Mono.Linker.Tests.Cases.Expectations.Assertions;
using Mono.Linker.Tests.Cases.Expectations.Metadata;
using Mono.Linker.Tests.Cases.Symbols.Dependencies;

namespace Mono.Linker.Tests.Cases.Symbols
{
    [SetupCompileBefore("LibraryWithEmbeddedPdbSymbols.dll", new[] { "Dependencies/LibraryWithEmbeddedPdbSymbols.cs" }, additionalArguments: new[] { "/debug:embedded" }, compilerToUse: "csc")]
    [SetupLinkerLinkSymbols("true")]
    [SetupLinkerArgument("--deterministic", "true")]

    [KeptSymbols("LibraryWithEmbeddedPdbSymbols.dll")]

    [KeptMemberInAssembly("LibraryWithEmbeddedPdbSymbols.dll", typeof(LibraryWithEmbeddedPdbSymbols), "SomeMethod()")]
    [RemovedMemberInAssembly("LibraryWithEmbeddedPdbSymbols.dll", typeof(LibraryWithEmbeddedPdbSymbols), "NotUsed()")]
    public class ReferenceWithEmbeddedPdbAndSymbolLinkingEnabledAndDeterministicMvid
    {
        static void Main()
        {
            LibraryWithEmbeddedPdbSymbols.SomeMethod();
        }
    }
}
