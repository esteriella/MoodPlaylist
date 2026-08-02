import { describe, expect, it, vi } from "vitest";
import { libraryApi } from "./library";
import type { Track } from "@/app/models/library.models";

describe("libraryApi", () => {
  it("builds the playlists query including an optional mood", async () => {
    const request = vi.fn().mockResolvedValue({ data: [] });

    await libraryApi.playlists(request, "mine", "happy mood");

    expect(request).toHaveBeenCalledWith(
      "https://localhost:44302/library/playlists?view=mine&sortDir=desc&pageSize=24&moodId=happy+mood",
      {},
    );
  });

  it("appends every selected mood to recommendation requests", async () => {
    const request = vi.fn().mockResolvedValue({ data: [] });

    await libraryApi.recommendations(request, ["calm", "focused"]);

    expect(request).toHaveBeenCalledWith(
      "https://localhost:44302/library/recommendations?limit=20&moodIds=calm&moodIds=focused",
      {},
    );
  });

  it("serializes playlist creation", async () => {
    const request = vi.fn().mockResolvedValue({});
    const body = { title: "Evening", moodId: "calm", tracks: "[]" };

    await libraryApi.createPlaylist(request, body);

    expect(request).toHaveBeenCalledWith("https://localhost:44302/library/playlists", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    });
  });

  it("sends selected tracks to an existing playlist", async () => {
    const request = vi.fn().mockResolvedValue({});
    const tracks = [{ id: "track-1", name: "First" }] as Track[];

    await libraryApi.saveTracks(request, "playlist-1", tracks);

    expect(request).toHaveBeenCalledWith("https://localhost:44302/library/playlists/playlist-1/tracks", expect.objectContaining({
      method: "POST",
      body: JSON.stringify({ tracks }),
    }));
  });
});
