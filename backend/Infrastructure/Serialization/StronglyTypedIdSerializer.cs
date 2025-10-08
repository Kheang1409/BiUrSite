using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson;
using System;

namespace Backend.Infrastructure.Serialization;
public class StronglyTypedIdSerializer<TStrongId> : SerializerBase<TStrongId>
    where TStrongId : notnull
{
    private readonly Func<Guid, TStrongId> _factory;
    private readonly Func<TStrongId, Guid> _extractor;

    public StronglyTypedIdSerializer(Func<Guid, TStrongId> factory, Func<TStrongId, Guid> extractor)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _extractor = extractor ?? throw new ArgumentNullException(nameof(extractor));
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TStrongId value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        var guidSerializer = new GuidSerializer(GuidRepresentation.Standard);
        guidSerializer.Serialize(context, args, _extractor(value));
    }

    public override TStrongId Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var guidSerializer = new GuidSerializer(GuidRepresentation.Standard);
        var guid = guidSerializer.Deserialize(context, args);
        return _factory(guid);
    }
}