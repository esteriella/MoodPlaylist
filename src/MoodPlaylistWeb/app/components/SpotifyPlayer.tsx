import { Track } from "@/app/models/library.models";

export default function SpotifyPlayer({ track, onClose }: { track?: Track; onClose: () => void }) {
  if (!track) return null;

  return (
    <aside className="spotify-player" aria-label={`Now listening to ${track.name}`}>
      <div className="spotify-player-heading">
        <div><span>Now listening</span><strong>{track.name}</strong></div>
        <div className="spotify-player-actions">
          <a href={track.playback.externalUrl} target="_blank" rel="noreferrer">Open in Spotify</a>
          <button type="button" onClick={onClose} aria-label="Close Spotify player">×</button>
        </div>
      </div>
      <iframe
        src={track.playback.embedUrl}
        title={`Spotify player for ${track.name}`}
        width="100%"
        height="152"
        allow="autoplay; clipboard-write; encrypted-media; fullscreen; picture-in-picture"
        loading="lazy"
      />
      <small>Spotify controls playback. Sign in there for the listening access available to your account.</small>
    </aside>
  );
}
