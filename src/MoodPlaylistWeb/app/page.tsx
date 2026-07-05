import Header from "@/app/components/Header";
import MoodSelector from "@/app/components/MoodSelector";
import HeroSection from "@/app/components/HeroSection";
import HowItWorks from "@/app/components/About";
import CallToAction from "@/app/components/CallToAction";
import PlaylistGrid from "@/app/components/PlaylistGrid";
import Footer from "@/app/components/Footer";

// export default function Home() {
//   return (
//     <main className="flex flex-col min-h-screen justify-between">
//       <Header />
//       <div className="border-t border-gray-400 " />
//       <div className="grow">
//         <MoodSelector />
//         <PlaylistGrid />
//       </div>
//       <div className="border-t border-gray-400 " />
//       <Footer />
//     </main>
//   );
// }




export default function LandingPage() {
  return (
    <main className="flex flex-col justify-between text-white">
      <Header />
      <div className="border-t border-gray-200 " />
      <HeroSection />
      <HowItWorks />
      <div className="grow">
        <MoodSelector />
      </div>
      <CallToAction />
      <div className="border-t border-gray-200 " />
      <Footer />
    </main>
  );
}



