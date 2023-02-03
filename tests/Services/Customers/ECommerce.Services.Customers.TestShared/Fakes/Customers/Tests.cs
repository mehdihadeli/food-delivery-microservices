using ECommerce.Services.Customers.TestShared.Fakes.Customers.Entities;
using ECommerce.Services.Customers.TestShared.Fakes.Customers.Events;
using FluentAssertions;
using Tests.Shared.XunitCategories;

namespace ECommerce.Services.Customers.TestShared.Fakes.Customers;

public class Tests
{
    public class Events
    {
        [Fact]
        [CategoryTrait(TestCategory.Unit)]
        public void auto_fake_user_registered_v1_test()
        {
            var userRegistered = new AutoFakeUserRegisteredV1().Generate(1).First();
            userRegistered.IdentityId.Should().NotBeEmpty();
            userRegistered.UserName.Should().NotBeEmpty();
        }

        [Fact]
        [CategoryTrait(TestCategory.Unit)]
        public void fake_user_registered_v1_test()
        {
            var userRegistered = new FakeUserRegisteredV1().Generate(1).First();
            userRegistered.IdentityId.Should().NotBeEmpty();
            userRegistered.UserName.Should().NotBeEmpty();
        }
    }

    public class Dtos
    {
    }

    public class Entities
    {
        [Fact]
        [CategoryTrait(TestCategory.Unit)]
        public void fake_customer_test()
        {
            var customers = new FakeCustomer().Generate(5);
            customers.All(x => x.Id > 0).Should().BeTrue();
            customers.All(x => x.IdentityId != Guid.Empty).Should().BeTrue();
            customers.All(x => !string.IsNullOrWhiteSpace(x.Email)).Should().BeTrue();
            customers.All(x => !string.IsNullOrWhiteSpace(x.PhoneNumber)).Should().BeTrue();
            customers.All(x => !string.IsNullOrWhiteSpace(x.Name.FirstName)).Should().BeTrue();
            customers.All(x => !string.IsNullOrWhiteSpace(x.Name.LastName)).Should().BeTrue();
            customers.All(x => !string.IsNullOrWhiteSpace(x.Name.FullName)).Should().BeTrue();
        }

        [Fact]
        [CategoryTrait(TestCategory.Unit)]
        public void fake_customer_read_model_test()
        {
            var customers = new FakeCustomerReadModel().Generate(5);
            customers.All(x => x.Id != Guid.Empty).Should().BeTrue();
            customers.All(x => x.IdentityId != Guid.Empty).Should().BeTrue();
            customers.All(x => !string.IsNullOrWhiteSpace(x.Email)).Should().BeTrue();
            customers.All(x => !string.IsNullOrWhiteSpace(x.PhoneNumber)).Should().BeTrue();
            customers.All(x => !string.IsNullOrWhiteSpace(x.FirstName)).Should().BeTrue();
            customers.All(x => !string.IsNullOrWhiteSpace(x.LastName)).Should().BeTrue();
            customers.All(x => !string.IsNullOrWhiteSpace(x.FullName)).Should().BeTrue();
            customers.All(x => !string.IsNullOrWhiteSpace(x.City)).Should().BeTrue();
            customers.All(x => !string.IsNullOrWhiteSpace(x.Country)).Should().BeTrue();
            customers.All(x => !string.IsNullOrWhiteSpace(x.DetailAddress)).Should().BeTrue();
            customers.All(x => x.BirthDate is { } && x.BirthDate.Value != default).Should().BeTrue();
        }
    }
}
