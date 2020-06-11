using System;
using System.Text.Json;
using AuthJanitor.Integrations.IdentityServices.AzureActiveDirectory;
using Xunit;

namespace AuthJanitor.Tests
{
    public class JsonConverterTests
    {

        [Fact]
        public void JsonSerializerShouldReturnStringForNumbers()
        {
            string expected = "{\"MemberInt\":\"5\",\"MemberLong\":\"1\"}";

            var writeAsString = true;
            string tokenJson = JsonSerializer.Serialize(SampleDTO(), GetSerializerOptions(writeAsString));

            Assert.Equal(expected, tokenJson);
        }

        [Fact]
        public void JsonSerializerShouldReturnNumbersForNumbers()
        {
            string expected = "{\"MemberInt\":5,\"MemberLong\":1}";

            var writeAsString = false;
            string tokenJson = JsonSerializer.Serialize(SampleDTO(), GetSerializerOptions(writeAsString));

            Assert.Equal(expected, tokenJson);
        }

        private JsonSerializerOptions GetSerializerOptions(bool writeAsString)
        {
            return new JsonSerializerOptions()
            {
                Converters = { new StringToIntJsonConverter(writeAsString), new StringToLongJsonConverter(writeAsString) }
            };
        }

        private TestDTO SampleDTO()
        {
            return new TestDTO()
            {
                MemberInt = 5,
                MemberLong = 1L
            };
        }

    }

    public class TestDTO
    {
        public int MemberInt { get; set; }
        public long MemberLong { get; set; }
    }

}
