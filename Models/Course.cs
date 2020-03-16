using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace stream_aggregator_3.Models
{
  public class Course
  {
    public int CourseId { get; set; }
    public string Name { get; set; }
    // [JsonIgnore]
    public List<CourseEnrolled> CoursesEnrolled { get; set; } = new List<CourseEnrolled>();
  }
}