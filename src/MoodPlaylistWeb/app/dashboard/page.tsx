"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import toast from "react-hot-toast";
import { libraryApi } from "@/app/api/library";
import { useAuth } from "@/app/context/AuthContext";
import { Mood, Playlist, Track } from "@/app/models/library.models";
import { MenuIcon } from "@/app/components/Icons";

const initials = (value: string) =>
  value.split(" ").map((part) => part[0]).join("").slice(0, 2).toUpperCase();

const trackCount = (playlist: Playlist) => playlist.tracks?.length ?? 0;

export default function DashboardPage() {
  const router = useRouter();
  const { token, name, logout, request, skipAuthToast } = useAuth();
  const [moods, setMoods] = useState<Mood[]>([]);
  const [myPlaylists, setMyPlaylists] = useState<Playlist[]>([]);
  const [communityPlaylists, setCommunityPlaylists] = useState<Playlist[]>([]);
  const [recommendations, setRecommendations] = useState<Track[]>([]);
  const [selectedMoodIds, setSelectedMoodIds] = useState<string[]>([]);
  const [selectedTrackIds, setSelectedTrackIds] = useState<string[]>([]);
  const [playlistTitle, setPlaylistTitle] = useState("");
  const [targetPlaylistId, setTargetPlaylistId] = useState("");
  const [activeTab, setActiveTab] = useState<"create" | "library" | "discover">("create");
  const [busy, setBusy] = useState(false);
  const [loading, setLoading] = useState(true);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  const loadDashboard = useCallback(async () => {
    try {
      const [moodResult, mineResult, communityResult] = await Promise.all([
        libraryApi.moods(request),
        libraryApi.playlists(request, "mine"),
        libraryApi.playlists(request, "others"),
      ]);
      setMoods(moodResult.data ?? []);
      setMyPlaylists(mineResult.data ?? []);
      setCommunityPlaylists(communityResult.data ?? []);
      const requestedMoodId = new URLSearchParams(window.location.search).get("moodId");
      if (requestedMoodId && moodResult.data.some((mood) => mood.id === requestedMoodId))
        setSelectedMoodIds([requestedMoodId]);
    } catch {
      // The shared request helper displays the useful message.
    } finally {
      setLoading(false);
    }
  }, [request]);

  useEffect(() => {
    if (!token) {
      if (!skipAuthToast) toast.error("Sign in to open your music space");
      router.replace("/auth/login");
      return;
    }
    const loadTimer = window.setTimeout(() => void loadDashboard(), 0);
    return () => window.clearTimeout(loadTimer);
  }, [loadDashboard, router, skipAuthToast, token]);

  const selectedTracks = useMemo(
    () => recommendations.filter((track) => selectedTrackIds.includes(track.id)),
    [recommendations, selectedTrackIds],
  );

  const chooseMood = (moodId: string) => {
    setSelectedMoodIds((current) =>
      current.includes(moodId)
        ? current.filter((id) => id !== moodId)
        : current.length < 5 ? [...current, moodId] : current,
    );
    setRecommendations([]);
    setSelectedTrackIds([]);
  };

  const findMusic = async () => {
    if (!selectedMoodIds.length) return toast.error("Choose at least one mood");
    setBusy(true);
    try {
      const result = await libraryApi.recommendations(request, selectedMoodIds);
      setRecommendations(result.data ?? []);
      setSelectedTrackIds([]);
    } finally {
      setBusy(false);
    }
  };

  const createPlaylist = async () => {
    if (!playlistTitle.trim()) return toast.error("Give your playlist a name");
    if (!selectedMoodIds.length) return toast.error("Choose a mood first");
    setBusy(true);
    try {
      const result = await libraryApi.createPlaylist(request, {
        title: playlistTitle.trim(),
        moodId: selectedMoodIds[0],
        tracks: JSON.stringify(selectedTracks),
      });
      setMyPlaylists((current) => [result.data, ...current]);
      setPlaylistTitle("");
      setSelectedTrackIds([]);
      setActiveTab("library");
    } finally {
      setBusy(false);
    }
  };

  const saveToPlaylist = async () => {
    if (!targetPlaylistId) return toast.error("Choose a playlist");
    if (!selectedTracks.length) return toast.error("Select some tracks first");
    setBusy(true);
    try {
      await libraryApi.saveTracks(request, targetPlaylistId, selectedTracks);
      await loadDashboard();
      setSelectedTrackIds([]);
    } finally {
      setBusy(false);
    }
  };

  const refreshPlaylist = async (playlistId: string) => {
    setBusy(true);
    try {
      await libraryApi.refreshPlaylist(request, playlistId);
      await loadDashboard();
    } finally {
      setBusy(false);
    }
  };

  if (!token || loading) {
    return <main className="app-shell grid min-h-screen place-items-center"><div className="loader-orbit" /></main>;
  }

  return (
    <main className="app-shell min-h-screen">
      <div className="ambient ambient-one" />
      <div className="ambient ambient-two" />
      <header className="dashboard-header">
        <button className="brand" onClick={() => setActiveTab("create")}>
          <span className="brand-mark">M</span>
          <span>MoodPlaylist</span>
        </button>
        <nav className="dashboard-nav" aria-label="Dashboard sections">
          {(["create", "library", "discover"] as const).map((tab) => (
            <button key={tab} onClick={() => { setActiveTab(tab); setMobileMenuOpen(false); }} className={activeTab === tab ? "active" : ""}>
              {tab === "create" ? "Find music" : tab === "library" ? "My playlists" : "Discover"}
            </button>
          ))}
        </nav>
        <div className="profile-menu">
          <span className="profile-avatar">{initials(name ?? "MP")}</span>
          <div><strong>{name}</strong><small>Your music space</small></div>
          <button className="quiet-button" onClick={logout}>Sign out</button>
        </div>
        <button
          type="button"
          className="dashboard-menu-toggle"
          aria-label={mobileMenuOpen ? "Close dashboard menu" : "Open dashboard menu"}
          aria-expanded={mobileMenuOpen}
          aria-controls="dashboard-mobile-menu"
          onClick={() => setMobileMenuOpen((open) => !open)}
        >
          <MenuIcon open={mobileMenuOpen} />
        </button>
        {mobileMenuOpen && (
          <div id="dashboard-mobile-menu" className="dashboard-mobile-menu">
            <div className="dashboard-mobile-profile">
              <span className="profile-avatar">{initials(name ?? "MP")}</span>
              <div><strong>{name}</strong><small>Your music space</small></div>
            </div>
            <nav aria-label="Mobile dashboard sections">
              {(["create", "library", "discover"] as const).map((tab) => (
                <button key={tab} onClick={() => { setActiveTab(tab); setMobileMenuOpen(false); }} className={activeTab === tab ? "active" : ""}>
                  {tab === "create" ? "Find music" : tab === "library" ? "My playlists" : "Discover"}
                </button>
              ))}
            </nav>
            <button className="dashboard-mobile-signout" onClick={logout}>Sign out</button>
          </div>
        )}
      </header>

      <div className="dashboard-layout">
        <aside className="side-panel">
          <p className="eyebrow">Mood mixer</p>
          <h1>What does today sound like?</h1>
          <p className="muted">Pick up to five moods. We’ll shape a fresh set of tracks around your mix.</p>
          <div className="mood-list">
            {moods.map((mood) => {
              const selected = selectedMoodIds.includes(mood.id);
              return (
                <button
                  key={mood.id}
                  className={`mood-pill ${selected ? "selected" : ""}`}
                  onClick={() => chooseMood(mood.id)}
                  style={{ "--mood-color": mood.color ?? "#a78bfa" } as React.CSSProperties}
                >
                  <span>{mood.emoji || "♪"}</span><span>{mood.name}</span><i>{selected ? "✓" : "+"}</i>
                </button>
              );
            })}
          </div>
          <button className="primary-button" disabled={busy || !selectedMoodIds.length} onClick={findMusic}>
            {busy ? "Finding your sound…" : "Find my sound"}
          </button>
        </aside>

        <section className="content-panel">
          {activeTab === "create" && (
            <>
              <div className="section-heading">
                <div><p className="eyebrow">Made for this moment</p><h2>Your mood mix</h2></div>
                <span className="result-count">{recommendations.length} tracks</span>
              </div>

              {!recommendations.length ? (
                <div className="empty-state">
                  <span className="empty-icon">♫</span>
                  <h3>Your next playlist starts here</h3>
                  <p>Choose a mood on the left, then ask us to find your sound.</p>
                </div>
              ) : (
                <div className="track-list">
                  {recommendations.map((track, index) => {
                    const selected = selectedTrackIds.includes(track.id);
                    return (
                      <button key={track.id} className={`track-row ${selected ? "selected" : ""}`} onClick={() => setSelectedTrackIds((current) => selected ? current.filter((id) => id !== track.id) : [...current, track.id])}>
                        <span className="track-number">{String(index + 1).padStart(2, "0")}</span>
                        <span className="track-art">{track.name.slice(0, 1).toUpperCase()}</span>
                        <span className="track-copy"><strong>{track.name}</strong><small>Spotify recommendation</small></span>
                        <span className="popularity">{track.popularity ? `${track.popularity}% match` : "Fresh pick"}</span>
                        <span className="select-mark">{selected ? "✓" : "+"}</span>
                      </button>
                    );
                  })}
                </div>
              )}

              {recommendations.length > 0 && (
                <div className="save-bar">
                  <div><strong>{selectedTracks.length} selected</strong><small>Choose where these tracks should live.</small></div>
                  <input value={playlistTitle} onChange={(event) => setPlaylistTitle(event.target.value)} placeholder="New playlist name" />
                  <button className="secondary-button" disabled={busy} onClick={createPlaylist}>Create playlist</button>
                  <span className="save-divider">or</span>
                  <select value={targetPlaylistId} onChange={(event) => setTargetPlaylistId(event.target.value)}>
                    <option value="">Existing playlist</option>
                    {myPlaylists.map((playlist) => <option key={playlist.id} value={playlist.id}>{playlist.title}</option>)}
                  </select>
                  <button className="primary-button compact" disabled={busy} onClick={saveToPlaylist}>Save tracks</button>
                </div>
              )}
            </>
          )}

          {activeTab === "library" && (
            <PlaylistCollection title="My playlists" subtitle="Everything you’ve saved, ready for another listen." playlists={myPlaylists} actionLabel="Refresh mix" onAction={refreshPlaylist} busy={busy} />
          )}

          {activeTab === "discover" && (
            <PlaylistCollection title="Made by the community" subtitle="Explore mood-led playlists from other listeners." playlists={communityPlaylists} />
          )}
        </section>
      </div>
    </main>
  );
}

