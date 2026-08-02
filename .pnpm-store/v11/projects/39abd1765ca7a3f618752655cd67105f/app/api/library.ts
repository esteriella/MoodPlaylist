import { config } from "@/app/helpers/config";
import { ApiResponseModel } from "@/app/models/api-response.model";
import { Mood, Playlist, Track, UpsertPlaylist } from "@/app/models/library.models";

type Request = <T>(url: string, options: RequestInit) => Promise<T>;

const endpoint = (path: string) => `${config.apiBaseUrl}/library${path}`;

export const libraryApi = {
  moods: (request: Request) =>
    request<ApiResponseModel<Mood[]>>(endpoint("/available-moods"), {}),

  playlists: (request: Request, view: "mine" | "others" | "all", moodId?: string) => {
    const query = new URLSearchParams({ view, sortDir: "desc", pageSize: "24" });
    if (moodId) query.set("moodId", moodId);
    return request<ApiResponseModel<Playlist[]>>(endpoint(`/playlists?${query}`), {});
  },

  recommendations: (request: Request, moodIds: string[]) => {
    const query = new URLSearchParams({ limit: "20" });
    moodIds.forEach((id) => query.append("moodIds", id));
    return request<ApiResponseModel<Track[]>>(endpoint(`/recommendations?${query}`), {});
  },

  createPlaylist: (request: Request, body: UpsertPlaylist) =>
    request<ApiResponseModel<Playlist>>(endpoint("/playlists"), {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    }),

  saveTracks: (request: Request, playlistId: string, tracks: Track[]) =>
    request<ApiResponseModel<Track[]>>(endpoint(`/playlists/${playlistId}/tracks`), {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ tracks }),
    }),

  refreshPlaylist: (request: Request, playlistId: string) =>
    request<ApiResponseModel<Track[]>>(endpoint(`/playlists/${playlistId}/refresh`), {
      method: "POST",
    }),
};
