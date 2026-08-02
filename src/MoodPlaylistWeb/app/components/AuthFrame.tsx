import Image from "next/image";
import Link from "next/link";

export function AuthFrame({ eyebrow, title, description, children, footer }: {
  eyebrow: string;
  title: string;
  description: string;
  children: React.ReactNode;
  footer: React.ReactNode;
}) {
  return (
    <div className="auth-page">
      <section className="auth-story">
        <Link href="/" className="brand text-white"><span className="brand-mark">M</span><span>MoodPlaylist</span></Link>
        <div className="auth-story-copy">
          <p>Music for the moment</p>
          <h2>Every feeling has a soundtrack.</h2>
          <span>Choose the mood. Find the tracks. Keep what feels right.</span>
        </div>
        <div className="auth-art-wrap">
          <div className="auth-record"><i /><b>♫</b></div>
          <Image src="/albums/lady-removebg.png" alt="A listener enjoying music" width={520} height={560} priority />
        </div>
        <small>Thoughtful recommendations, saved your way.</small>
      </section>

      <section className="auth-form-side">
        <div className="auth-mobile-brand"><Link href="/" className="brand"><span className="brand-mark">M</span><span>MoodPlaylist</span></Link></div>
        <div className="auth-card">
          <p className="auth-eyebrow">{eyebrow}</p>
          <h1>{title}</h1>
          <p className="auth-description">{description}</p>
          {children}
          <div className="auth-footer">{footer}</div>
        </div>
        <p className="auth-legal">By continuing, you agree to our Terms and Privacy Policy.</p>
      </section>
    </div>
  );
}

export function AuthField({ label, name, type = "text", value, placeholder, autoComplete, onChange }: {
  label: string;
  name: string;
  type?: string;
  value: string;
  placeholder: string;
  autoComplete?: string;
  onChange: (event: React.ChangeEvent<HTMLInputElement>) => void;
}) {
  return (
    <label className="auth-field">
      <span>{label}</span>
      <input required name={name} type={type} value={value} placeholder={placeholder} autoComplete={autoComplete} onChange={onChange} />
    </label>
  );
}
