using System;

namespace SoupToNuts.Phonebook
{
    public class PhonebookEntry : IComparable<PhonebookEntry>
    {
        public string Name { get; set; }
        public string Number { get; set; }

        public int CompareTo(PhonebookEntry other)
        {
            if (other == null)
                return 1;

            return this.Name.CompareTo(other.Name);
        }
    }
}