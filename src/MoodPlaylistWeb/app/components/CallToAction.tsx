"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";

export default function CallToAction() {
  const router = useRouter();

  return (
    <section className="relative text-center overflow-hidden py-16 ">
        <div className="absolute top-0 left-0 w-full overflow-hidden leading-0">
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
      <h2 className="text-3xl font-bold text-white mb-4">
        Ready to Personalize Your Music?
      </h2>
      <p className="text-gray-100 mb-6">
        Join now and unlock playlists for every mood.
      </p>
      <Link href="/auth/register" className="bg-white text-pink-600 font-bold px-8 py-3 rounded-full hover:scale-105 transition">
        Register Free
      </Link>

      {/* <div className="absolute bottom-0 left-0 w-full overflow-hidden leading-0 rotate-180">
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
      </div> */}
    </section>
  );
}
