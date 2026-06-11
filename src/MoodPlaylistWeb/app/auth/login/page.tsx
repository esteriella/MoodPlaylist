"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { useAuth } from "@/app/context/AuthContext";
import { EmailIcon, LockIcon } from "@/app/components/Icons";
import Loader from "@/app/components/Loader";
import { loginUser } from "@/app/api/auth"; // ✅ import API function

export default function LoginPage() {
  const router = useRouter();
  const { login } = useAuth();
  const [form, setForm] = useState({ email: "", password: "" });
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e: React.SubmitEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError("");

    setLoading(true);
    try {
      const response = await loginUser(form); // 🔗 call API helper
      const token = response.data.token;
      const name = response.data.name;
      login(token, name);
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <main className="min-h-screen w-full flex">
      {/* Left side: illustration */}
      <div className="flex-1 flex items-center justify-center">
        <img
          src="/albums/auth-art.png"
          alt="MoodPlaylist illustration"
          className="3xl min-h-screen"
        />
      </div>

      {/* Right side: login form */}
      <div className="flex-1 flex items-center justify-center bg-linear-to-br from-[#a3366d] via-[#5f41a6] to-[#353680]">
        <div className="bg-black/35 backdrop-blur-lg min-h-screen p-40 rounded-2xl shadow-xl w-full max-w-full">
          <div className="flex items-center justify-center mb-6">
            <span className="text-pink-400 text-2xl">❤️</span>
            <h1 className="text-2xl font-bold text-white ml-2">
              MoodPlaylist <span className="text-pink-300">🎵</span>
            </h1>
          </div>

          <h2 className="text-3xl font-bold text-white mb-6 text-center">
            Login to MoodPlaylist
          </h2>

          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="flex items-center px-3 gap-2 py-2 rounded-lg bg-white/20 text-white">
              <EmailIcon />
              <input
                type="email"
                name="email"
                placeholder="Email"
                value={form.email}
                onChange={handleChange}
                className="flex-1 bg-transparent focus:outline-none"
              />
            </div>
            <div className="flex items-center px-3 gap-2 py-2 rounded-lg bg-white/20 text-white">
              <LockIcon />
              <input
                type="password"
                name="password"
                placeholder="Password"
                value={form.password}
                onChange={handleChange}
                className="flex-1 bg-transparent focus:outline-none"
              />
            </div>
            {loading ? (
              <div className="flex justify-center">
                <Loader />
              </div>
            ) : (
              <button
                type="submit"
                className="w-full py-2 bg-pink-500 hover:bg-pink-600 text-white font-semibold rounded-lg transition"
              >
                Login
              </button>
            )}
          </form>

          {error && <p className="text-red-400 mt-4 text-center">{error}</p>}

          <p className="text-center text-gray-200 mt-6">
            Don’t have an account?{" "}
            <Link href="/auth/register" className="text-pink-300 hover:underline">
              Register here
            </Link>
          </p>
        </div>
      </div>
    </main>
  );
}
