export default function Footer() {
  return (
    <footer className="mt-8 px-6 text-center flex flex-row justify-between text-sm text-gray-200">
      <div className="space-x-4 mb-2 font-bold">
        <a href="#">About</a>
        <a href="#">Support</a>
        <a href="#">Terms</a>
        <a href="#">Privacy</a>
      </div>
      <p className="font-bold">
        © 2026 MoodPlaylist. Built with <span className="text-indigo-300">Next.js</span> &{" "}
        <span className="text-pink-300">Tailwind CSS</span>.
      </p>
    </footer>
  );
}

