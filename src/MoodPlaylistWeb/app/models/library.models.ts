export interface Mood {
  id: string;
  name: string;
  color?: string;
  emoji?: string;
}

export interface Track {
  href: string;
  id: string;
  isPlayable: boolean;
  name: string;
  popularity: number;
  previewUrl?: string;
  trackNumber: number;
  type: string;
  uri: string;
  isLocal: boolean;
  playback: {
    embedUrl: string;
    externalUrl: string;
  };
}

export interface Playlist {
  id: string;
  title: string;
  creatorName: string;
  creatorTag: string;
  mood?: Mood;
  tracks: Track[];
}

export interface UpsertPlaylist {
  title: string;
  moodId?: string;
  tracks: string;
}
