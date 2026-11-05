using Microsoft.EntityFrameworkCore;
using PersonalDetailsApi.Domain.Entities;
using PersonalDetailsApi.Domain.Enums;

namespace PersonalDetailsApi.Data;

/// <summary>Applies pending migrations at start-up and optionally seeds sample data.</summary>
public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        // DbContext is scoped, so we need our own scope outside of an HTTP request.
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        await db.Database.MigrateAsync();
        logger.LogInformation("Database ready at {DataSource}", db.Database.GetDbConnection().DataSource);

        if (app.Configuration.GetValue<bool>("Database:SeedSampleData") && !await db.Persons.IgnoreQueryFilters().AnyAsync())
        {
            db.Persons.AddRange(SamplePersons());
            var rows = await db.SaveChangesAsync();
            logger.LogInformation("Seeded sample data ({Rows} rows)", rows);
        }
    }

    // 'yield return' builds the sequence lazily (iterator method).
    private static IEnumerable<Person> SamplePersons()
    {
        yield return new Person
        {
            FirstName = "Aarav", LastName = "Menon", DateOfBirth = new DateOnly(1992, 4, 18), PlaceOfBirth = "Kochi",
            Gender = Gender.Male, MaritalStatus = MaritalStatus.Married, BloodGroup = BloodGroup.BPositive,
            Nationality = "Indian", Email = "aarav.menon@example.com", PrimaryPhone = "+91 98470 00001",
            Languages = ["English", "Malayalam", "Hindi"],
            Addresses = [new Address { Type = AddressType.Home, Line1 = "12 MG Road", City = "Kochi", State = "Kerala", PostalCode = "682016", Country = "India", IsPrimary = true }],
            EmergencyContacts = [new EmergencyContact { Name = "Diya Menon", Relationship = Relationship.Spouse, Phone = "+91 98470 00002" }],
            Educations = [new Education { Level = EducationLevel.Bachelor, Institution = "Cochin University", FieldOfStudy = "Computer Science", StartYear = 2010, EndYear = 2014, Grade = "8.4 CGPA" }],
            Employments = [new Employment { Employer = "Contoso Ltd", JobTitle = "Software Engineer", EmploymentType = EmploymentType.FullTime, StartDate = new DateOnly(2015, 7, 1) }],
            IdentityDocuments = [new IdentityDocument { DocumentType = DocumentType.Passport, DocumentNumber = "P1234567", IssuingCountry = "India", IssueDate = new DateOnly(2020, 1, 10), ExpiryDate = new DateOnly(2030, 1, 9) }],
        };
        yield return new Person
        {
            FirstName = "Sara", MiddleName = "J.", LastName = "Thomas", DateOfBirth = new DateOnly(1988, 11, 2), PlaceOfBirth = "Bengaluru",
            Gender = Gender.Female, MaritalStatus = MaritalStatus.Single, BloodGroup = BloodGroup.OPositive,
            Nationality = "Indian", Email = "sara.thomas@example.com", PrimaryPhone = "+91 98450 00003",
            Languages = ["English", "Kannada"],
            Addresses = [new Address { Type = AddressType.Home, Line1 = "45 Residency Road", City = "Bengaluru", State = "Karnataka", PostalCode = "560025", Country = "India", IsPrimary = true }],
            EmergencyContacts = [new EmergencyContact { Name = "John Thomas", Relationship = Relationship.Parent, Phone = "+91 98450 00004" }],
            Educations = [new Education { Level = EducationLevel.Master, Institution = "IISc", FieldOfStudy = "Data Science", StartYear = 2010, EndYear = 2012 }],
            Employments =
            [
                new Employment { Employer = "Fabrikam Inc", JobTitle = "Data Analyst", EmploymentType = EmploymentType.FullTime, StartDate = new DateOnly(2012, 8, 1), EndDate = new DateOnly(2019, 3, 31) },
                new Employment { Employer = "Northwind Traders", JobTitle = "Lead Data Scientist", EmploymentType = EmploymentType.FullTime, StartDate = new DateOnly(2019, 4, 15) },
            ],
        };
        yield return new Person
        {
            FirstName = "Liam", LastName = "Carter", DateOfBirth = new DateOnly(2001, 6, 30),
            Gender = Gender.Male, MaritalStatus = MaritalStatus.Single, BloodGroup = BloodGroup.ANegative,
            Nationality = "British", Email = "liam.carter@example.com", PrimaryPhone = "+44 7700 900123",
            Languages = ["English"],
            Addresses = [new Address { Type = AddressType.Home, Line1 = "221 Baker Street", City = "London", PostalCode = "NW1 6XE", Country = "United Kingdom", IsPrimary = true }],
            Educations = [new Education { Level = EducationLevel.Bachelor, Institution = "University of Leeds", FieldOfStudy = "Economics", StartYear = 2019, EndYear = 2022 }],
            Employments = [new Employment { Employer = "Adventure Works", JobTitle = "Graduate Analyst", EmploymentType = EmploymentType.Internship, StartDate = new DateOnly(2022, 9, 1) }],
        };
    }
}
