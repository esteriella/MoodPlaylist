import { Toaster } from "react-hot-toast";
import { AuthProvider } from "./context/AuthContext";
import "./globals.css";

export const metadata = {
  title: "MoodPlaylist",
  description: "Music playlists tailored to your mood",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body className="min-h-screen flex flex-col justify-between" >
        <AuthProvider>
          {children}
        </AuthProvider>
        
      </body>
    </html>
  );
}


