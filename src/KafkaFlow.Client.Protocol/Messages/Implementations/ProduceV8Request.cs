namespace KafkaFlow.Client.Protocol.Messages.Implementations
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;

    internal class ProduceV8Request : IProduceRequest
    {
        public ProduceV8Request(ProduceAcks acks, int timeout)
        {
            this.Acks = acks;
            this.Timeout = timeout;
        }

        public ApiKey ApiKey => ApiKey.Produce;

        public short ApiVersion => 8;

        public Type ResponseType => typeof(ProduceV8Response);

        public string? TransactionalId { get; } = null;

        public ProduceAcks Acks { get; }

        public int Timeout { get; }

        public ConcurrentDictionary<string, IProduceRequest.ITopic> Topics { get; } =
            new ConcurrentDictionary<string, IProduceRequest.ITopic>();

        public IProduceRequest.ITopic CreateTopic(string name) => new Topic(name);

        public void Write(Stream destination)
        {
            destination.WriteString(this.TransactionalId);
            destination.WriteInt16((short) this.Acks);
            destination.WriteInt32(this.Timeout);
            destination.WriteArray(this.Topics.Values);
        }

        public class Topic : IProduceRequest.ITopic
        {
            public Topic(string name)
            {
                this.Name = name;
            }

            public string Name { get; }

            public ConcurrentDictionary<int, IProduceRequest.IPartition> Partitions { get; } =
                new ConcurrentDictionary<int, IProduceRequest.IPartition>();

            public IProduceRequest.IPartition CreatePartition(int id) => new Partition(id);

            public void Write(Stream destination)
            {
                destination.WriteString(this.Name);
                destination.WriteArray(this.Partitions.Values);
            }
        }

        public class Partition : IProduceRequest.IPartition
        {
            public Partition(int id)
            {
                this.Id = id;
            }

            public int Id { get; }

            public RecordBatch RecordBatch { get; set; } = new RecordBatch();

            public void Write(Stream destination)
            {
                destination.WriteInt32(this.Id);
                destination.WriteMessage(this.RecordBatch);
            }
        }
    }
}
