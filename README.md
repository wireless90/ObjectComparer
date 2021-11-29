# ObjectComparer
Project aimed to track Additions, Deletions and Amendments made to an Object or a List of Objects.


# Basic Example
The below example shows how to retrieve the differences.

```cs
static void Main(string[] args)
{
    List<Person> people = PersonFactory.GetPersons();
  
    Person before = people.First();
    Person after = people.Last();

    ObjectTracker objectTracker = new ObjectTracker();

    List<Difference> differences = objectTracker
                                        .Track(before, after, callbackArgument: null, x => x.Gender)
                                        .TrackCollection(before.PersonNames, after.PersonNames, key => key.Id, callbackArgument:null, x => x.Name, x => x.Type)
                                        .GetDifferences();


    differences.ForEach(x => System.Console.WriteLine(x));
}
```

This produces

```json
{"PropertyName":"Person.Gender", "OldValue": "Male", "NewValue":"Female", "Type":"Amend"}
```

# Injecting CallBacks

Let's say we want to fill up our own ViewModels or we need to perform an action once we detected a change, we can do so with `Callbacks`.

The example below shows how to populateyour own viewmodel with callbacks.

**ViewModel Example**

```cs
    public class AmendmentHistory
    {
        public string Id { get; set; }

        public string ListingId { get; set; }
        public string Status { get; set; }

        public string OldValue { get; set; }
        public string NewValue { get; set; }

        public string PropertyName { get; set; }

        public string Requester { get; set; }

        public override string ToString() => JsonConvert.SerializeObject(this);
    }
```

**CallBacks**
```cs
        static void Main(string[] args)
        {
            List<Person> people = PersonFactory.GetPersons();

            Person before = people.First();
            Person after = people.Last();

            ObjectTracker<AmendmentHistory> objectTracker = new ObjectTracker<AmendmentHistory>();

            List<AmendmentHistory> differences = objectTracker
                                                .SetCallback(CustomFunction)
                                                .Track(before, after, callbackArgument: "123", x => x.Gender)
                                                .TrackCollection(before.PersonNames, after.PersonNames, key => key.Id, callbackArgument:null, x => x.Name, x => x.Type)
                                                .GetDifferences();


            differences.ForEach(x => System.Console.WriteLine(x));
        }
        private static List<AmendmentHistory> CustomFunction(List<Difference> differences, object callbackArgument)
        {
            List<AmendmentHistory> amendmentHistories = new List<AmendmentHistory>();

            foreach (var difference in differences)
            {
                AmendmentHistory amendmentHistory = new AmendmentHistory()
                {
                    Id = "1",
                    ListingId = (string)callbackArgument,
                    NewValue = difference.NewValue,
                    OldValue = difference.OldValue,
                    PropertyName = difference.PropertyName,
                    Requester = "MRAZALI",
                    Status = "Approved"
                };

                amendmentHistories.Add(amendmentHistory);
            }
            return amendmentHistories;
        }
    }
```
