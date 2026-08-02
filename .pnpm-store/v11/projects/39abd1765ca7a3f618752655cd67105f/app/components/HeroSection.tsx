import Image from "next/image";
import Link from "next/link";

export default function HeroSection() {
  return (
    <section className="relative mx-auto flex w-full max-w-360 flex-col items-center overflow-hidden pl-7 pb-16 pt-12 sm:px-10 md:min-h-170 md:flex-row md:items-center md:px-12 md:py-16 lg:px-16">
      <div className="z-10 text-center md:w-[48%] md:text-left lg:w-[46%]">
        <p className="mb-5 text-xs font-black uppercase tracking-[.24em] text-pink-200">A playlist for every feeling</p>
        <h1 className="mb-6 font-serif text-5xl leading-[.95] tracking-tight text-white md:text-7xl">Music that meets you where you are.</h1>
        <p className="mx-auto mb-10 max-w-lg text-lg leading-8 text-white/75 md:mx-0">Pick how you feel, discover a fresh mix, and keep the tracks that belong in this moment.</p>
        <Link href="/auth/register" className="inline-flex rounded-full bg-white px-7 py-4 font-extrabold text-[#6239aa] shadow-2xl transition hover:-translate-y-1">Create your first mix</Link>
      </div>
      <div className="relative -mb-16 mt-8 flex h-110 w-full items-end justify-center sm:h-130 md:absolute md:inset-y-0 md:right-[-7%] md:mb-0 md:mt-0 md:h-auto md:w-[62%] md:justify-end lg:right-[-3%] lg:w-[60%]">
        <div className="absolute inset-x-[12%] bottom-[8%] top-[16%] rounded-full bg-pink-300/25 blur-3xl md:inset-x-[8%]" />
        <Image
          src="/albums/lady-removebg.png"
          alt="A listener enjoying music"
          width={790}
          height={842}
          priority
          sizes="(max-width: 767px) 92vw, 62vw"
          className="relative h-full w-auto max-w-none object-contain object-bottom drop-shadow-[0_28px_45px_rgba(24,10,38,0.24)] md:h-[94%] lg:h-[102%]"
        />
      </div>
    </section>
  );
}
