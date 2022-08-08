using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("yxdb_test")]
namespace yxdb
{
    internal struct MetaInfoField
    {
        public MetaInfoField(string name, string type, int size, int scale)
        {
            Name = name;
            Type = type;
            Size = size;
            Scale = scale;
        }

        public string Name;
        public string Type;
        public int Size;
        public int Scale;
    }
}