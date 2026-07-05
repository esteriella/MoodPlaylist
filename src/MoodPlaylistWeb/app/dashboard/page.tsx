// "use client";

// import { useEffect, useState } from "react";
// import { useRouter } from "next/navigation";
// import Link from "next/link";
// import { useAuth } from "@/app/context/AuthContext";
// import toast from "react-hot-toast";

// export default function DashboardPage() {
//   const router = useRouter();
//   const { token, name, logout, skipAuthToast } = useAuth();
//   const [loading, setLoading] = useState(true);

//   useEffect(() => {
//     if (!token) {
//       if (!skipAuthToast) {
//         toast.error("You must be logged in to access the dashboard");
//       }
//       router.push("/auth/login");
//       return;
//     }

//     setLoading(false);
//   }, [token, router, skipAuthToast]);

//   if (loading) return null;
//   if (!name) return null;

//   return (
//     <main className="min-h-screen p-10">
//       {/* Header */}
//       <header className="flex justify-between items-center mb-10">
//         <h1 className="text-4xl font-bold text-white">
//           Welcome back, <span className="text-pink-300">{name}</span> 🎶
//         </h1>
//         <div className="space-x-2">
//           <Link
//             href="/playlist"
//             className="px-6 py-2 bg-white text-indigo-600 rounded-md hover:bg-gray-200 inline-block"
//           >
//             Your Playlists
//           </Link>
//           <button
//             onClick={() => {
//               logout();
//               toast.success("You have been logged out");
//             }}
//             className="px-4 py-2 bg-white text-indigo-600 rounded-md hover:bg-gray-200"
//           >
//             Logout
//           </button>
//         </div>
//       </header>

//       {/* Dashboard Grid */}
//       <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
//         {/* Mood Selector */}
//         <section className="bg-black/35 backdrop-blur-lg p-6 rounded-xl shadow-lg">
//           <h2 className="text-2xl font-semibold text-white mb-4">
//             Pick Your Mood
//           </h2>
//           <div className="flex flex-wrap gap-3">
//             {["Happy", "Chill", "Focus", "Romantic", "Energetic"].map((mood) => (
//               <button
//                 key={mood}
//                 className="px-4 py-2 bg-pink-500 hover:bg-pink-600 text-white rounded-lg transition"
//               >
//                 {mood}
//               </button>
//             ))}
//           </div>
//         </section>

//         {/* Playlists */}
//         <section className="bg-black/35 backdrop-blur-lg p-6 rounded-xl shadow-lg md:col-span-2">
//           <h2 className="text-2xl font-semibold text-white mb-4">
//             Your Playlists
//           </h2>
//           <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
//             {["Morning Vibes", "Workout Pump", "Late Night Chill", "Romantic Classics"].map(
//               (playlist) => (
//                 <div
//                   key={playlist}
//                   className="bg-black/20 p-4 rounded-lg text-white hover:bg-white/30 transition"
//                 >
//                   <h3 className="text-lg font-bold">{playlist}</h3>
//                   <p className="text-sm text-gray-200">12 songs • curated for you</p>
//                 </div>
//               )
//             )}
//           </div>
//         </section>

//         {/* Recommendations */}
//         <section className="bg-black/35 backdrop-blur-lg p-6 rounded-xl shadow-lg md:col-span-3">
//           <h2 className="text-2xl font-semibold text-white mb-4">
//             Recommended for You
//           </h2>
//           <div className="flex flex-wrap gap-6">
//             {["Lo-fi Beats", "Afro Pop Hits", "Jazz Essentials", "Indie Discoveries"].map((rec) => (
//               <div
//                 key={rec}
//                 className="bg-black/25 p-4 rounded-lg text-white hover:bg-white/30 transition w-48"
//               >
//                 <h3 className="font-bold">{rec}</h3>
//                 <p className="text-sm text-gray-200">Playlist • 20 songs</p>
//               </div>
//             ))}
//           </div>
//         </section>
//       </div>
//     </main>
//   );
// }

"use client";

import { useState, useEffect } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { toast } from "react-hot-toast";
import Header from "@/app/components/Header";
import { useAuth } from "@/app/context/AuthContext" // adjust path to your project
import { config } from "../helpers/config";