function PlaylistCollection({ title, subtitle, playlists, actionLabel, onAction, busy }: {
  title: string;
  subtitle: string;
  playlists: Playlist[];
  actionLabel?: string;
  onAction?: (id: string) => void;
  busy?: boolean;
}) {
  return (
    <>
      <div className="section-heading"><div><p className="eyebrow">Your collection</p><h2>{title}</h2><p className="muted">{subtitle}</p></div><span className="result-count">{playlists.length} playlists</span></div>
      <div className="playlist-grid">
        {playlists.map((playlist, index) => (
          <article className="playlist-card" key={playlist.id}>
            <div className={`playlist-cover cover-${index % 4}`}><span>{playlist.mood?.emoji || "♫"}</span><i /></div>
            <div className="playlist-card-copy"><p>{playlist.mood?.name || "Mixed mood"}</p><h3>{playlist.title}</h3><small>{trackCount(playlist)} tracks · {playlist.creatorName}</small></div>
            {actionLabel && playlist.mood && <button disabled={busy} onClick={() => onAction?.(playlist.id)}>{actionLabel}</button>}
          </article>
        ))}
        {!playlists.length && <div className="empty-state wide"><span className="empty-icon">♡</span><h3>Nothing here yet</h3><p>Your first playlist will appear here.</p></div>}
      </div>
    </>
  );
}
