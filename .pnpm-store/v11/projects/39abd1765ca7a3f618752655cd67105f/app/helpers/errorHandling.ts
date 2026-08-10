import toast from "react-hot-toast";

export async function apiRequest<T>(
  url: string,
  options: RequestInit,
  logout: () => void,
  refreshAccessToken?: () => Promise<string | null>,
): Promise<T> {
  let res: Response;
  try {
    res = await fetch(url, options);
  } catch {
    toast.error("Network error: Could not reach server");
    throw new Error("Network error");
  }

  const responseText = await res.text();
  let data: { message?: string } | undefined;
  try {
    data = responseText ? JSON.parse(responseText) as { message?: string } : undefined;
  } catch {
    const message = gatewayMessage(res.status);
    console.error("Unexpected API response", {
      status: res.status,
      contentType: res.headers.get("content-type") ?? "unknown",
      path: safePath(url),
    });
    toast.error(message);
    throw new Error(message);
  }

  // Handle status codes
  if (!res.ok) {
    if (res.status === 401 && refreshAccessToken) {
      const newToken = await refreshAccessToken();
      if (newToken) {
        const headers = new Headers(options.headers);
        headers.set("Authorization", `Bearer ${newToken}`);
        return apiRequest<T>(url, { ...options, headers }, logout);
      }

      const sessionMessage = "Your session has expired. Please sign in again.";
      logout();
      toast.error(sessionMessage);
      throw new Error(sessionMessage);
    }

    const message = data?.message || `Request failed with status ${res.status}`;
    if (res.status === 401) logout();
    toast.error(message);
    throw new Error(message);
  }

  // Success toast
  if (options.method && options.method !== "GET" && data?.message && data.message !== "success") {
    toast.success(data.message);
  }

  return data as T;
}

function gatewayMessage(status: number) {
  if ([502, 503, 504].includes(status))
    return "The music service took too long to respond. Please try again shortly.";
  if (status === 404)
    return "The requested API route is unavailable. Please refresh and try again.";
  return status >= 500
    ? "The server could not complete the request. Please try again."
    : "The server returned an unexpected response. Please try again.";
}

function safePath(url: string) {
  try {
    return new URL(url, window.location.origin).pathname;
  } catch {
    return "unknown";
  }
}
