"use client";

import Image from "next/image";
import Link from "next/link";
import { useRouter } from "next/navigation";

export default function HeroSection() {
  const router = useRouter();

  return (
    <section
      id="hero"
      className="flex flex-col md:flex-row items-center justify-center py-2 px-10 relative top-0 left-0 overflow-hidden leading-0"
    >
      <div className="text-center md:text-left md:w-1/2">
        <h1 className="text-6xl font-extrabold text-white mb-6">
          Music that Matches Your Mood
        </h1>
        <p className="text-xl text-gray-100 mb-12 max-w-lg">
          Pick how you feel, and we’ll craft the perfect playlist for you.
        </p>
        <Link href="/auth/register" className="bg-white text-purple-600 font-bold px-8 py-5 rounded-full hover:scale-105 transition">
          Get Started
        </Link>
      </div>
      <div className="md:w-1/2 mt-10 md:mt-0">
        <img src="/albums/lady-removebg.png" alt="Listening to music" />
      </div>
      <div className="absolute bottom-0 left-0 w-full overflow-hidden leading-0 rotate-180">
        <svg
          className="relative block w-full h-20"
          xmlns="http://www.w3.org/2000/svg"
          preserveAspectRatio="none"
          viewBox="0 0 1200 120"
        >
          <path
            d="M0,0 C300,100 900,0 1200,100 L1200,0 L0,0 Z"
            className="fill-white"
          ></path>
        </svg>
      </div>
    </section>
  );
}
