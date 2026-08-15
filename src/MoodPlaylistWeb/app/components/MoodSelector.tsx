"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import toast from "react-hot-toast";
import { libraryApi } from "@/app/api/library";
import { useAuth } from "@/app/context/AuthContext";
import { Mood } from "@/app/models/library.models";

export default function MoodSelector() {
  const { token, request } = useAuth();
  const router = useRouter();
  const [moods, setMoods] = useState<Mood[]>([]);

  useEffect(() => {
    const timer = window.setTimeout(() => {
      libraryApi.moods(request).then((result) => setMoods(result.data.slice(0, 4))).catch(() => undefined);
    }, 0);
    return () => window.clearTimeout(timer);
  }, [request]);

  const chooseMood = (mood: Mood) => {
    if (!token) {
      toast(`Create an account to build a ${mood.name.toLowerCase()} mix`);
      router.push(`/auth/register?mood=${encodeURIComponent(mood.name)}`);
      return;
    }
    router.push(`/dashboard?moodId=${mood.id}`);
  };

  return (
    <section className="bg-[#f8f5ef] px-6 pb-24 pt-4 text-[#1c1722] md:px-12 md:pb-32">
      <div className="mx-auto max-w-6xl">
        <div className="mb-10 flex flex-col justify-between gap-5 md:flex-row md:items-end">
          <div><p className="mb-3 text-xs font-black uppercase tracking-[.22em] text-[#8051d0]">Try a feeling</p><h2 className="font-serif text-4xl tracking-tight md:text-5xl">Where are you today?</h2></div>
          <p className="max-w-md text-sm leading-6 text-[#77707d]">Tap one mood to begin and find tracks shaped around that feeling.</p>
        </div>
        <div className="grid grid-cols-2 gap-3 md:grid-cols-4 md:gap-5">
          {moods.map((mood, index) => (
            <button key={mood.id} onClick={() => chooseMood(mood)} className={`landing-mood-card landing-mood-${index}`}>
              <span className="text-4xl md:text-5xl">{mood.emoji || "♪"}</span>
              <span><strong>{mood.name}</strong><small>Explore this mood</small></span>
              <i>↗</i>
            </button>
          ))}
          {!moods.length && [0, 1, 2, 3].map((item) => <div key={item} className="h-48 animate-pulse rounded-3xl bg-black/5" />)}
        </div>
      </div>
    </section>
  );
}
