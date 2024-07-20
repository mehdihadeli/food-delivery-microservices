using Tests.Shared.XunitFramework;

[assembly: TestFramework(
    $"{nameof(Tests)}.{nameof(Tests.Shared)}.{nameof(Tests.Shared.XunitFramework)}.{nameof(CustomTestFramework)}",
    $"{nameof(Tests)}.{nameof(Tests.Shared)}"
)]
