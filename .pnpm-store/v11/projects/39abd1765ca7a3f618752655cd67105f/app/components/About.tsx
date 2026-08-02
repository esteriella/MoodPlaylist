const steps = [
  { number: "01", title: "Name the feeling", copy: "Choose one mood or blend a few when the day refuses to fit one label.", symbol: "◌" },
  { number: "02", title: "Meet your mix", copy: "Discover fresh Spotify tracks shaped around the energy you picked.", symbol: "♫" },
  { number: "03", title: "Make it yours", copy: "Keep your favourites together, refresh the mix, and come back anytime.", symbol: "♡" },
];

export default function HowItWorks() {
  return (
    <section className="relative bg-[#f8f5ef] px-6 py-24 text-[#1c1722] md:px-12">
      <div className="mx-auto max-w-6xl">
        <div className="mb-14 grid gap-7 md:grid-cols-[.8fr_1.2fr] md:items-end">
          <div>
            <p className="mb-4 text-xs font-black uppercase tracking-[.22em] text-[#8051d0]">Simple by design</p>
            <h2 className="font-serif text-4xl leading-none tracking-tight md:text-6xl">From feeling to playlist.</h2>
          </div>
          <p className="max-w-xl text-base leading-7 text-[#77707d] md:justify-self-end">No endless searching and no complicated setup. Start with how you feel and let the music follow.</p>
        </div>
        <div className="grid gap-px overflow-hidden rounded-[28px] border border-black/10 bg-black/10 md:grid-cols-3">
          {steps.map((step) => (
            <article key={step.number} className="group bg-white p-8 transition hover:bg-[#fbf8ff] md:p-10">
              <div className="mb-16 flex items-start justify-between">
                <span className="text-xs font-black tracking-[.18em] text-[#9b92a0]">{step.number}</span>
                <span className="grid h-12 w-12 place-items-center rounded-2xl bg-[#eee7fb] text-xl text-[#7445c7] transition group-hover:-rotate-6 group-hover:scale-110">{step.symbol}</span>
              </div>
              <h3 className="mb-3 font-serif text-2xl">{step.title}</h3>
              <p className="text-sm leading-6 text-[#77707d]">{step.copy}</p>
            </article>
          ))}
        </div>
      </div>
    </section>
  );
}
