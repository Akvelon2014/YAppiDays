//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MobileConference.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Mentor
    {
        public Mentor()
        {
            this.Ideas = new HashSet<Idea>();
        }
    
        public int Id { get; set; }
    
        public virtual ICollection<Idea> Ideas { get; set; }
        public virtual UserProfile UserProfile { get; set; }
    }
}
