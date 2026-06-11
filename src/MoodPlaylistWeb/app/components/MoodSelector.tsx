const moods = [
  { name: "Happy", color: "bg-yellow-400", icon: "😊" },
  { name: "Sad", color: "bg-blue-400", icon: "💧" },
  { name: "Chill", color: "bg-green-400", icon: "🌿" },
  { name: "Energetic", color: "bg-red-400", icon: "⚡" },
];

export default function MoodSelector() {
  return (
    <section className="text-center mt-10 px-16">
      <h2 className="text-5xl text-white font-bold mb-2">Choose Your Mood</h2>
      <p className="text-gray-200 mb-8">Select a mood to get a playlist tailored for you.</p>
      <div className="grid grid-cols-2 md:grid-cols-4 gap-6 justify-center">
        {moods.map((mood) => (
          <button
            key={mood.name}
            className={`${mood.color} py-12 rounded-xl  shadow-2xl hover:scale-105 transition transform`}
          >
            <span className="text-8xl">{mood.icon}</span>
            <p className="mt-2 font-bold text-2xl text-white">{mood.name}</p>
          </button>
        ))}
      </div>
      <div className="border-t border-gray-400 mt-16" />
    </section>
  );
}

