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
    
    public partial class StatusRealizedPlatform
    {
        public StatusRealizedPlatform()
        {
            this.RealizedPlatforms = new HashSet<RealizedPlatform>();
        }
    
        public int Id { get; set; }
        public string Title { get; set; }
    
        public virtual ICollection<RealizedPlatform> RealizedPlatforms { get; set; }
    }
}
