import { z } from "zod";
// ✅ Define a Zod schema for ApiResponseModel<T>

export const ApiResponseSchema = <T extends z.ZodTypeAny>(dataSchema: T) =>
  z.object({
    statusCode: z.number(),
    successful: z.boolean(),
    message: z.string(),
    errorDetails: z.string().optional(),
    pageNumber: z.number(),
    pageSize: z.number(),
    data: dataSchema,
  });
export interface ApiResponseModel<T> {
    statusCode: number;
    successful: boolean;
    message: string;
    errorDetails?: string;
    pageNumber: number;
    pageSize: number;
    data: T;
}