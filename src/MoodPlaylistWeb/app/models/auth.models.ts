import { z } from "zod";

// ✅ Login schema
export const LoginSchema = z.object({
  email: z.email(),
  password: z
    .string()
    .min(8, "Password must be at least 8 characters")
    .max(12, "Password must not exceed 12 characters")
    .regex(
      /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$/,
      "Password must include uppercase, lowercase, number, and special character"
    ),
});

// ✅ Register schema
export const RegisterSchema = z.object({
  name: z.string().min(2).max(100),
  email: z.email(),
  password: z
    .string()
    .min(8, "Password must be at least 8 characters")
    .max(12, "Password must not exceed 12 characters")
    .regex(
      /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$/,
      "Password must include uppercase, lowercase, number, and special character"
    ),
});

// ✅ Zod schema for LoginResponseModel
export const LoginResponseSchema = z.object({
  tag: z.string(),
  name: z.string(),
  token: z.string(),
  refreshToken: z.string(),
});

// ✅ Type inference from Zod
export type RegisterModel = z.infer<typeof RegisterSchema>;
export type LoginModel = z.infer<typeof LoginSchema>;
export type LoginResponseModel = z.infer<typeof LoginResponseSchema>;

// ✅ Response model
// export interface LoginResponseModel {
//   tag: string;
//   name: string;
//   token: string;
//   refreshToken: string;
// }
