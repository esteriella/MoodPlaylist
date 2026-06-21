export default function HowItWorks() {
  return (
    <section className="relative bg-black/25 backdrop-blur-md py-20 px-8 text-center overflow-hidden">
      {/* Curved Top */}
      <div className="absolute top-0 left-0 w-full overflow-hidden leading-0">
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

      <h2 className="text-4xl font-bold text-white mb-20 relative z-10">
        How It Works
      </h2>

      <div className="grid md:grid-cols-3 gap-10 text-gray-200 max-w-5xl mx-auto relative z-10">
        {/* Card 1 */}
        <div className="relative bg-black/50 rounded-3xl p-8 shadow-lg hover:bg-white/20 transition">
          <div className="absolute -top-10 left-1/2 transform -translate-x-1/2">
            <span className="text-6xl bg-linear-to-r from-yellow-600 to-orange-700 rounded-full p-4">
              🎭
            </span>
          </div>
          <h3 className="text-xl font-bold mt-10 text-white">
            Choose Your Mood
          </h3>
          <p className="text-gray-200 mt-2">
            Select from moods like Happy, Sad, Energetic, and more.
          </p>
        </div>

        {/* Card 2 */}
        <div className="relative bg-black/50 rounded-3xl p-8 shadow-lg hover:bg-white/20 transition">
          <div className="absolute -top-10 left-1/2 transform -translate-x-1/2">
            <span className="text-6xl bg-linear-to-r from-pink-700 to-purple-800 rounded-full p-4">
              🎶
            </span>
          </div>
          <h3 className="text-xl font-bold mt-10 text-white">
            Get Your Playlist
          </h3>
          <p className="text-gray-200 mt-2">
            We’ll create a playlist that fits your mood.
          </p>
        </div>

        {/* Card 3 */}
        <div className="relative bg-black/50 rounded-3xl p-8 shadow-lg hover:bg-white/20 transition">
          <div className="absolute -top-10 left-1/2 transform -translate-x-1/2">
            <span className="text-6xl bg-linear-to-r from-red-500 to-pink-700 rounded-full p-4">
              💖
            </span>
          </div>
          <h3 className="text-xl font-bold mt-10 text-white">Enjoy & Save</h3>
          <p className="text-gray-200 mt-2">
            Register to save your moods and playlists to your dashboard.
          </p>
        </div>
      </div>

      {/* Curved Bottom */}
      {/* <div className="absolute bottom-0 left-0 w-full overflow-hidden leading-0 rotate-180">
        <svg
          className="relative block w-full h-20"
          xmlns="http://www.w3.org/2000/svg"
          preserveAspectRatio="none"
          viewBox="0 0 1200 120"
        >
          <path
            d="M0,0 C300,100 900,0 1200,100 L1200,0 L0,0 Z"
            className="fill-black/30"
          ></path>
        </svg>
      </div> */}
    </section>
  );
}
