// Backend DTO-ları ilə eyni: Tis.Application/DTOs/*

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

// ---- Universitetlər (SuperAdmin) ----

export interface Tenant {
  id: number;
  code: string;
  name: string;
  isActive: boolean;
  isPlatform: boolean;
  userCount: number;
  createdAt: string;
  // Yalnız yaradılma cavabında: ilk adminə dəvət e-poçtu göndərilibmi
  inviteEmailSent: boolean | null;
}

export interface CreateTenantRequest {
  code: string;
  name: string;
  adminEmail: string;
  adminFirstName: string;
  adminLastName: string;
}

// ---- İstifadəçilər (Admin) ----

export interface UserItem {
  id: number;
  email: string;
  studentNumber: string | null;
  firstName: string;
  lastName: string;
  role: UserRole;
  invitationPending: boolean;
  createdAt: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasMore: boolean;
}

// ---- Dəvət ----

export interface InviteInfo {
  firstName: string;
  email: string;
  studentNumber: string | null;
  tenantName: string;
}
