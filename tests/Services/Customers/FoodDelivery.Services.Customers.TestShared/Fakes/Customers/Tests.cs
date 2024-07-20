using Bogus;
using FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Commands;
using FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Entities;
using FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Events;
using FluentAssertions;
using Tests.Shared.XunitCategories;

namespace FoodDelivery.Services.Customers.TestShared.Fakes.Customers;

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

    public class Dtos { }

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
            customers.All(x => !string.IsNullOrWhiteSpace(x.Nationality.Value)).Should().BeTrue();
            customers.All(x => x.BirthDate.Value != default).Should().BeTrue();
            customers.All(x => !string.IsNullOrWhiteSpace(x.Address.City)).Should().BeTrue();
        }

        [Fact]
        [CategoryTrait(TestCategory.Unit)]
        public void fake_customer_with_customerId_test()
        {
            var id = new Faker().Random.Number(1, 100);
            var identityId = Guid.NewGuid();

            var customer = new FakeCustomer(id, identityId).Generate();
            customer.Id.Value.Should().Be(id);
            customer.IdentityId.Should().Be(identityId);
            customer.Nationality.Should().NotBeNull();
            customer.BirthDate.Should().NotBeNull();
            customer.Address.Should().NotBeNull();
            customer.Email.Should().NotBeNull();
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

    public class Queries { }

    public class Commands
    {
        public class CreateCustomer
        {
            [Fact]
            [CategoryTrait(TestCategory.Unit)]
            public void fake_create_customer_test()
            {
                var customers = new FakeCreateCustomer().Generate(5);
                customers.All(x => x.Id > 0).Should().BeTrue();
                customers.All(x => !string.IsNullOrWhiteSpace(x.Email)).Should().BeTrue();
            }

            [Fact]
            [CategoryTrait(TestCategory.Unit)]
            public void fake_create_customer_with_email_test()
            {
                var customer = new FakeCreateCustomer("test@test.com").Generate();
                customer.Id.Should().BeGreaterThan(0);
                customer.Email.Should().NotBeEmpty();
                customer.Email.Should().Be("test@test.com");
            }
        }

        public class UpdateCustomer
        {
            [Fact]
            [CategoryTrait(TestCategory.Unit)]
            public void fake_update_customer_test()
            {
                var id = new Faker().Random.Number(1);
                var customer = new FakeUpdateCustomer(id).Generate();
                customer.Id.Should().Be(id);
                customer.Email.Should().NotBeEmpty();
                customer.FirstName.Should().NotBeEmpty();
                customer.LastName.Should().NotBeEmpty();
                customer.PhoneNumber.Should().NotBeEmpty();
            }
        }
    }
}
