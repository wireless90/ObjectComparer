using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectComparer.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Person> people = PersonFactory.GetPersons();

            Person before = people.First();
            Person after = people.Last();

            ObjectTracker<AmendmentHistory> objectTracker = new ObjectTracker<AmendmentHistory>();

            List<AmendmentHistory> differences =  objectTracker
                                                    .SetCallback(CustomFunction)
                                                    .Track(before, after, x => x.Gender)
                                                    .TrackCollection(before.PersonNames, after.PersonNames, keyExpression: x => x.Id, x => x.Name)
                                                    .GetDifferences();

            //foreach (Difference difference in differences)
            //{
            //    System.Console.WriteLine(difference);
            //}
        }
        private static List<AmendmentHistory> CustomFunction(List<Difference> differences)
    {
        List<AmendmentHistory> amendmentHistories = new List<AmendmentHistory>();

        foreach (var difference in differences)
        {
            AmendmentHistory amendmentHistory = new AmendmentHistory()
            {
                Id = new Guid().ToString(),
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


    public class AmendmentHistory
    {
        public string Id { get; set; }

        public string Status { get; set; }

        public string OldValue { get; set; }
        public string NewValue { get; set; }

        public string PropertyName { get; set; }

        public string Requester { get; set; }
    }
}
