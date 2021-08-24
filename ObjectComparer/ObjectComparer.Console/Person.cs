using System;
using System.Collections.Generic;

namespace ObjectComparer.Console
{
    public class Person
    {
        public string Id { get; set; }

        public List<PersonName> PersonNames { get; set; }
    }

    public class PersonName
    {
        public string Id { get; set; }
        public string Type { get; set; }

        public string Name { get; set; }

        public DateTime LastModified { get; set; }
    }

    public static class PersonFactory
    {
        public static List<Person> GetPersons()
        {

            Person razali = new Person()
            {
                Id = "P1",
                PersonNames = new List<PersonName>()
                {
                    new PersonName() { Id = "N1", Name = "Razali", Type = "P", LastModified = DateTime.Now},
                    new PersonName() { Id = "N2", Name = "Lizara", Type = "A", LastModified = DateTime.Now}
                }
            };

            Person john = new Person()
            {
                Id = "P2",
                PersonNames = new List<PersonName>()
                {
                    new PersonName() { Id = "N3", Name = "John", Type = "P", LastModified = DateTime.Now},
                    new PersonName() { Id = "N4", Name = "Johnny", Type = "A", LastModified = DateTime.Now}
                }
            };


            List<Person> people = new List<Person>() { razali, john};

            return people;
        }
    }
}
