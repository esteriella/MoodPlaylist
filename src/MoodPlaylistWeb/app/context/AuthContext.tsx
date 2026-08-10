"use client";

import { createContext, useCallback, useContext, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import { apiRequest } from "@/app/helpers/errorHandling";
import { config } from "@/app/helpers/config";
import type { ApiResponseModel } from "@/app/models/api-response.model";
import type { LoginResponseModel } from "@/app/models/auth.models";

type AuthContextType = {
  token: string | null;
  name: string | null;
  skipAuthToast: boolean;
  login: (token: string, name: string, refreshToken: string) => void;
  logout: () => void;
  request: <T>(url: string, options: RequestInit) => Promise<T>;
};

const AuthContext = createContext<AuthContextType | undefined>(undefined);

// ✅ Global authentication provider
export function AuthProvider({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const [token, setToken] = useState<string | null>(() => {
    if (typeof window === "undefined") return null;
    return localStorage.getItem("authToken");
  });
  const [name, setName] = useState<string | null>(() => {
    if (typeof window === "undefined") return null;
    return localStorage.getItem("authName");
  });
  const [skipAuthToast, setSkipAuthToast] = useState(false);
  const refreshPromise = useRef<Promise<string | null> | null>(null);

  const login = useCallback((newToken: string, name: string, refreshToken: string) => {
    localStorage.setItem("authToken", newToken);
    localStorage.setItem("authName", name);
    localStorage.setItem("refreshToken", refreshToken);
    setToken(newToken);
    setName(name);
    setSkipAuthToast(false);
    router.push("/dashboard");
  }, [router]);

  const logout = useCallback(() => {
    setSkipAuthToast(true);
    router.push("/auth/login");
    localStorage.removeItem("authToken");
    localStorage.removeItem("authName");
    localStorage.removeItem("refreshToken");
    setToken(null);
    setName(null);
  }, [router]);

  const refreshAccessToken = useCallback(async () => {
    if (refreshPromise.current) return refreshPromise.current;

    refreshPromise.current = (async () => {
      const storedRefreshToken = localStorage.getItem("refreshToken");
      if (!storedRefreshToken) return null;

      try {
        const response = await fetch(`${config.apiBaseUrl}/auth/refresh`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ refreshToken: storedRefreshToken }),
        });
        if (!response.ok) return null;

        const result = await response.json() as ApiResponseModel<LoginResponseModel>;
        if (!result.successful || !result.data?.token || !result.data.refreshToken) return null;

        localStorage.setItem("authToken", result.data.token);
        localStorage.setItem("refreshToken", result.data.refreshToken);
        setToken(result.data.token);
        return result.data.token;
      } catch {
        return null;
      }
    })().finally(() => {
      refreshPromise.current = null;
    });

    return refreshPromise.current;
  }, []);

  const request = useCallback(async <T,>(url: string, options: RequestInit): Promise<T> => {
    // attach token if available
    const headers = {
      ...options.headers,
      Authorization: token ? `Bearer ${token}` : "",
    };

    return apiRequest<T>(url, { ...options, headers }, logout, refreshAccessToken);
  }, [logout, refreshAccessToken, token]);

  return (
    <AuthContext.Provider value={{ token, name, skipAuthToast, login, logout, request }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}
