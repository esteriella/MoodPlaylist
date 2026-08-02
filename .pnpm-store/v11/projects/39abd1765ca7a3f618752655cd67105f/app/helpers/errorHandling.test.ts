import toast from "react-hot-toast";
import { afterEach, describe, expect, it, vi } from "vitest";
import { apiRequest } from "./errorHandling";

vi.mock("react-hot-toast", () => ({
  default: { error: vi.fn(), success: vi.fn() },
}));

describe("apiRequest", () => {
  afterEach(() => vi.unstubAllGlobals());

  it("returns parsed JSON for a successful request", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response(JSON.stringify({ data: [1, 2] }), {
      status: 200,
      headers: { "Content-Type": "application/json" },
    })));

    await expect(apiRequest("/items", { method: "GET" }, vi.fn())).resolves.toEqual({ data: [1, 2] });
    expect(toast.success).not.toHaveBeenCalled();
  });

  it("shows a success toast for a mutating request with a message", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response(JSON.stringify({ message: "Playlist saved" }), { status: 200 })));

    await apiRequest("/playlists", { method: "POST" }, vi.fn());

    expect(toast.success).toHaveBeenCalledWith("Playlist saved");
  });

  it("logs out and reports the API message on a 401", async () => {
    const logout = vi.fn();
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response(JSON.stringify({ message: "Session expired" }), { status: 401 })));

    await expect(apiRequest("/private", {}, logout)).rejects.toThrow("Session expired");
    expect(logout).toHaveBeenCalledOnce();
    expect(toast.error).toHaveBeenCalledWith("Session expired");
  });

  it("reports network failures", async () => {
    vi.stubGlobal("fetch", vi.fn().mockRejectedValue(new TypeError("offline")));

    await expect(apiRequest("/items", {}, vi.fn())).rejects.toThrow("Network error");
    expect(toast.error).toHaveBeenCalledWith("Network error: Could not reach server");
  });

  it("reports non-JSON responses", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response("not json", { status: 200 })));

    await expect(apiRequest("/items", {}, vi.fn())).rejects.toThrow("Invalid JSON response");
    expect(toast.error).toHaveBeenCalledWith("Server did not return JSON");
  });
});
