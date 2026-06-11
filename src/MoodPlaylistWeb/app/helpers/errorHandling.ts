import toast from "react-hot-toast";

export async function apiRequest<T>(url: string, options: RequestInit, logout: () => void): Promise<T> {
  let res: Response;
  try {
    res = await fetch(url, options);
  } catch {
    toast.error("Network error: Could not reach server");
    throw new Error("Network error");
  }

  let data: any;
  try {
    data = await res.json();
  } catch {
    toast.error("Server did not return JSON");
    throw new Error("Invalid JSON response");
  }

  // Handle status codes
  if (!res.ok) {
    const message = data?.message || `Request failed with status ${res.status}`;
    toast.error(message);
    throw new Error(message);
  }

  // Success toast
  if (data?.message) {
    toast.success(data.message);
  }

  return data as T;
}
