using Newtonsoft.Json;

namespace ObjectComparer
{
    public enum TypeOfDifference {  Add, Delete, Amend };

    public class Difference
    {
        public string PropertyName { get; set; }

        public string OldValue { get; set; }

        public string NewValue { get; set; }

        public TypeOfDifference Type { get; set; }

        public override string ToString() => JsonConvert.SerializeObject(this);
    }
}
