using Generator;
using Optional;

namespace SourceGeneratorApp
{
    [UpdateBuilder]
    public class ClientUpdate
    {
        public ClientId Id { get; }

        public Option<string> Name { get; }

        public ClientUpdate(ClientId id, Option<string> name)
        {
            Id = id;
            Name = name;
        }
    }

    public class ClientId
    {
    }
}
