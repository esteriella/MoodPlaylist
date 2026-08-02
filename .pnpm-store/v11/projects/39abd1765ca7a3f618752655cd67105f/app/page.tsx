import Header from "@/app/components/Header";
import MoodSelector from "@/app/components/MoodSelector";
import HeroSection from "@/app/components/HeroSection";
import HowItWorks from "@/app/components/About";
import CallToAction from "@/app/components/CallToAction";
import Footer from "@/app/components/Footer";

export default function LandingPage() {
  return (
    <main className="landing-shell min-h-screen text-white">
      <Header />
      <HeroSection />
      <HowItWorks />
      <MoodSelector />
      <CallToAction />
      <Footer />
    </main>
  );
}
