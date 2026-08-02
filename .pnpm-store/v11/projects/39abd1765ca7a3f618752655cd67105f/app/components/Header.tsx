import Link from "next/link";

export default function Header() {
  return (
    <header className="flex items-center justify-between px-6 py-5 md:px-20">
      <Link href="/" className="brand text-white">
        <span className="brand-mark">M</span>
        <span>MoodPlaylist</span>
      </Link>
      <nav className="flex items-center gap-3 text-sm font-bold">
        <Link href="/auth/login" className="rounded-full px-4 py-2 text-white/80 transition hover:bg-white/10 hover:text-white">Sign in</Link>
        <Link href="/auth/register" className="rounded-full bg-white px-5 py-2.5 text-[#5d35a4] transition hover:-translate-y-0.5">Get started</Link>
      </nav>
    </header>
  );
}
