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

  it("refreshes once and retries an expired authenticated request", async () => {
    const fetchMock = vi.fn()
      .mockResolvedValueOnce(new Response(JSON.stringify({ message: "Expired" }), { status: 401 }))
      .mockResolvedValueOnce(new Response(JSON.stringify({ data: ["track"] }), { status: 200 }));
    vi.stubGlobal("fetch", fetchMock);
    const refresh = vi.fn().mockResolvedValue("fresh-token");

    await expect(apiRequest("/recommendations", { headers: { Authorization: "Bearer old-token" } }, vi.fn(), refresh))
      .resolves.toEqual({ data: ["track"] });

    expect(refresh).toHaveBeenCalledOnce();
    expect(new Headers(fetchMock.mock.calls[1][1].headers).get("Authorization")).toBe("Bearer fresh-token");
  });

  it("signs out when a session cannot be refreshed", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response(JSON.stringify({ message: "Expired" }), { status: 401 })));
    const logout = vi.fn();

    await expect(apiRequest("/recommendations", {}, logout, vi.fn().mockResolvedValue(null)))
      .rejects.toThrow("Your session has expired. Please sign in again.");

    expect(logout).toHaveBeenCalledOnce();
  });

  it("reports network failures", async () => {
    const fetchMock = vi.fn().mockRejectedValue(new TypeError("offline"));
    vi.stubGlobal("fetch", fetchMock);

    await expect(apiRequest("/items", {}, vi.fn())).rejects.toThrow("Network error");
    expect(fetchMock).toHaveBeenCalledTimes(3);
    expect(toast.error).toHaveBeenCalledWith("Network error: Could not reach server");
  });

  it("retries a temporary gateway failure before returning recommendations", async () => {
    const fetchMock = vi.fn()
      .mockResolvedValueOnce(new Response("temporarily unavailable", { status: 503 }))
      .mockResolvedValueOnce(new Response(JSON.stringify({ data: ["track"] }), { status: 200 }));
    vi.stubGlobal("fetch", fetchMock);
    await expect(apiRequest("/library/recommendations", {}, vi.fn())).resolves.toEqual({ data: ["track"] });
    expect(fetchMock).toHaveBeenCalledTimes(2);
    expect(toast.error).not.toHaveBeenCalled();
  });

  it("reports non-JSON responses", async () => {
    vi.spyOn(console, "error").mockImplementation(() => undefined);
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response("not json", { status: 200 })));

    await expect(apiRequest("/items", {}, vi.fn())).rejects.toThrow("The server returned an unexpected response. Please try again.");
    expect(toast.error).toHaveBeenCalledWith("The server returned an unexpected response. Please try again.");
  });

  it("turns an HTML gateway timeout into a useful message", async () => {
    vi.spyOn(console, "error").mockImplementation(() => undefined);
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response("<html>Gateway timeout</html>", {
      status: 504,
      headers: { "Content-Type": "text/html" },
    })));

    await expect(apiRequest("/library/recommendations", {}, vi.fn()))
      .rejects.toThrow("The music service took too long to respond. Please try again shortly.");
    expect(toast.error).toHaveBeenCalledWith("The music service took too long to respond. Please try again shortly.");
  });
});
