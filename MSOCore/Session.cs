//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MSOCore
{
    using System;
    using System.Collections.Generic;
    
    public partial class Session
    {
        public Session()
        {
            this.Event_Sess = new HashSet<Event_Sess>();
        }
    
        public string Session1 { get; set; }
        public string Description { get; set; }
        public Nullable<float> Worth { get; set; }
        public byte[] SSMA_TimeStamp { get; set; }
        public Nullable<System.TimeSpan> StartTime { get; set; }
        public Nullable<System.TimeSpan> FinishTime { get; set; }
        public bool IsActive { get; set; }
    
        public virtual ICollection<Event_Sess> Event_Sess { get; set; }
    }
}
