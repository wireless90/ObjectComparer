using System;
using System.Collections.Generic;

namespace ObjectComparer.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Person> people = PersonFactory.GetPersons();

            ObjectTracker objectTracker = new ObjectTracker();

            var differences = objectTracker
                                .Track(people[0].PersonNames, people[1].PersonNames, keyExpression: x => x.Id, x => x.Name, x => x.Type)
                                .GetDifferences();

            foreach (var difference in differences)
            {
                System.Console.WriteLine(difference);
            }
        }
    }
}
