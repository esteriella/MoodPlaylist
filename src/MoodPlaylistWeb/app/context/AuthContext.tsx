"use client";

import { createContext, useContext, useState } from "react";
import { useRouter } from "next/navigation";
import { apiRequest } from "@/app/helpers/errorHandling";

type AuthContextType = {
  token: string | null;
  name: string | null;
  skipAuthToast: boolean;
  login: (token: string, name: string) => void;
  logout: () => void;
  request: <T>(url: string, options: RequestInit) => Promise<T>;
};

const AuthContext = createContext<AuthContextType | undefined>(undefined);

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

  const login = (newToken: string, name: string) => {
    localStorage.setItem("authToken", newToken);
    localStorage.setItem("authName", name);
    setToken(newToken);
    setName(name);
    setSkipAuthToast(false);
    router.push("/dashboard");
  };

  const logout = () => {
    setSkipAuthToast(true);
    router.push("/auth/login");
    localStorage.removeItem("authToken");
    localStorage.removeItem("authName");
    setToken(null);
    setName(null);
  };

  // ✅ Global request wrapper with auto logout
  const request = async <T,>(url: string, options: RequestInit): Promise<T> => {
    // attach token if available
    const headers = {
      ...options.headers,
      Authorization: token ? `Bearer ${token}` : "",
    };

    return apiRequest<T>(url, { ...options, headers }, logout);
  };

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
