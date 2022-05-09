using AutoMapper;
using Store.Services.Customers.RestockSubscriptions.Dtos;
using Store.Services.Customers.RestockSubscriptions.Features;
using Store.Services.Customers.RestockSubscriptions.Models.Read;
using Store.Services.Customers.RestockSubscriptions.Models.Write;

namespace Store.Services.Customers.RestockSubscriptions;

public class RestockSubscriptionsMapping : Profile
{
    public RestockSubscriptionsMapping()
    {
        CreateMap<RestockSubscription, RestockSubscriptionDto>()
            .ForMember(x => x.Id, opt => opt.MapFrom(x => x.Id.Value))
            .ForMember(x => x.Email, opt => opt.MapFrom(x => x.Email.Value))
            .ForMember(x => x.ProductName, opt => opt.MapFrom(x => x.ProductInformation.Name))
            .ForMember(x => x.ProductId, opt => opt.MapFrom(x => x.ProductInformation.Id.Value))
            .ForMember(x => x.CustomerId, opt => opt.MapFrom(x => x.CustomerId.Value));

        CreateMap<RestockSubscription, RestockSubscriptionReadModel>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.RestockSubscriptionId, opt => opt.MapFrom(x => x.Id.Value))
            .ForMember(x => x.Email, opt => opt.MapFrom(x => x.Email.Value))
            .ForMember(x => x.ProductName, opt => opt.MapFrom(x => x.ProductInformation.Name))
            .ForMember(x => x.ProductId, opt => opt.MapFrom(x => x.ProductInformation.Id.Value))
            .ForMember(x => x.CustomerId, opt => opt.MapFrom(x => x.CustomerId.Value));

        CreateMap<RestockSubscriptionReadModel, RestockSubscriptionDto>()
            .ForMember(x => x.Id, opt => opt.MapFrom(x => x.RestockSubscriptionId))
            .ForMember(x => x.Email, opt => opt.MapFrom(x => x.Email))
            .ForMember(x => x.ProductName, opt => opt.MapFrom(x => x.ProductName))
            .ForMember(x => x.ProductId, opt => opt.MapFrom(x => x.ProductId))
            .ForMember(x => x.CustomerId, opt => opt.MapFrom(x => x.CustomerId));

        CreateMap<CreateMongoRestockSubscriptionReadModels, RestockSubscriptionReadModel>()
            .ForMember(x => x.RestockSubscriptionId, opt => opt.MapFrom(x => x.RestockSubscriptionId))
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.IsDeleted, opt => opt.MapFrom(x => x.IsDeleted));

        CreateMap<UpdateMongoRestockSubscriptionReadModel, RestockSubscriptionReadModel>()
            .ForMember(x => x.RestockSubscriptionId, opt => opt.MapFrom(x => x.RestockSubscription.Id.Value))
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.Created, opt => opt.MapFrom(x => x.RestockSubscription.Created))
            .ForMember(x => x.Email, opt => opt.MapFrom(x => x.RestockSubscription.Email.Value))
            .ForMember(x => x.Processed, opt => opt.MapFrom(x => x.RestockSubscription.Processed))
            .ForMember(x => x.CustomerId, opt => opt.MapFrom(x => x.RestockSubscription.CustomerId.Value))
            .ForMember(x => x.CustomerName, opt => opt.Ignore())
            .ForMember(x => x.ProcessedTime, opt => opt.MapFrom(x => x.RestockSubscription.ProcessedTime))
            .ForMember(x => x.ProductId, opt => opt.MapFrom(x => x.RestockSubscription.ProductInformation.Id.Value))
            .ForMember(x => x.ProductName, opt => opt.MapFrom(x => x.RestockSubscription.ProductInformation.Name))
            .ForMember(x => x.IsDeleted, opt => opt.MapFrom(x => x.IsDeleted));
    }
}
