## yxdb-net

yxdb-net is a library for reading YXDB files into .NET applications.

Install from [NuGet](https://www.nuget.org/packages/yxdb). The package is compiled against .NET Standard 2.0.

The library has 1 depdency to Microsoft's `System.Text.Json` package and is a pure C# solution.

The public API is contained in the YxdbReader class. Instantiate YxdbReader using one of the two constructors:
* `new YxdbReader(String)` - load from a file
* `new YxdbReader(Stream)` - load from an in-memory stream

Iterate through the records in the file using the `next()` method in a while loop:

```
while (reader.Next()) {
    // do something
}
```

Fields can be access via the `readX()` methods on the YxdbReader class. There are readers for each kind of data field supported by YXDB files:
* `ReadByte()` - read Byte fields
* `ReadBlob()` - read Blob and SpatialObj fields
* `ReadBool()` - read Bool fields
* `ReadDate()` - read Date and DateTime fields
* `ReadDouble()` - read FixedDecimal, Float, and Double fields
* `ReadLong()` - read Int16, Int32, and Int64 fields
* `ReadString()` - read String, WString, V_String, and V_WString fields

Each read method has 2 overloads:
* `ReadX(int index)` - read by field index number
* `ReadX(String name)` - read by field name

If either the index number or field name is invalid, the read methods will throw an exception.

To read spatial objects, use the `yxdb.Spatial.ToGeoJson()` function. The `ToGeoJson()` function translates the binary SpatialObj format into a GeoJSON string.