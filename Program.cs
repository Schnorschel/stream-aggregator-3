using System;
using stream_aggregator_3.Models;
using System.Linq;
using Newtonsoft.Json;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace stream_aggregator_3
{
  public class Program
  {
    // private readonly StreamAggregatorContext context;

    // public Program(StreamAggregatorContext _context)
    // {
    //   this.context = _context;
    // }

    // static StreamAggregatorContext Db = new StreamAggregatorContext();
    // static List<Person> Persons = new List<Person>();
    // static List<Course> Courses = new List<Course>();
    // static List<CourseEnrolled> CoursesEnrolled = new List<CourseEnrolled>();

    // Process all events in chronological order
    static void ConsumeEvents(IEnumerable<EventStream> Jmodel)
    {
      foreach (var evIt in Jmodel.OrderBy(ev => ev.EventId))
      {
        ConsumeEvent(evIt);
      }
    }

    static void ConsumeEvent(EventStream EvIt)
    {
      var Db = new StreamAggregatorContext();
      switch (EvIt.Event.ToLower())
      {
        case "person_new":
          // Add new person to (local) data structure
          // Check first whether a person with same Id already exists
          var existingPerson = Db.Persons.FirstOrDefault(p => p.PersonId == EvIt.StreamId);
          if (existingPerson == null)
          {
            // Person does not exist, so make sure name has been provided,  
            // and, if so, add new person to (local) data structure
            if (EvIt.data.Name != "")
            {
              // Instantiate a new Person object, populate its properties..
              var newPerson = new Person();
              newPerson.PersonId = EvIt.StreamId;
              newPerson.Name = EvIt.data.Name;
              // ..and add it to List Collection Persons
              Db.Persons.Add(newPerson);
              Db.SaveChanges();
              // Log that "person_new" event was successfully processed
              AddToLog(EvIt.EventId, EvIt.StreamId, $"Successfully processed event '{EvIt.Event}' to add person '{EvIt.data.Name}'.");
            }
            else
            {
              // Issue error that new person name has not been provided
              AddToLog(EvIt.EventId, EvIt.StreamId, $"Could not process event '{EvIt.Event}' due to missing person name.");
            }
          }
          else
          {
            // Issue error that person already exists and cannot be replaced/overwritten
            AddToLog(EvIt.EventId, EvIt.StreamId, $"Could not process event '{EvIt.Event}' due to person {EvIt.data.Name} already existing.");
          }
          break;
        case "course_new":
          // Add new course to (local) data structure
          // Check first, if course already exists
          var existingCourse = Db.Courses.FirstOrDefault(c => c.CourseId == EvIt.StreamId);
          if (existingCourse == null)
          {
            // Course does not exist, so make sure course name has been provided, and if so, 
            // add it to (local) data structure
            if (EvIt.data.Name != "")
            {
              var newCourse = new Course();
              newCourse.CourseId = EvIt.StreamId;
              newCourse.Name = EvIt.data.Name;
              Db.Courses.Add(newCourse);
              Db.SaveChanges();
              // Log that "course_new" event was successfully processed
              AddToLog(EvIt.EventId, EvIt.StreamId, $"Successfully processed event '{EvIt.Event}' to create course '{EvIt.data.Name}'.");
            }
            else
            {
              // Issue error that course name has not been provided
              AddToLog(EvIt.EventId, EvIt.StreamId, $"Could not process event '{EvIt.Event}' due to missing course name.");
            }
          }
          else
          {
            // Issue error that course already exists and cannot be replaced/overwritten
            AddToLog(EvIt.EventId, EvIt.StreamId, $"Could not process event '{EvIt.Event}' due to course {EvIt.data.Name} already existing.");
          }
          break;
        case "course_enrolled":
          // Enroll existing person into existing course
          // Check that course and person exist
          existingCourse = Db.Courses.FirstOrDefault(c => c.CourseId == EvIt.StreamId);
          if (existingCourse != null)
          {
            existingPerson = Db.Persons.FirstOrDefault(p => p.PersonId == EvIt.data.PersonId);
            if (existingPerson != null)
            {
              // Make sure that enrollment does not already exist
              var existingCourseEnrolled = Db.CoursesEnrolled.FirstOrDefault(ce => ce.CourseId == EvIt.StreamId && ce.PersonId == EvIt.data.PersonId);
              if (existingCourseEnrolled == null)
              {
                // Create a new CourseEnrollment object, populate all properties and save to data structure
                var newCourseEnrolled = new CourseEnrolled();
                newCourseEnrolled.CourseId = EvIt.StreamId;
                // newCourseEnrolled.CourseName = existingCourse.Name;
                newCourseEnrolled.PersonId = (int)EvIt.data.PersonId;
                // newCourseEnrolled.Persons = existingPerson;
                Db.CoursesEnrolled.Add(newCourseEnrolled);
                Db.SaveChanges();
                // Log that "course_enrolled" event was successfully processed
                AddToLog(EvIt.EventId, EvIt.StreamId, $"Successfully processed event '{EvIt.Event}' to enroll person '{existingPerson.Name}' in course '{existingCourse.Name}'.");
              }
              else
              {
                // Issue error that course enrollment does already exist
                AddToLog(EvIt.EventId, EvIt.StreamId, $"Could not process event '{EvIt.Event}' due to course enrollment for person '{existingPerson.Name}' in course '{existingCourse.Name}' already existing.");
              }
            }
            else
            {
              // Issue error that person does not exist
              AddToLog(EvIt.EventId, EvIt.StreamId, $"Could not process event '{EvIt.Event}' due to person {EvIt.data.PersonId} not existing.");
            }
          }
          else
          {
            // Issue error that course does not exist
            AddToLog(EvIt.EventId, EvIt.StreamId, $"Could not process event '{EvIt.Event}' due to course {EvIt.StreamId} not existing.");
          }
          break;
        case "course_renamed":
          // Rename existing course
          // Ensure course to rename exists
          existingCourse = Db.Courses.FirstOrDefault(c => c.CourseId == EvIt.StreamId);
          if (existingCourse != null)
          {
            if (EvIt.data.Name != null)
            {
              string existingCourseName = existingCourse.Name;
              existingCourse.Name = EvIt.data.Name;
              Db.SaveChanges();
              // Log that "course_renamed" event was successfully processed
              AddToLog(EvIt.EventId, EvIt.StreamId, $"Successfully processed event '{EvIt.Event}' to rename course '{existingCourseName}' to '{EvIt.data.Name}'.");
            }
            else
            {
              // Issue error that new course name was not provided
              AddToLog(EvIt.EventId, EvIt.StreamId, $"Could not process event '{EvIt.Event}' due to missing course name.");
            }
          }
          else
          {
            // Issue error that course to rename does not exist
            AddToLog(EvIt.EventId, EvIt.StreamId, $"Could not process event '{EvIt.Event}' due to course {EvIt.StreamId} not existing.");
          }
          break;
        case "course_disenrolled":
          // Unenroll person from course
          // First, ensure enrollment record exists
          var existingCourseEnrolled2 = Db.CoursesEnrolled.FirstOrDefault(ce => ce.CourseId == EvIt.StreamId && ce.PersonId == EvIt.data.PersonId);
          if (existingCourseEnrolled2 != null)
          {
            Db.CoursesEnrolled.Remove(existingCourseEnrolled2);
            Db.SaveChanges();
            // Log that "course_disenrolled" event was successfully processed
            AddToLog(EvIt.EventId, EvIt.StreamId, $"Successfully processed event '{EvIt.Event}' to unenroll person {EvIt.data.PersonId} from course {EvIt.StreamId}.");
          }
          else
          {
            // Issue error that course enrollment does not exist
            AddToLog(EvIt.EventId, EvIt.StreamId, $"Could not process event '{EvIt.Event}' due to course enrollment for course {EvIt.StreamId} and person {EvIt.data.PersonId} not existing.");
          }
          break;
        case "person_renamed":
          // Rename existing person
          // Ensure person exists, and new name has been provided; if so, overwrite name
          existingPerson = Db.Persons.FirstOrDefault(p => p.PersonId == EvIt.StreamId);
          if (existingPerson != null && EvIt.data.Name != null && EvIt.data.Name != "")
          {
            string existingPersonName = existingPerson.Name;
            existingPerson.Name = EvIt.data.Name;
            Db.SaveChanges();
            // Log that "person_renamed" event was successfully processed
            AddToLog(EvIt.EventId, EvIt.StreamId, $"Successfully processed event '{EvIt.Event}' to rename person '{existingPersonName}' to '{EvIt.data.Name}'.");
          }
          else
          {
            // Person does not exist or name not provided: cannot rename, issue appropriate error
            AddToLog(EvIt.EventId, EvIt.StreamId, $"Could not process event '{EvIt.Event}' due to person {EvIt.StreamId} not existing or person name not provided.");
          }
          break;
        default:
          // Log that event could not be processed, because it has no code handler
          AddToLog(EvIt.EventId, EvIt.StreamId, $"Could not process event '{EvIt.Event}' because no handler exists for it.");
          break;
      }
    }

    static void CleanTableCourseEnrolled()
    {
      var Db = new StreamAggregatorContext();
      Db.CoursesEnrolled.RemoveRange(Db.CoursesEnrolled);
      Db.SaveChanges();
    }

    static void CleanTableCourse()
    {
      var Db = new StreamAggregatorContext();
      Db.Courses.RemoveRange(Db.Courses);
      Db.SaveChanges();
    }

    static void CleanTablePerson()
    {
      var Db = new StreamAggregatorContext();
      Db.Persons.RemoveRange(Db.Persons);
      Db.SaveChanges();
    }

    static void CleanAllTables()
    {
      CleanTableCourseEnrolled();
      CleanTableCourse();
      CleanTablePerson();
    }

    static void DisplayEventStream(IEnumerable<EventStream> Jmodel)
    // Basic Output routine to show select data of JSON object (used for testing)
    {
      foreach (var evs in Jmodel.OrderBy(ev => ev.EventId))
      {
        Console.Write($"eventId: {evs.EventId}.");
        if (evs.data.Name != "" && evs.data.Name != null)
        {
          Console.Write($" data.name: {evs.data.Name}.");
        }
        if (evs.data.PersonId != null)
        {
          Console.Write($" data.personId: {evs.data.PersonId}");
        }
        Console.WriteLine();
      }
    }

    static void AddToLog(int EventId, int StreamId, string Description)
    {
      Console.WriteLine($"EventId: {EventId}. StreamId: {StreamId}. [{Description}]");
    }

    static void Main(string[] args)
    // - Import the event stream from external JSON file into local List object
    // - Process the event stream in chronological order and - after successful validation 
    //   of each event - write data to local objects 
    {
      // Specify options for built-in JSON serializer
      var options = new JsonSerializerOptions
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        IgnoreNullValues = true,
        WriteIndented = true
      };

      // Specify options for json.net (Newtonsoft) serializer
      var settings = new JsonSerializerSettings
      {
        NullValueHandling = NullValueHandling.Ignore,
        MissingMemberHandling = MissingMemberHandling.Ignore
      };

      // Import the event stream from external JSON file into local string
      var jsonString = File.ReadAllText("events.json");
      // This statement failed with given JSON file structure, so json.net was utilized instead
      // var jsonModel = JsonSerializer.Deserialize<EventStream>(jsonString, options);
      // Port imported JSON string data into List object
      var jsonModel = JsonConvert.DeserializeObject<List<EventStream>>(jsonString, settings);

      // Display the events to be processed
      // DisplayEventStream(jsonModel);

      // Clear all tables of their contents
      CleanAllTables();

      // Process events
      ConsumeEvents(jsonModel);

      Console.WriteLine();

      // var modelJson = System.Text.Json.JsonSerializer.Serialize(jsonModel, options);
      Console.WriteLine("Course:");
      Console.WriteLine("-------");
      var DbC = new StreamAggregatorContext();
      Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(DbC.Courses, options));
      Console.WriteLine();
      Console.WriteLine("Person:");
      Console.WriteLine("-------");
      var DbP = new StreamAggregatorContext();
      Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(DbP.Persons, options));
      Console.WriteLine();
      Console.WriteLine("Course Enrollment:");
      Console.WriteLine("------------------");
      // Error: 'List<Course>' does not contain a definition for 'Include' and no accessible extension method 'Include' accepting a first argument of type 'List<Course>' could be found
      var DbC2 = new StreamAggregatorContext();
      Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(DbC2.Courses.Include(c => c.CoursesEnrolled).ThenInclude(ce => ce.Person).ToList(), options));

      // Console.WriteLine(modelJson);
    }
  }
}
