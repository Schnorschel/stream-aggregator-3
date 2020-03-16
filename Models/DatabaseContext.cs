using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace stream_aggregator_3.Models
{
  public partial class StreamAggregatorContext : DbContext
  {

    public DbSet<CourseEnrolled> CoursesEnrolled { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Person> Persons { get; set; }

    // Add Database tables here
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      if (!optionsBuilder.IsConfigured)
      {
        // #error Make sure to update the connection string to the correct database
        optionsBuilder.UseNpgsql("server=localhost;database=stream_aggregator;user id=postgres");
      }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<CourseEnrolled>(ece =>
      {
        ece.HasKey(ec => new { ec.CourseId, ec.PersonId });
        ece.HasOne(ce => ce.Person).WithMany(p => p.CoursesEnrolled);
      });
      modelBuilder.Entity<Course>(ec =>
      {
        ec.HasMany(c => c.CoursesEnrolled).WithOne(ce => ce.Course);
      });
    }
  }
}
