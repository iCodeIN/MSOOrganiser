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
    
    public partial class Entrant
    {
        public Nullable<int> Year { get; set; }
        public int EntryNumber { get; set; }
        public Nullable<int> Mind_Sport_ID { get; set; }
        public string Game_Code { get; set; }
        public Nullable<bool> Receipt { get; set; }
        public string Partner { get; set; }
        public decimal Fee { get; set; }
        public string Comment { get; set; }
        public Nullable<int> Rank { get; set; }
        public string Score { get; set; }
        public string Tie_break { get; set; }
        public Nullable<bool> Absent { get; set; }
        public string Medal { get; set; }
        public Nullable<float> Penta_Score { get; set; }
        public Nullable<bool> MustUse { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public Nullable<int> PIN { get; set; }
        public byte[] SSMA_TimeStamp { get; set; }
        public Nullable<int> OlympiadId { get; set; }
    
        public virtual Contestant Name { get; set; }
    }
}
