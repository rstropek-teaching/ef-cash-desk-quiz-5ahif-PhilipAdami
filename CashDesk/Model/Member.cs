using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CashDesk.Model
{
    public class Member : IMember
    {

        //public Member(string firstName, string lastName, DateTime birthday)
        //{
        //    FirstName = firstName;
        //    LastName = lastName;
        //    Birthday = birthday;
        //}

        [Key]
        public int MemberNumber { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        public DateTime Birthday { get; set; }

    }
}