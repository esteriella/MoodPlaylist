import Image from "next/image";

const tracks = [
  { title: "Feel Good Song", artist: "Artist Name", img: "/albums/feel-good.jpg" },
  { title: "Sunshine Vibes", artist: "Artist Name", img: "/albums/sunshine-vibes.jpg" },
  { title: "Upbeat Groove", artist: "Artist Name", img: "/albums/upbeat-groove.jpg" },
];

export default function PlaylistGrid() {
  return (
    <section className="text-center mt-16 px-16 mb-72">
      <h2 className="text-5xl font-bold mb-2 text-white">Your Playlist</h2>
      <p className="text-gray-200 mb-8">Tracks curated for your mood.</p>
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6 justify-center">
        {tracks.map((track) => (
          <div
            key={track.title}
            className="bg-black/50 backdrop-blur-md  rounded-xl hover:bg-white/20 transition"
          >
            <Image
              src={track.img}
              alt={track.title}
              width={700}
              height={300}
              className="rounded-lg mb-4"
            />
            <p className="font-bold text-2xl text-white mt-8 px-4 text-start">{track.title}</p>
            <p className="text-sm text-gray-200 mt-2 mb-4 px-4 text-start">{track.artist}</p>
          </div>
        ))}
      </div>
    </section>
  );
}

