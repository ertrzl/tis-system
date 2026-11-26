// Backend DTO-ları ilə eyni: Tis.Application/DTOs/Auth

export type UserRole = "SuperAdmin" | "Admin" | "Teacher" | "Student";

export interface CurrentUser {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  tenantId: number;
  tenantName: string;
}

export interface LoginRequest {
  tenantCode: string;
  email: string;
  password: string;
}

export interface TokenResponse {
  accessToken: string;
  expiresAt: string;
  user: CurrentUser;
}
