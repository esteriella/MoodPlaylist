import Link from "next/link";

export default function Header() {
  return (
    <header className="flex justify-between items-center py-4 px-20">
      <h1 className="text-2xl font-bold text-white flex items-center gap-2">
        <span className="text-pink-400">❤️</span> MoodPlaylist 🎵
      </h1>
      <div className="space-x-2">
        <Link
          href="/auth/login"
          className="px-6 py-2 bg-white text-indigo-600 rounded-md hover:bg-gray-200"
        >
          Login
        </Link>
        <Link
          href="/auth/register"
          className="px-6 py-2 bg-white text-indigo-600 rounded-md hover:bg-gray-200 inline-block"
        >
          Register
        </Link>
        {/* <Link
          href="/dashboard"
          className="px-6 py-2 bg-white text-indigo-600 rounded-md hover:bg-gray-200 inline-block"
        >
          Dashboard
        </Link> */}
      </div>
    </header>
  );
}


