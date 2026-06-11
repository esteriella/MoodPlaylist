"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { useAuth } from "@/app/context/AuthContext";
import toast from "react-hot-toast";

export default function DashboardPage() {
  const router = useRouter();
  const { token, name, logout, skipAuthToast } = useAuth();
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!token) {
      if (!skipAuthToast) {
        toast.error("You must be logged in to access the dashboard");
      }
      router.push("/auth/login");
      return;
    }

    setLoading(false);
  }, [token, router, skipAuthToast]);

  if (loading) return null;
  if (!name) return null;

  return (
    <main className="min-h-screen p-10">
      {/* Header */}
      <header className="flex justify-between items-center mb-10">
        <h1 className="text-4xl font-bold text-white">
          Welcome back, <span className="text-pink-300">{name}</span> 🎶
        </h1>
        <div className="space-x-2">
          <Link
            href="/playlist"
            className="px-6 py-2 bg-white text-indigo-600 rounded-md hover:bg-gray-200 inline-block"
          >
            Your Playlists
          </Link>
          <button
            onClick={() => {
              logout();
              toast.success("You have been logged out");
            }}
            className="px-4 py-2 bg-white text-indigo-600 rounded-md hover:bg-gray-200"
          >
            Logout
          </button>
        </div>
      </header>

      {/* Dashboard Grid */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
        {/* Mood Selector */}
        <section className="bg-black/35 backdrop-blur-lg p-6 rounded-xl shadow-lg">
          <h2 className="text-2xl font-semibold text-white mb-4">
            Pick Your Mood
          </h2>
          <div className="flex flex-wrap gap-3">
            {["Happy", "Chill", "Focus", "Romantic", "Energetic"].map((mood) => (
              <button
                key={mood}
                className="px-4 py-2 bg-pink-500 hover:bg-pink-600 text-white rounded-lg transition"
              >
                {mood}
              </button>
            ))}
          </div>
        </section>

        {/* Playlists */}
        <section className="bg-black/35 backdrop-blur-lg p-6 rounded-xl shadow-lg md:col-span-2">
          <h2 className="text-2xl font-semibold text-white mb-4">
            Your Playlists
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {["Morning Vibes", "Workout Pump", "Late Night Chill", "Romantic Classics"].map(
              (playlist) => (
                <div
                  key={playlist}
                  className="bg-black/20 p-4 rounded-lg text-white hover:bg-white/30 transition"
                >
                  <h3 className="text-lg font-bold">{playlist}</h3>
                  <p className="text-sm text-gray-200">12 songs • curated for you</p>
                </div>
              )
            )}
          </div>
        </section>

        {/* Recommendations */}
        <section className="bg-black/35 backdrop-blur-lg p-6 rounded-xl shadow-lg md:col-span-3">
          <h2 className="text-2xl font-semibold text-white mb-4">
            Recommended for You
          </h2>
          <div className="flex flex-wrap gap-6">
            {["Lo-fi Beats", "Afro Pop Hits", "Jazz Essentials", "Indie Discoveries"].map((rec) => (
              <div
                key={rec}
                className="bg-black/25 p-4 rounded-lg text-white hover:bg-white/30 transition w-48"
              >
                <h3 className="font-bold">{rec}</h3>
                <p className="text-sm text-gray-200">Playlist • 20 songs</p>
              </div>
            ))}
          </div>
        </section>
      </div>
    </main>
  );
}
