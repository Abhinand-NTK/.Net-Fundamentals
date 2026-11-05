namespace PersonalDetailsApi.Domain.Enums;

public enum Gender { Male, Female, NonBinary, Other, PreferNotToSay }

public enum MaritalStatus { Single, Married, Divorced, Widowed, Separated, DomesticPartnership }

public enum BloodGroup { Unknown, APositive, ANegative, BPositive, BNegative, ABPositive, ABNegative, OPositive, ONegative }

public enum AddressType { Home, Permanent, Work, Mailing, Other }

public enum Relationship { Spouse, Parent, Child, Sibling, Relative, Friend, Colleague, Guardian, Other }

public enum EducationLevel { HighSchool, Diploma, Associate, Bachelor, Master, Doctorate, Certification, Other }

public enum EmploymentType { FullTime, PartTime, Contract, Internship, Freelance, SelfEmployed }

public enum DocumentType { Passport, NationalId, DrivingLicense, VoterId, TaxId, Other }

/// <summary>Fields a person list can be sorted by.</summary>
public enum PersonSortField { LastName, FirstName, DateOfBirth, CreatedAt }

public enum SortDirection { Asc, Desc }
