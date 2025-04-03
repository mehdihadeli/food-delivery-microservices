using FluentAssertions;
using FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1.Events.Domain;
using FoodDelivery.Services.Customers.Customers.Features.UpdatingCustomer.v1.Events.Domain;
using FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Models;
using Tests.Shared.XunitCategories;

namespace FoodDelivery.Services.Customers.UnitTests.Customers.Models.Customer;

//https://www.testwithspring.com/lesson/the-best-practices-of-nested-unit-tests/
//https://jeremydmiller.com/2022/10/24/using-context-specification-to-better-express-complicated-tests/
//{do_something}_{given_some_condition}

public class CustomerReadModelTests
{
    public class CreateCustomer
    {
        [Fact]
        [CategoryTrait(TestCategory.Unit)]
        public void can_create_customer_with_valid_inputs()
        {
            // Arrange
            var customerInput = new FakeCustomer().Generate();

            // Act
            var createdCustomer = Services.Customers.Customers.Models.Customer.Create(
                customerInput.Id,
                customerInput.Email,
                customerInput.PhoneNumber,
                customerInput.Name,
                customerInput.IdentityId,
                customerInput.Address,
                customerInput.BirthDate,
                customerInput.Nationality
            );

            // Assert
            createdCustomer.IdentityId.Should().Be(customerInput.IdentityId);
            createdCustomer.Id.Should().BeEquivalentTo(customerInput.Id);
            createdCustomer.Name.Should().BeEquivalentTo(customerInput.Name);
            createdCustomer.PhoneNumber.Should().BeEquivalentTo(customerInput.PhoneNumber);
            createdCustomer.Address.Should().BeEquivalentTo(customerInput.Address);
            createdCustomer.Nationality.Should().BeEquivalentTo(customerInput.Nationality);
        }

        [Fact]
        [CategoryTrait(TestCategory.Unit)]
        public void queue_domain_events_on_create()
        {
            // Arrange
            var customerInput = new FakeCustomer().Generate();

            // Act
            var createdCustomer = Services.Customers.Customers.Models.Customer.Create(
                customerInput.Id,
                customerInput.Email,
                customerInput.PhoneNumber,
                customerInput.Name,
                customerInput.IdentityId
            );

            // Assert
            createdCustomer.GetUncommittedDomainEvents().Count.Should().Be(1);
            customerInput.GetUncommittedDomainEvents().FirstOrDefault().Should().BeOfType<CustomerCreated>();
        }
    }

    public class UpdateCustomer
    {
        [Fact]
        [CategoryTrait(TestCategory.Unit)]
        public void can_update_customer_with_valid_inputs()
        {
            // Arrange
            var fakeCustomer = new FakeCustomer().Generate();
            var fakeUpdateCustomer = new FakeCustomer(fakeCustomer.Id, fakeCustomer.IdentityId).Generate();

            // Act
            fakeCustomer.Update(
                fakeUpdateCustomer.Email,
                fakeUpdateCustomer.PhoneNumber,
                fakeUpdateCustomer.Name,
                fakeUpdateCustomer.Address,
                fakeUpdateCustomer.BirthDate,
                fakeUpdateCustomer.Nationality
            );

            // Assert
            fakeCustomer.Nationality.Should().BeEquivalentTo(fakeUpdateCustomer.Nationality);
            fakeCustomer.Email.Should().BeEquivalentTo(fakeUpdateCustomer.Email);
            fakeCustomer.BirthDate.Should().BeEquivalentTo(fakeUpdateCustomer.BirthDate);
            fakeCustomer.Name.Should().BeEquivalentTo(fakeUpdateCustomer.Name);
        }

        [Fact]
        [CategoryTrait(TestCategory.Unit)]
        public void queue_domain_event_on_update()
        {
            // Arrange
            var fakeCustomer = new FakeCustomer().Generate();
            var fakeUpdateCustomer = new FakeCustomer(fakeCustomer.Id, fakeCustomer.IdentityId).Generate();

            fakeCustomer.ClearDomainEvents();
            fakeUpdateCustomer.ClearDomainEvents();

            // Act
            fakeCustomer.Update(
                fakeUpdateCustomer.Email,
                fakeUpdateCustomer.PhoneNumber,
                fakeUpdateCustomer.Name,
                fakeUpdateCustomer.Address,
                fakeUpdateCustomer.BirthDate,
                fakeUpdateCustomer.Nationality
            );

            // Assert
            fakeCustomer.GetUncommittedDomainEvents().Count.Should().Be(1);
            fakeCustomer.GetUncommittedDomainEvents().FirstOrDefault().Should().BeOfType(typeof(CustomerUpdated));
        }
    }
}
