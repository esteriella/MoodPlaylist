import Header from "@/app/components/Header";
import MoodSelector from "@/app/components/MoodSelector";
import PlaylistGrid from "@/app/components/PlaylistGrid";
import Footer from "@/app/components/Footer";

export default function Home() {
  return (
    <main className="flex flex-col min-h-screen justify-between">
      <Header />
      <div className="border-t border-gray-400 " />
      <div className="grow">
        <MoodSelector />
        <PlaylistGrid />
      </div>
      <div className="border-t border-gray-400 " />
      <Footer />
    </main>
  );
}



