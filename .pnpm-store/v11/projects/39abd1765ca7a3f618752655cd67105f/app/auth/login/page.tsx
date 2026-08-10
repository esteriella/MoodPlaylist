"use client";

import { useState } from "react";
import Link from "next/link";
import { loginUser } from "@/app/api/auth";
import { AuthField, AuthFrame } from "@/app/components/AuthFrame";
import { useAuth } from "@/app/context/AuthContext";
import { LoginSchema } from "@/app/models/auth.models";

export default function LoginPage() {
  const { login } = useAuth();
  const [form, setForm] = useState({ email: "", password: "" });
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const change = (event: React.ChangeEvent<HTMLInputElement>) =>
    setForm((current) => ({ ...current, [event.target.name]: event.target.value }));

  const submit = async (event: React.SubmitEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError("");
    const checked = LoginSchema.safeParse(form);
    if (!checked.success) return setError(checked.error.issues[0]?.message ?? "Check your details");

    setLoading(true);
    try {
      const response = await loginUser(checked.data);
      login(response.data.token, response.data.name, response.data.refreshToken);
    } catch (reason: unknown) {
      setError(reason instanceof Error ? reason.message : "Unable to sign in");
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthFrame eyebrow="Welcome back" title="Find your way back to the music." description="Sign in to open your playlists and discover something new." footer={<>New here? <Link href="/auth/register">Create an account</Link></>}>
      <form onSubmit={submit} className="auth-form">
        <AuthField label="Email address" name="email" type="email" value={form.email} placeholder="you@example.com" autoComplete="email" onChange={change} />
        <AuthField label="Password" name="password" type="password" value={form.password} placeholder="Enter your password" autoComplete="current-password" onChange={change} />
        {error && <p className="auth-error" role="alert">{error}</p>}
        <button className="auth-submit" type="submit" disabled={loading}>{loading ? <><i className="button-spinner" /> Signing in…</> : "Sign in"}</button>
      </form>
    </AuthFrame>
  );
}
