"use client";

import { useState } from "react";
import Link from "next/link";
import { registerUser } from "@/app/api/auth";
import { AuthField, AuthFrame } from "@/app/components/AuthFrame";
import { useAuth } from "@/app/context/AuthContext";
import { RegisterSchema } from "@/app/models/auth.models";

export default function RegisterPage() {
  const { login } = useAuth();
  const [form, setForm] = useState({ name: "", email: "", password: "", confirmPassword: "" });
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const change = (event: React.ChangeEvent<HTMLInputElement>) =>
    setForm((current) => ({ ...current, [event.target.name]: event.target.value }));

  const submit = async (event: React.SubmitEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError("");
    if (form.password !== form.confirmPassword) return setError("Passwords do not match");
    const checked = RegisterSchema.safeParse({ name: form.name, email: form.email, password: form.password });
    if (!checked.success) return setError(checked.error.issues[0]?.message ?? "Check your details");

    setLoading(true);
    try {
      const response = await registerUser(checked.data);
      login(response.data.token, response.data.name);
    } catch (reason: unknown) {
      setError(reason instanceof Error ? reason.message : "Unable to create your account");
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthFrame eyebrow="Create your space" title="Start with a feeling. Keep the soundtrack." description="Join MoodPlaylist and make every mix a little more personal." footer={<>Already have an account? <Link href="/auth/login">Sign in</Link></>}>
      <form onSubmit={submit} className="auth-form">
        <AuthField label="Your name" name="name" value={form.name} placeholder="What should we call you?" autoComplete="name" onChange={change} />
        <AuthField label="Email address" name="email" type="email" value={form.email} placeholder="you@example.com" autoComplete="email" onChange={change} />
        <div className="auth-field-row">
          <AuthField label="Password" name="password" type="password" value={form.password} placeholder="8–12 characters" autoComplete="new-password" onChange={change} />
          <AuthField label="Confirm password" name="confirmPassword" type="password" value={form.confirmPassword} placeholder="Repeat password" autoComplete="new-password" onChange={change} />
        </div>
        <p className="password-note">Use uppercase, lowercase, a number, and a special character.</p>
        {error && <p className="auth-error" role="alert">{error}</p>}
        <button className="auth-submit" type="submit" disabled={loading}>{loading ? <><i className="button-spinner" /> Creating account…</> : "Create my account"}</button>
      </form>
    </AuthFrame>
  );
}
