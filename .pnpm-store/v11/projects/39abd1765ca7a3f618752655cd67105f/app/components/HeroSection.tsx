import Image from "next/image";
import Link from "next/link";

export default function HeroSection() {
  return (
    <section className="relative mx-auto flex w-full max-w-7xl flex-col items-center overflow-hidden px-7 pb-24 pt-12 md:flex-row md:px-12 md:pt-16">
      <div className="z-10 text-center md:w-1/2 md:text-left">
        <p className="mb-5 text-xs font-black uppercase tracking-[.24em] text-pink-200">A playlist for every feeling</p>
        <h1 className="mb-6 font-serif text-5xl leading-[.95] tracking-tight text-white md:text-7xl">Music that meets you where you are.</h1>
        <p className="mx-auto mb-10 max-w-lg text-lg leading-8 text-white/75 md:mx-0">Pick how you feel, discover a fresh mix, and keep the tracks that belong in this moment.</p>
        <Link href="/auth/register" className="inline-flex rounded-full bg-white px-7 py-4 font-extrabold text-[#6239aa] shadow-2xl transition hover:-translate-y-1">Create your first mix</Link>
      </div>
      <div className="relative mt-10 flex w-full justify-center md:mt-0 md:w-1/2">
        <div className="absolute inset-12 rounded-full bg-pink-300/20 blur-3xl" />
        <Image src="/albums/lady-removebg.png" alt="A listener enjoying music" width={640} height={640} priority className="relative max-h-[560px] w-auto object-contain" />
      </div>
    </section>
  );
}
