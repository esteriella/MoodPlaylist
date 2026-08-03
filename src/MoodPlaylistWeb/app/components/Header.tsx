"use client";

import Link from "next/link";
import { useState } from "react";
import { MenuIcon } from "@/app/components/Icons";

export default function Header() {
  const [menuOpen, setMenuOpen] = useState(false);

  return (
    <header className="relative z-30 flex items-center justify-between px-6 py-5 md:px-20">
      <Link href="/" className="brand text-white">
        <span className="brand-mark">M</span>
        <span>MoodPlaylist</span>
      </Link>
      <nav className="hidden items-center gap-3 text-sm font-bold md:flex" aria-label="Main navigation">
        <Link href="/auth/login" className="rounded-full px-4 py-2 text-white/80 transition hover:bg-white/10 hover:text-white">Sign in</Link>
        <Link href="/auth/register" className="rounded-full bg-white px-5 py-2.5 text-[#5d35a4] transition hover:-translate-y-0.5">Get started</Link>
      </nav>
      <button
        type="button"
        className="grid h-11 w-11 place-items-center rounded-xl border border-white/20 bg-white/10 text-white backdrop-blur transition hover:bg-white/20 md:hidden"
        aria-label={menuOpen ? "Close navigation menu" : "Open navigation menu"}
        aria-expanded={menuOpen}
        aria-controls="mobile-main-navigation"
        onClick={() => setMenuOpen((open) => !open)}
      >
        <MenuIcon open={menuOpen} />
      </button>
      {menuOpen && (
        <nav id="mobile-main-navigation" className="absolute left-6 right-6 top-19.5 flex flex-col gap-2 rounded-2xl border border-white/15 bg-[#2d1d40]/95 p-3 text-sm font-bold shadow-2xl backdrop-blur-xl md:hidden" aria-label="Mobile navigation">
          <Link href="/auth/login" onClick={() => setMenuOpen(false)} className="rounded-xl px-4 py-3 text-white/85 transition hover:bg-white/10 hover:text-white">Sign in</Link>
          <Link href="/auth/register" onClick={() => setMenuOpen(false)} className="rounded-xl bg-white px-4 py-3 text-center text-[#5d35a4]">Get started</Link>
        </nav>
      )}
    </header>
  );
}