export default function DashboardPage() {
  const router = useRouter();
  const { token, name, logout, skipAuthToast } = useAuth();

  // ✅ all useState hooks at the top
  const [loading, setLoading] = useState(true);
  const [moods, setMoods] = useState<any[]>([]);
  const [playlists, setPlaylists] = useState<any[]>([]);
  const [recommendations, setRecommendations] = useState<any[]>([]);
  const [newPlaylist, setNewPlaylist] = useState("");
  const [creating, setCreating] = useState(false);

  // ✅ useEffect hooks also at the top
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

  useEffect(() => {
    async function fetchData() {
      try {
        const moodsRes = await fetch(`${config.apiBaseUrl}/library/available-moods`);
        const playlistsRes = await fetch(`${config.apiBaseUrl}/library/playlists`);
        const recRes = await fetch(`${config.apiBaseUrl}/library/recommendations`);

        const moodsData = await moodsRes.json();
        setMoods(moodsData.data);
        const playlistsData = await playlistsRes.json();
        setPlaylists(playlistsData.data);
        const recData = await recRes.json();
        setRecommendations(recData.data);
      } catch {
        toast.error("Failed to load dashboard data");
      }
    }
    if (token) fetchData();
  }, [token]);

  // ✅ only after all hooks, you can conditionally return
  if (loading) return null;
  if (!name) return null;

  // Mood click
  const handleMoodClick = (mood: { name: string }) => {
    toast.success(`Mood selected: ${mood.name}`);
  };

  // Create playlist
  const handleCreatePlaylist = async (
    e: React.FormEvent<HTMLFormElement>,
    moodId?: string, // optional moodId if user picked a mood
  ) => {
    e.preventDefault();
    if (!newPlaylist) return;
    setCreating(true);

    const upsert = {
      title: newPlaylist,
      moodId: moodId, // ✅ include moodId when available
      tracks: JSON.stringify([]), // empty playlist initially
    };

    const res = await fetch(`${config.apiBaseUrl}/library/playlists`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(upsert),
    });

    if (res.ok) {
      const created = await res.json();
      setPlaylists([...playlists, created]);
      setNewPlaylist("");
      toast.success("Playlist created!");
    } else {
      toast.error("Failed to create playlist");
    }
    setCreating(false);
  };


  // Edit playlist
  const handleEditPlaylist = async (playlist: any) => {
    const newTitle = prompt("Enter new playlist title:", playlist.title);
    if (!newTitle) return;

    const upsert = {
      title: newTitle,
      moodId: playlist.mood?.id,
      tracks: JSON.stringify(playlist.Tracks),
    };

    const res = await fetch(`${config.apiBaseUrl}/library/playlists/${playlist.id}`, {
      method: "PUT",
      body: JSON.stringify(upsert),
    });

    if (res.ok) {
      setPlaylists(
        playlists.map((p) =>
          p.title === playlist.title ? { ...p, title: newTitle } : p
        )
      );
      toast.success("Playlist updated!");
    } else {
      toast.error("Failed to update playlist");
    }
  };

  // Delete playlist
  const handleDeletePlaylist = async (playlist: any) => {
    const res = await fetch(`${config.apiBaseUrl}/library/playlists/${playlist.id}`, {
      method: "DELETE",
    });
    if (res.ok) {
      setPlaylists(playlists.filter((p) => p.id !== playlist.id));
      toast.success("Playlist deleted!");
    } else {
      toast.error("Failed to delete playlist");
    }
  };

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
            {moods.map((mood) => (
              <button
                key={mood.id}
                onClick={() => handleMoodClick(mood)}
                style={{ backgroundColor: mood.color }}
                className="px-4 py-2 rounded-lg text-white shadow hover:scale-105 transition"
              >
                {mood.emoji} {mood.name}
              </button>
            ))}
          </div>
        </section>

        {/* Manage Playlists */}
        <section className="bg-black/35 backdrop-blur-lg p-6 rounded-xl shadow-lg">
          <h2 className="text-2xl font-semibold text-white mb-4">
            Manage Your Playlists
          </h2>

          {/* Create Playlist */}
          <form onSubmit={handleCreatePlaylist} className="flex gap-3 mb-6">
            <input
              type="text"
              placeholder="New playlist title"
              value={newPlaylist}
              onChange={(e) => setNewPlaylist(e.target.value)}
              className="flex-1 px-3 py-2 rounded-lg bg-white/20 text-white focus:outline-none"
            />
            <button
              type="submit"
              disabled={creating}
              className="px-4 py-2 bg-pink-500 hover:bg-pink-600 text-white rounded-lg"
            >
              {creating ? "Adding..." : "Add"}
            </button>
          </form>

          {/* Existing Playlists with Edit/Delete */}
          <div className="space-y-4">
            {playlists.map((playlist) => (
              <div
                key={playlist.title}
                className="bg-black/20 p-4 rounded-lg text-white flex justify-between items-center"
              >
                <div>
                  <h3 className="text-lg font-bold">{playlist.title}</h3>
                  <p className="text-sm text-gray-200">
                    {playlist.Tracks.length} tracks • by {playlist.creatorName}
                  </p>
                </div>
                <div className="space-x-2">
                  <button
                    onClick={() => handleEditPlaylist(playlist)}
                    className="px-3 py-1 bg-indigo-500 hover:bg-indigo-600 rounded-lg text-sm"
                  >
                    Edit
                  </button>
                  <button
                    onClick={() => handleDeletePlaylist(playlist.title)}
                    className="px-3 py-1 bg-red-500 hover:bg-red-600 rounded-lg text-sm"
                  >
                    Delete
                  </button>
                </div>
              </div>
            ))}
          </div>
        </section>

        {/* Your Playlists */}
        <section className="bg-black/35 backdrop-blur-lg p-6 rounded-xl shadow-lg">
          <h2 className="text-2xl font-semibold text-white mb-4">
            Your Playlists
          </h2>
          <div className="space-y-4">
            {playlists.map((playlist) => (
              <div
                key={playlist.title}
                className="bg-black/20 p-4 rounded-lg text-white hover:bg-white/30 transition"
              >
                <h3 className="text-lg font-bold">{playlist.title}</h3>
                <p className="text-sm text-gray-200">
                  {playlist.Tracks.length} tracks • by {playlist.creatorName}
                </p>
              </div>
            ))}
          </div>
        </section>

        

        {/* Recommendations */}
        <section className="bg-black/35 backdrop-blur-lg mb-4 p-6 rounded-xl shadow-lg md:col-span-3">
          <h2 className="text-2xl font-semibold text-white mb-4">
            Recommended for You
          </h2>
          <div className="flex flex-wrap gap-6">
            {recommendations.length > 0
              ? recommendations.map((rec) => (
                  <div
                    key={rec.title}
                    className="bg-black/25 p-4 rounded-lg text-white hover:bg-white/30 transition w-48"
                  >
                    <h3 className="font-bold">{rec.title}</h3>
                    <p className="text-sm text-gray-200">
                      Playlist • {rec.Tracks?.length || 0} songs
                    </p>
                  </div>
                ))
              : [
                  "Lo-fi Beats",
                  "Afro Pop Hits",
                  "Jazz Essentials",
                  "Indie Discoveries",
                ].map((rec) => (
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

