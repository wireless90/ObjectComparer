using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace ObjectComparer
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TypeOfDifference {
        [EnumMember(Value = nameof(Add))]
        Add,
        [EnumMember(Value = nameof(Delete))]
        Delete,
        [EnumMember(Value = nameof(Amend))]
        Amend 
    };

    public class Difference
    {
        public string PropertyName { get; set; }

        public string OldValue { get; set; }

        public string NewValue { get; set; }

        public TypeOfDifference Type { get; set; }

        public override string ToString() => JsonConvert.SerializeObject(this);
    }
}
