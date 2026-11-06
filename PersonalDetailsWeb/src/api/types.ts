// TypeScript mirrors of the C# DTOs in PersonalDetailsApi/Contracts.
// The API serialises with camelCase names, enums as strings, DateOnly as "yyyy-MM-dd",
// and omits null values, so nullable C# properties become optional (`?`) here.

// ---------- Enums (Domain/Enums/Enums.cs) ----------
export const GENDERS = ['Male', 'Female', 'NonBinary', 'Other', 'PreferNotToSay'] as const;
export const MARITAL_STATUSES = ['Single', 'Married', 'Divorced', 'Widowed', 'Separated', 'DomesticPartnership'] as const;
export const BLOOD_GROUPS = ['Unknown', 'APositive', 'ANegative', 'BPositive', 'BNegative', 'ABPositive', 'ABNegative', 'OPositive', 'ONegative'] as const;
export const ADDRESS_TYPES = ['Home', 'Permanent', 'Work', 'Mailing', 'Other'] as const;
export const RELATIONSHIPS = ['Spouse', 'Parent', 'Child', 'Sibling', 'Relative', 'Friend', 'Colleague', 'Guardian', 'Other'] as const;
export const EDUCATION_LEVELS = ['HighSchool', 'Diploma', 'Associate', 'Bachelor', 'Master', 'Doctorate', 'Certification', 'Other'] as const;
export const EMPLOYMENT_TYPES = ['FullTime', 'PartTime', 'Contract', 'Internship', 'Freelance', 'SelfEmployed'] as const;
export const DOCUMENT_TYPES = ['Passport', 'NationalId', 'DrivingLicense', 'VoterId', 'TaxId', 'Other'] as const;
export const SORT_FIELDS = ['LastName', 'FirstName', 'DateOfBirth', 'CreatedAt'] as const;
export const SORT_DIRECTIONS = ['Asc', 'Desc'] as const;

export type Gender = (typeof GENDERS)[number];
export type MaritalStatus = (typeof MARITAL_STATUSES)[number];
export type BloodGroup = (typeof BLOOD_GROUPS)[number];
export type AddressType = (typeof ADDRESS_TYPES)[number];
export type Relationship = (typeof RELATIONSHIPS)[number];
export type EducationLevel = (typeof EDUCATION_LEVELS)[number];
export type EmploymentType = (typeof EMPLOYMENT_TYPES)[number];
export type DocumentType = (typeof DOCUMENT_TYPES)[number];
export type PersonSortField = (typeof SORT_FIELDS)[number];
export type SortDirection = (typeof SORT_DIRECTIONS)[number];

// ---------- Requests (Contracts/Requests) ----------
export interface PersonQueryParameters {
  search?: string;
  gender?: Gender;
  bloodGroup?: BloodGroup;
  maritalStatus?: MaritalStatus;
  city?: string;
  minAge?: number;
  maxAge?: number;
  sortBy?: PersonSortField;
  sortDirection?: SortDirection;
  page?: number;
  pageSize?: number;
}

export interface AddressRequest {
  type: AddressType;
  line1: string;
  line2: string | null;
  city: string;
  state: string | null;
  postalCode: string;
  country: string;
  isPrimary: boolean;
}

export interface EmergencyContactRequest {
  name: string;
  relationship: Relationship;
  phone: string;
  email: string | null;
}

export interface EducationRequest {
  level: EducationLevel;
  institution: string;
  fieldOfStudy: string | null;
  startYear: number;
  endYear: number | null;
  grade: string | null;
}

export interface EmploymentRequest {
  employer: string;
  jobTitle: string;
  employmentType: EmploymentType;
  startDate: string;
  endDate: string | null;
}

export interface IdentityDocumentRequest {
  documentType: DocumentType;
  documentNumber: string;
  issuingCountry: string;
  issueDate: string | null;
  expiryDate: string | null;
}

export interface PersonRequest {
  firstName: string;
  middleName: string | null;
  lastName: string;
  preferredName: string | null;
  dateOfBirth: string;
  placeOfBirth: string | null;
  gender: Gender;
  maritalStatus: MaritalStatus;
  bloodGroup: BloodGroup;
  nationality: string;
  email: string;
  primaryPhone: string;
  alternatePhone: string | null;
  languages: string[];
  notes: string | null;
  addresses: AddressRequest[];
  emergencyContacts: EmergencyContactRequest[];
  educations: EducationRequest[];
  employments: EmploymentRequest[];
  identityDocuments: IdentityDocumentRequest[];
}

// ---------- Responses (Contracts/Responses) ----------
export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface PersonSummaryResponse {
  id: string;
  fullName: string;
  email: string;
  primaryPhone: string;
  age: number;
  gender: Gender;
  nationality: string;
  city?: string;
}

export interface AddressResponse {
  id: string;
  type: AddressType;
  line1: string;
  line2?: string;
  city: string;
  state?: string;
  postalCode: string;
  country: string;
  isPrimary: boolean;
}

export interface EmergencyContactResponse {
  id: string;
  name: string;
  relationship: Relationship;
  phone: string;
  email?: string;
}

export interface EducationResponse {
  id: string;
  level: EducationLevel;
  institution: string;
  fieldOfStudy?: string;
  startYear: number;
  endYear?: number;
  grade?: string;
}

export interface EmploymentResponse {
  id: string;
  employer: string;
  jobTitle: string;
  employmentType: EmploymentType;
  startDate: string;
  endDate?: string;
  isCurrent: boolean;
}

export interface IdentityDocumentResponse {
  id: string;
  documentType: DocumentType;
  maskedDocumentNumber: string;
  issuingCountry: string;
  issueDate?: string;
  expiryDate?: string;
  isExpired: boolean;
}

export interface PersonDetailResponse {
  id: string;
  firstName: string;
  middleName?: string;
  lastName: string;
  preferredName?: string;
  fullName: string;
  dateOfBirth: string;
  age: number;
  placeOfBirth?: string;
  gender: Gender;
  maritalStatus: MaritalStatus;
  bloodGroup: BloodGroup;
  nationality: string;
  email: string;
  primaryPhone: string;
  alternatePhone?: string;
  languages: string[];
  notes?: string;
  addresses: AddressResponse[];
  emergencyContacts: EmergencyContactResponse[];
  educations: EducationResponse[];
  employments: EmploymentResponse[];
  identityDocuments: IdentityDocumentResponse[];
  createdAtUtc: string;
  updatedAtUtc?: string;
}

export interface PersonStatisticsResponse {
  totalPersons: number;
  averageAge: number;
  byGender: Record<string, number>;
  byBloodGroup: Record<string, number>;
  byNationality: Record<string, number>;
  byAgeGroup: Record<string, number>;
}

/** RFC 7807 error body. Validation errors (400) also carry `errors`: field path → messages. */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
}
