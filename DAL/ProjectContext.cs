using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;
using SystemWspomaganiaNauczania.Models;

namespace SystemWspomaganiaNauczania.DAL
{
    public class ProjectContext : DbContext
    {
        public ProjectContext() : base("DefaultConnection")
        {

        }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<OrderTask> OrderTasks { get; set; }
        public DbSet<Word> TaskWords { get; set; }
        public DbSet<WordInOrderTask> WordInOrderTasks { get; set; }
        public DbSet<GroupTaskWord> GroupTaskWords { get; set; }
        public DbSet<GroupTaskSolved> GroupTaskSolved { get; set; }
        public DbSet<OrderTaskSolved> OrderTaskSolved { get; set; }
        public DbSet<Container> Container { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<GroupTask> GroupTasks { get; set; }

        public System.Data.Entity.DbSet<SystemWspomaganiaNauczania.Models.FontStyle> FontStyles { get; set; }
    }
}