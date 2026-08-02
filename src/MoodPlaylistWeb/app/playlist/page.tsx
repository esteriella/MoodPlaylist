"use client";

import Image from "next/image";
import { useRouter } from "next/navigation";

const playlist = {
  title: "Morning Vibes",
  description: "Start your day with uplifting tunes that bring a little more energy to the morning.",
  cover: "/albums/feel-good.jpg",
  tracks: [
    { title: "Sunrise Dreams", artist: "Lo-Fi Collective", duration: "3:45" },
    { title: "Coffee & Chill", artist: "Indie Beats", duration: "4:12" },
    { title: "Positive Energy", artist: "Afro Pop Crew", duration: "2:58" },
    { title: "Focus Flow", artist: "Jazz Essentials", duration: "5:01" },
  ],
};

export default function PlaylistPage() {
  const router = useRouter();

  return (
    <main className="app-shell min-h-screen px-5 py-8 md:px-12 md:py-12">
      <div className="mx-auto max-w-5xl">
        <header className="mb-10 flex items-center justify-between gap-5">
          <div><p className="eyebrow">Playlist preview</p><h1 className="font-serif text-4xl tracking-tight md:text-5xl">{playlist.title}</h1></div>
          <button onClick={() => router.push("/dashboard")} className="quiet-button bg-white">Back to dashboard</button>
        </header>

        <section className="mb-8 grid gap-7 rounded-3xl border border-black/10 bg-white/70 p-5 shadow-xl shadow-purple-950/5 backdrop-blur-md md:grid-cols-[220px_1fr] md:p-7">
          <Image src={playlist.cover} alt="Feel Good album artwork" width={440} height={440} className="aspect-square w-full rounded-2xl object-cover" />
          <div className="flex flex-col justify-center">
            <p className="mb-6 max-w-xl leading-7 text-[#746e78]">{playlist.description}</p>
            <span className="w-fit rounded-full bg-[#eee7fb] px-4 py-2 text-xs font-bold text-[#7047bd]">{playlist.tracks.length} tracks</span>
          </div>
        </section>

        <section className="overflow-hidden rounded-2xl border border-black/10 bg-white/75">
          <div className="border-b border-black/10 px-5 py-4"><h2 className="font-serif text-2xl">Tracks</h2></div>
          <ol>
            {playlist.tracks.map((track, index) => (
              <li key={track.title} className="grid grid-cols-[32px_1fr_auto] items-center gap-3 border-b border-black/5 px-5 py-4 last:border-0 hover:bg-purple-50/60">
                <span className="font-mono text-xs text-[#aaa3ad]">{String(index + 1).padStart(2, "0")}</span>
                <div><p className="text-sm font-bold text-[#211c25]">{track.title}</p><p className="mt-1 text-xs text-[#827b86]">{track.artist}</p></div>
                <span className="text-xs text-[#827b86]">{track.duration}</span>
              </li>
            ))}
          </ol>
        </section>
      </div>
    </main>
  );
}
