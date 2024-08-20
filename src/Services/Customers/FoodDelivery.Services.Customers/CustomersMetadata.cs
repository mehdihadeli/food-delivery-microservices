using Riok.Mapperly.Abstractions;

// https://mapperly.riok.app/docs/configuration/mapper/#default-mapper-configuration
[assembly: MapperDefaults(EnumMappingIgnoreCase = true, EnumMappingStrategy = EnumMappingStrategy.ByName)]

namespace FoodDelivery.Services.Customers;

public class CustomersMetadata;
