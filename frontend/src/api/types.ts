// Backend DTO-ları ilə eyni: Tis.Application/DTOs/Auth

export type UserRole = "SuperAdmin" | "Admin" | "Teacher" | "Student";

export interface CurrentUser {
  id: number;
  email: string;
  studentNumber: string | null;
  firstName: string;
  lastName: string;
  role: UserRole;
  tenantId: number;
  tenantName: string;
}

export interface LoginRequest {
  tenantCode: string;
  identifier: string; // tələbə nömrəsi və ya e-poçt
  password: string;
}

export interface TokenResponse {
  accessToken: string;
  expiresAt: string;
  refreshToken: string;
  refreshExpiresAt: string;
  user: CurrentUser;
}
