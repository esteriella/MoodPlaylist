// Responses
export interface AvailableMoodModel{
    id: string;// this is a guid, maybe preferably we should use uuid here but for now we can use guid as string
    name: string;
    color?: string;
    emoji?: string;
};

export interface UserPlaylistModel{
    title: string;
    creatorName: string;
    creatorTag: string;
    mood?: AvailableMoodModel;
    Tracks: TrackModel[]; // tracks comes as json string so we have to format back to array of tracks
};

export interface TrackModel{
    href: string;
    id: string;
    isPlayable: boolean;
    name: string;
    popularity: number;
    previewUrl: string;
    trackNumber: number;
    type: string;
    uri: string;
    isLocal: boolean;
};

export interface TrackDetailModel{
    href: string;
    id: string;
    isPlayable: boolean;
    name: string;
    popularity: number;
    previewUrl: string;
    trackNumber: number;
    type: string;
    uri: string;
    isLocal: boolean;
};

// Requests

export interface UpsertModel{
    title: string;
    moodId?: string; // this is a guid
    tracks: string; // tracks should be formated as json before sending to backend
};