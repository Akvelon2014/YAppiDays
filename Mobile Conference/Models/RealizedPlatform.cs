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
    
    public partial class RealizedPlatform
    {
        public int Id { get; set; }
        public int IdeaId { get; set; }
        public int PlatformId { get; set; }
        public int Status { get; set; }
    
        public virtual Idea Idea { get; set; }
        public virtual Platform Platform { get; set; }
        public virtual StatusRealizedPlatform StatusRealizedPlatform { get; set; }
    }
}
