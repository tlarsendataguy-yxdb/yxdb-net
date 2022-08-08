using System.Runtime.CompilerServices;
using Xunit;
using yxdb;

[assembly: InternalsVisibleTo("yxdb_test")]
namespace yxdb_test
{
    public class ExtractorsTest
    {
        [Fact]
        public void ExtractInt16()
        {
            var extract = Extractors.NewInt16Extractor(2);
            var result = extract(new byte[] { 0, 0, 10, 0, 0, 0 });
            
            Assert.Equal(10, result);
        }
    }
}