import { Toaster } from "react-hot-toast";

export default function AuthLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <main className="min-h-screen flex items-center justify-center overflow-y-hidden bg-[#0f0f1a]">
      {children}
      <Toaster
          position="top-right"
          toastOptions={{
            style: { background: "#333", color: "#fff" },
            success: { style: { background: "#4caf50" } },
            error: { style: { background: "#f44336" } },
          }}
        />
    </main>
  );
}
