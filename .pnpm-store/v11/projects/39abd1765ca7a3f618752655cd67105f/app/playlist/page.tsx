"use client";
import { useRouter } from "next/navigation";

export default function PlaylistPage() {
  const router = useRouter();

  // Fake playlist data — replace with API call
  const playlist = {
    title: "Morning Vibes 🌅",
    description: "Start your day with uplifting tunes to energize your mood.",
    cover: "/albums/feel-good.jpg", // replace with real cover image
    tracks: [
      { title: "Sunrise Dreams", artist: "Lo-Fi Collective", duration: "3:45" },
      { title: "Coffee & Chill", artist: "Indie Beats", duration: "4:12" },
      { title: "Positive Energy", artist: "Afro Pop Crew", duration: "2:58" },
      { title: "Focus Flow", artist: "Jazz Essentials", duration: "5:01" },
    ],
  };

  return (
    <main className="min-h-screen p-10">
      {/* Header */}
      <header className="flex justify-between items-center mb-10">
        <h1 className="text-4xl font-bold text-white">{playlist.title}</h1>
        <button
          onClick={() => router.push("/dashboard")}
          className="px-4 py-2 bg-white text-indigo-600 rounded-md hover:bg-gray-200"
        >
          Back to Dashboard
        </button>
      </header>

      {/* Playlist Info */}
      <section className="flex flex-col md:flex-row gap-8 mb-10">
        <img
          src={playlist.cover}
          alt="Playlist cover"
          className="w-48 h-48 rounded-xl shadow-2xl object-cover"
        />
        <div className="flex flex-col justify-center">
          <p className="text-gray-200 mb-4">{playlist.description}</p>
          <button className="px-6 py-2 bg-pink-500 hover:bg-pink-600 text-white rounded-lg transition w-fit">
            Play All ▶️
          </button>
        </div>
      </section>

      {/* Track List */}
      <section className="bg-black/35 backdrop-blur-lg p-6 rounded-xl shadow-lg">
        <h2 className="text-2xl font-semibold text-white mb-4">Tracks</h2>
        <ul className="divide-y divide-white/20">
          {playlist.tracks.map((track, idx) => (
            <li
              key={idx}
              className="flex justify-between items-center py-3 text-white hover:bg-white/10 px-2 rounded-lg transition"
            >
              <div>
                <p className="font-bold">{track.title}</p>
                <p className="text-sm text-gray-300">{track.artist}</p>
              </div>
              <div className="flex items-center gap-4">
                <span className="text-gray-300">{track.duration}</span>
                <button className="px-3 py-1 bg-pink-500 hover:bg-pink-600 rounded-md text-sm">
                  ▶️ Play
                </button>
              </div>
            </li>
          ))}
        </ul>
      </section>
    </main>
  );
}
