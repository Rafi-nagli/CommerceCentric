using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.DTO.Contracts;
using Amazon.DTO.Users;

namespace Amazon.DTO
{
    public class UserDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public int RoleId { get; set; }

        public long CompanyId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public bool IsApproved { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public DateTime? LastLockoutDate { get; set; }
        public DateTime? LastLoginDate { get; set; }

        public int PasswordFailuresSinceLastSuccess { get; set; }
        public DateTime? LastPasswordFailureDate { get; set; }
        public DateTime? LastPasswordChangedDate { get; set; }


        public bool? IsAcceptedTerms { get; set; }
        public string AcceptTermsDetails { get; set; }


        public DateTime? CreateDate { get; set; }


        public bool Deleted { get; set; }
        public DateTime? DeleteDate { get; set; }


        //Navigation
        public RoleDTO Role { get; set; }
        public CompanyDTO Company { get; set; }
    }
}
