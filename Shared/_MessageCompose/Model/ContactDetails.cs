using System;
using System.Collections.Generic;

namespace MessageCompose.Model
{
    [Serializable]
    public class ContactDetails
    {
        public ContactDetails(string name, string phoneNumber)
        {
            Name = name;
            PhoneNumber = phoneNumber;
        }

        public string Name { get; set; }
        public string PhoneNumber { get; set; }
    }
}
