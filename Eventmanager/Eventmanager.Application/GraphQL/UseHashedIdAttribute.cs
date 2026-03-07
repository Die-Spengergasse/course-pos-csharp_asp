using Eventmanager.Application.Model;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;
using IdHasher;
using System.Reflection;

namespace Eventmanager.Application.GraphQL;

/// <summary>
/// Converter to translate the Entity.Id property into an encoded ID value using the ID converter.
/// </summary>
public class UseHashedIdAttribute : ObjectFieldDescriptorAttribute
{
    protected override void OnConfigure(
        IDescriptorContext context, IObjectFieldDescriptor descriptor, MemberInfo member)
    {
        descriptor
            .Type<NonNullType<StringType>>()  // int to string
            .Resolve(ctx =>
            {
                var entity = ctx.Parent<Entity>();
                return Id.From(entity.Id).EncodedValue;
            });
    }
}