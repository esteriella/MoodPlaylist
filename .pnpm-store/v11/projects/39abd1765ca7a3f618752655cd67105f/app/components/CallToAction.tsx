import Link from "next/link";

export default function CallToAction() {
  return (
    <section className="px-6 py-20 md:px-12">
      <div className="relative mx-auto max-w-6xl overflow-hidden rounded-4xl border border-white/15 bg-white/10 px-7 py-16 text-center shadow-2xl backdrop-blur-md md:px-16 md:py-20">
        <div className="absolute -left-20 -top-24 h-56 w-56 rounded-full bg-pink-300/20 blur-3xl" />
        <div className="absolute -bottom-28 -right-12 h-64 w-64 rounded-full bg-violet-300/20 blur-3xl" />
        <p className="relative mb-5 text-xs font-black uppercase tracking-[.24em] text-pink-200">Your next favourite is waiting</p>
        <h2 className="relative mx-auto max-w-3xl font-serif text-4xl leading-tight tracking-tight text-white md:text-6xl">Turn the way you feel into something worth replaying.</h2>
        <p className="relative mx-auto mb-9 mt-6 max-w-xl text-base leading-7 text-white/70">Create your space, find a mood, and keep every track that feels right.</p>
        <div className="relative flex flex-col justify-center gap-3 sm:flex-row">
          <Link href="/auth/register" className="rounded-full bg-white px-7 py-4 font-extrabold text-[#6239aa] transition hover:-translate-y-1">Start your playlist</Link>
          <Link href="/auth/login" className="rounded-full border border-white/25 px-7 py-4 font-bold text-white transition hover:bg-white/10">I already have an account</Link>
        </div>
      </div>
    </section>
  );
}
