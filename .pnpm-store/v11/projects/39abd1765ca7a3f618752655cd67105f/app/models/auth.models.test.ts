import { describe, expect, it } from "vitest";
import { LoginSchema, RegisterSchema } from "./auth.models";

describe("authentication schemas", () => {
  const validPassword = "Strong1!";

  it("accepts valid login details", () => {
    expect(LoginSchema.safeParse({ email: "listener@example.com", password: validPassword }).success).toBe(true);
  });

  it.each([
    ["short passwords", "Ab1!"],
    ["passwords without an uppercase letter", "lowercase1!"],
    ["passwords without a number", "Password!"],
    ["passwords without a special character", "Password1"],
  ])("rejects %s", (_description, password) => {
    expect(LoginSchema.safeParse({ email: "listener@example.com", password }).success).toBe(false);
  });

  it("rejects invalid email addresses", () => {
    expect(LoginSchema.safeParse({ email: "not-an-email", password: validPassword }).success).toBe(false);
  });

  it("requires a registration name of at least two characters", () => {
    expect(RegisterSchema.safeParse({ name: "A", email: "a@example.com", password: validPassword }).success).toBe(false);
  });
});
