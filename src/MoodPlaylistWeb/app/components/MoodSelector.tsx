"use client";

import { useEffect, useState } from "react";
import { config } from "@/app/helpers/config";
import { useAuth } from "@/app/context/AuthContext"; // custom hook to access auth context
import { useRouter } from "next/navigation";
import toast from "react-hot-toast";

type Mood = {
  name: string;
  color: string;
  emoji: string;
};

export default function MoodSelector() {
  const { token } = useAuth();
  const router = useRouter();
  const [moods, setMoods] = useState<Mood[]>([]);

  useEffect(() => {
    const fetchMoods = async () => {
      try {
        const res = await fetch(`${config.apiBaseUrl}/library/available-moods`);
        const data = await res.json();
        setMoods(data.data); // assuming ApiResponseModel
      } catch {
        toast.error("Failed to load moods");
      }
    };
    fetchMoods();
  }, []);

  // always show random subset
  const displayedMoods = moods.sort(() => 0.5 - Math.random()).slice(0, 4);

  const handleMoodClick = (mood: Mood) => {
    if (!token) {
      toast("Please register to personalize your dashboard");
      router.push(`/auth/register?mood=${mood.name}`);
    } else {
      router.push(`/dashboard?mood=${mood.name}`);
    }
  };

  return (
    <section className="relative overflow-hidden text-center mt-10 pb-10 px-16">
      <h2 className="text-4xl text-white font-bold mb-2">Choose Your Mood</h2>
      <p className="text-gray-200 mb-8">
        Select a mood to get a playlist tailored for you.
      </p>
      <div className="grid grid-cols-2 md:grid-cols-4 mb-20 gap-6 justify-center">
        {displayedMoods.map((mood) => (
          // <button
          //   key={mood.name}
          //   onClick={() => handleMoodClick(mood)}
          //   style={{ backgroundColor: mood.color }}
          //   className="py-12 rounded-xl shadow-2xl hover:scale-105 transition transform"
          // >
          //   <span className="text-8xl">{mood.emoji}</span>
          //   <p className="mt-2 font-bold text-2xl text-white">{mood.name}</p>
          // </button>

          <button
            key={mood.name}
            onClick={() => handleMoodClick(mood)}
            style={{ backgroundColor: mood.color }}
            className="py-12 rounded-xl shadow-2xl hover:scale-105 transition transform text-white"
          >
            <span className="text-8xl">{mood.emoji}</span>
            <p className="mt-2 font-bold text-2xl">{mood.name}</p>
          </button>
        ))}
      </div>
      {/* <div className="border-t border-gray-400 mt-16" /> */}
      <div className="absolute bottom-0 left-0 w-full overflow-hidden leading-0 rotate-180">
        <svg
          className="relative block w-full h-20"
          xmlns="http://www.w3.org/2000/svg"
          preserveAspectRatio="none"
          viewBox="0 0 1200 120"
        >
          <path
            d="M0,0 C300,100 900,0 1200,100 L1200,0 L0,0 Z"
            className="fill-black/70"
          ></path>
        </svg>
      </div>
    </section>
  );
}
