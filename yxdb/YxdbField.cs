namespace yxdb
{
    public struct YxdbField
    {
        public YxdbField(string name, DataType type)
        {
            Name = name;
            Type = type;
        }

        public string Name;
        public DataType Type;

        public enum DataType
        {
            Blob,
            Boolean,
            Byte,
            Date,
            Double,
            Long,
            String
        }
    }
}