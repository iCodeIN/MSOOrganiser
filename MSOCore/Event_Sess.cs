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
    
    public partial class Event_Sess
    {
        public int INDEX { get; set; }
        public Nullable<int> EIN { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public string Session { get; set; }
    
        public virtual Event Event { get; set; }
        public virtual Session Session1 { get; set; }
    }
}