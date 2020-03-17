using NUnit.Framework;
namespace stream_aggregator_3
{
  [TestFixture]
  class ProgramTests
  {
    [TestCase("Joe Bloggs", 0)]
    [TestCase("Belva Poppins", 1)]
    [TestCase("Gal Person", 2)]
    [TestCase("Guy Person", 3)]
    public void Should_Return_Person_Name_Given_Id(string Name, int Id)
    {
      string result = Program.GetPersonNameById(Id);
      Assert.AreEqual(Name, result);
    }

    [TestCase("Ice Cream Eating", 4)]
    [TestCase("Popcorn Popping", 5)]
    public void Should_Return_Course_Name_Given_Id(string Name, int Id)
    {
      string result = Program.GetCourseNameById(Id);
      Assert.AreEqual(Name, result);
    }

    [TestCase(4, 1)]
    [TestCase(4, 2)]
    [TestCase(5, 1)]
    [TestCase(5, 3)]
    public void Should_Return_True_Given_Course_And_Person_Ids(int CourseId, int PersonId)
    {
      bool result = Program.EnrollmentExistsByIds(CourseId, PersonId);
      Assert.IsTrue(result);
    }

    [TestCase(4, 3)]
    public void Should_Return_False_Given_Course_And_Person_Ids(int CourseId, int PersonId)
    {
      bool result = Program.EnrollmentExistsByIds(CourseId, PersonId);
      Assert.IsFalse(result);
    }

  }
}