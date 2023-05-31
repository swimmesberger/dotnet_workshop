using CqsWorkshop.Contract;
using CqsWorkshop.Domain;
using Riok.Mapperly.Abstractions;

namespace CqsWorkshop.Infrastructure.Mappings; 

[Mapper]
public static partial class CustomerMapper {
    public static partial Customer ToEntity(this CustomerForCreationDto customerForCreationDto);
    public static partial IQueryable<CustomerDto> ProjectToDto(this IQueryable<Customer> queryable);
}