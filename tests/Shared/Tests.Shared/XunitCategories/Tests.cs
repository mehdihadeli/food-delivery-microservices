namespace Tests.Shared.XunitCategories;

//https://www.jetbrains.com/help/rider/Test_Categories.html

public class Tests
{
    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    public void category_trait_test()
    {
    }

    [Fact]
    [FeatureTrait(115)]
    public void feature_trait_test()
    {
    }

    [Fact]
    [BugTrait(115)]
    public void bug_trait_test()
    {
    }
}
