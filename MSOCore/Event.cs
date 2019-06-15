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
    
    public partial class Event
    {
        public Event()
        {
            this.Event_Sess = new HashSet<Event_Sess>();
            this.Arbiters = new HashSet<Arbiter>();
            this.Entrants = new HashSet<Entrant>();
            this.MetaGameDefinitions = new HashSet<MetaGameDefinition>();
        }
    
        public int EIN { get; set; }
        public int Number { get; set; }
        public string Code { get; set; }
        public string Mind_Sport { get; set; }
        public string Type { get; set; }
        public Nullable<int> Number_in_Team { get; set; }
        public Nullable<int> MAX_Number { get; set; }
        public Nullable<bool> Pentamind { get; set; }
        public string Entry_Fee { get; set; }
        public Nullable<bool> incMaxFee { get; set; }
        public Nullable<float> No_Sessions { get; set; }
        public string Prize_Giving { get; set; }
        public Nullable<decimal> Prize_fund { get; set; }
        public string C1st_Prize { get; set; }
        public string C2nd_Prize { get; set; }
        public string C3rd_Prize { get; set; }
        public string Other_Prizes { get; set; }
        public Nullable<bool> JNR_Medals { get; set; }
        public string JNR_1st_Prize { get; set; }
        public string JNR_2nd_Prize { get; set; }
        public string JNR_3rd_Prize { get; set; }
        public string JNR_Other_Prizes { get; set; }
        public string Location { get; set; }
        public string Notes { get; set; }
        public Nullable<int> X_Num { get; set; }
        public Nullable<bool> Display { get; set; }
        public byte[] SSMA_TimeStamp { get; set; }
        public int OlympiadId { get; set; }
        public Nullable<int> GameId { get; set; }
        public bool ConsistentWithBoardability { get; set; }
        public float PentamindFactor { get; set; }
    
        public virtual ICollection<Event_Sess> Event_Sess { get; set; }
        public virtual ICollection<Arbiter> Arbiters { get; set; }
        public virtual Olympiad_Info Olympiad_Info { get; set; }
        public virtual Game Game { get; set; }
        public virtual ICollection<Entrant> Entrants { get; set; }
        public virtual ICollection<MetaGameDefinition> MetaGameDefinitions { get; set; }
    }
}
