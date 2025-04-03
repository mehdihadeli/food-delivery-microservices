using Tests.Shared.XunitFramework;
using Xunit;

[assembly: TestFramework(
    $"{nameof(Tests)}.{nameof(Tests.Shared)}.{nameof(Tests.Shared.XunitFramework)}.{nameof(CustomTestFramework)}",
    $"{nameof(Tests)}.{nameof(Tests.Shared)}"
)]
