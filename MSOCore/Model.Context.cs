﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class DataEntities : DbContext
    {
        public DataEntities()
            : base("name=DataEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Badges___Misc> Badges___Misc { get; set; }
        public DbSet<Entrant> Entrants { get; set; }
        public DbSet<Event_Resource> Event_Resources { get; set; }
        public DbSet<Event_Sess> Event_Sesses { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventType> EventTypes { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Information> Information { get; set; }
        public DbSet<Medal_Point> Medal_Points { get; set; }
        public DbSet<Contestant> Contestants { get; set; }
        public DbSet<Olympiad_Info> Olympiad_Infoes { get; set; }
        public DbSet<Paring> Parings { get; set; }
        public DbSet<Payment_Method> Payment_Methods { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<SelectedCategory> SelectedCategories { get; set; }
        public DbSet<SelectedPeople> SelectedPeoples { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserLogin> UserLogins { get; set; }
        public DbSet<Fee> Fees { get; set; }
        public DbSet<Arbiter> Arbiters { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<GameCategory> GameCategories { get; set; }
        public DbSet<EntryJson> EntryJsons { get; set; }
        public DbSet<Nationality> Nationalities { get; set; }
        public DbSet<GlobalSetting> GlobalSettings { get; set; }
        public DbSet<Seeding> Seedings { get; set; }
        public DbSet<WomenNotInWomensPentamind> WomenNotInWomensPentaminds { get; set; }
        public DbSet<MetaGameDefinition> MetaGameDefinitions { get; set; }
        public DbSet<Parameter> Parameters { get; set; }
        public DbSet<Rating> Ratings { get; set; }
    }
}
