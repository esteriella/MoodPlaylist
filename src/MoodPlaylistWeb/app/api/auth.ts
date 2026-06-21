import { NextApiRequest, NextApiResponse } from "next";
import { ApiResponseModel, ApiResponseSchema } from "@/app/models/api-response.model";
import { config } from "@/app/helpers/config";
import { apiRequest } from "@/app/helpers/errorHandling";
import { RegisterSchema, LoginSchema, LoginResponseModel, RegisterModel, LoginResponseSchema } from "@/app/models/auth.models";

// export async function registerUser(form: RegisterModel): Promise<ApiResponseModel<LoginResponseModel>> {
//   console.log(config.apiBaseUrl);
//   // ✅ Validate input before sending
//   const parsed = RegisterSchema.safeParse(form);
//   if (!parsed.success) {
//     throw new Error("Invalid registration data: " + JSON.stringify(parsed.error.format()));
//   }

//   const res = await fetch(`${config.apiBaseUrl}/api/auth/register`, {
//     method: "POST",
//     headers: { "Content-Type": "application/json" },
//     body: JSON.stringify(parsed.data),
//   });

//   let data: unknown;
//   try {
//     data = await res.json();
//   } catch {
//     throw new Error("Server did not return JSON. Check backend response.");
//   }

//   // ✅ Validate response against ApiResponseModel<LoginResponseModel>
//   const responseSchema = ApiResponseSchema(LoginResponseSchema);
//   const parsedResponse = responseSchema.safeParse(data);

//   if (!parsedResponse.success) {
//     throw new Error("Invalid API response: " + JSON.stringify(parsedResponse.error.format()));
//   }

//   if (!res.ok || !parsedResponse.data.successful) {
//     throw new Error(parsedResponse.data.message || "Registration failed");
//   }

//   return parsedResponse.data; // ✅ typed ApiResponseModel<LoginResponseModel>
// }


// export async function loginUser(
//   form: { email: string; password: string }
// ): Promise<ApiResponseModel<LoginResponseModel>> {
//   const res = await fetch(`${config.apiBaseUrl}/auth/login`, {
//     method: "POST",
//     headers: { "Content-Type": "application/json" },
//     body: JSON.stringify(form),
//   });

//   let data: unknown;
//   try {
//     data = await res.json();
//   } catch {
//     throw new Error("Server did not return JSON. Check backend response.");
//   }

//   // ✅ Validate response against ApiResponseModel<LoginResponseModel>
//   const responseSchema = ApiResponseSchema(LoginResponseSchema);
//   const parsedResponse = responseSchema.safeParse(data);

//   if (!parsedResponse.success) {
//     throw new Error("Invalid API response: " + JSON.stringify(parsedResponse.error.format()));
//   }

//   if (!res.ok || !parsedResponse.data.successful) {
//     throw new Error(parsedResponse.data.message || "Login failed");
//   }

//   return parsedResponse.data; // ✅ typed ApiResponseModel<LoginResponseModel>
// }

export async function registerUser(form: RegisterModel): Promise<ApiResponseModel<LoginResponseModel>> {
  return apiRequest<ApiResponseModel<LoginResponseModel>>(
    `${config.apiBaseUrl}/auth/register`,
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(form),
    },
    () => ApiResponseSchema(LoginResponseSchema)
  );
}

export async function loginUser(form: { email: string; password: string }): Promise<ApiResponseModel<LoginResponseModel>> {
  return apiRequest<ApiResponseModel<LoginResponseModel>>(
    `${config.apiBaseUrl}/auth/login`,
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(form),
    },
    () => ApiResponseSchema(LoginResponseSchema)
  );
}
