import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import SpotifyPlayer from "./SpotifyPlayer";
import type { Track } from "@/app/models/library.models";

const track = {
  id: "4iV5W9uYEdYUVa79Axb7Rh",
  name: "Sunrise",
  playback: {
    embedUrl: "https://open.spotify.com/embed/track/4iV5W9uYEdYUVa79Axb7Rh",
    externalUrl: "https://open.spotify.com/track/4iV5W9uYEdYUVa79Axb7Rh",
  },
} as Track;

describe("SpotifyPlayer", () => {
  it("renders the official embed and external fallback", () => {
    render(<SpotifyPlayer track={track} onClose={() => undefined} />);

    expect(screen.getByTitle("Spotify player for Sunrise")).toHaveAttribute("src", track.playback.embedUrl);
    expect(screen.getByRole("link", { name: "Open in Spotify" })).toHaveAttribute("href", track.playback.externalUrl);
  });

  it("closes from its labelled button", () => {
    const onClose = vi.fn();
    render(<SpotifyPlayer track={track} onClose={onClose} />);

    fireEvent.click(screen.getByRole("button", { name: "Close Spotify player" }));
    expect(onClose).toHaveBeenCalledOnce();
  });

  it("renders nothing until a track is selected", () => {
    const { container } = render(<SpotifyPlayer onClose={() => undefined} />);
    expect(container).toBeEmptyDOMElement();
  });
});
