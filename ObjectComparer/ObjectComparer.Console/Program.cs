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

            ObjectTracker objectTracker = new ObjectTracker();

            List<Difference> differences =  objectTracker
                                                .Track(before, after, x => x.Gender)
                                                .TrackCollection(before.PersonNames, after.PersonNames, keyExpression: x => x.Id, x => x.Name)
                                                .GetDifferences();

            foreach (Difference difference in differences)
            {
                System.Console.WriteLine(difference);
            }
        }
    }
}
