import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import LoginPage from "./page";
import { loginUser } from "@/app/api/auth";
import { useAuth } from "@/app/context/AuthContext";

vi.mock("@/app/api/auth", () => ({ loginUser: vi.fn() }));
vi.mock("@/app/context/AuthContext", () => ({ useAuth: vi.fn() }));

describe("LoginPage", () => {
  const login = vi.fn();

  beforeEach(() => vi.mocked(useAuth).mockReturnValue({ login } as unknown as ReturnType<typeof useAuth>));

  it("shows validation feedback without calling the API", async () => {
    const user = userEvent.setup();
    render(<LoginPage />);

    await user.type(screen.getByLabelText("Email address"), "listener@example.com");
    await user.type(screen.getByLabelText("Password"), "weak");
    await user.click(screen.getByRole("button", { name: "Sign in" }));

    expect(await screen.findByRole("alert")).toHaveTextContent("Password must be at least 8 characters");
    expect(loginUser).not.toHaveBeenCalled();
  });

  it("logs in with the returned credentials", async () => {
    vi.mocked(loginUser).mockResolvedValue({
      statusCode: 200,
      successful: true,
      message: "success",
      pageNumber: 1,
      pageSize: 1,
      data: { token: "token-123", name: "Ada", tag: "ada", refreshToken: "refresh" },
    });
    const user = userEvent.setup();
    render(<LoginPage />);

    await user.type(screen.getByLabelText("Email address"), "ada@example.com");
    await user.type(screen.getByLabelText("Password"), "Strong1!");
    await user.click(screen.getByRole("button", { name: "Sign in" }));

    expect(loginUser).toHaveBeenCalledWith({ email: "ada@example.com", password: "Strong1!" });
    expect(login).toHaveBeenCalledWith("token-123", "Ada");
  });

  it("displays an API error", async () => {
    vi.mocked(loginUser).mockRejectedValue(new Error("Invalid credentials"));
    const user = userEvent.setup();
    render(<LoginPage />);

    await user.type(screen.getByLabelText("Email address"), "ada@example.com");
    await user.type(screen.getByLabelText("Password"), "Strong1!");
    await user.click(screen.getByRole("button", { name: "Sign in" }));

    expect(await screen.findByRole("alert")).toHaveTextContent("Invalid credentials");
    expect(login).not.toHaveBeenCalled();
  });
});
