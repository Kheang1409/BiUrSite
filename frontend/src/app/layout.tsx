import type { Metadata } from "next";
import { ClientProviders } from "@/components/ClientProviders";
import "@/styles/globals.css";

export const metadata: Metadata = {
  title: "BiUrSite - Social Media Platform",
  description: "A mini social media platform for connecting with others",
  icons: {
    icon: "/favicon.svg",
  },
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body className="bg-gradient-dark text-white" suppressHydrationWarning>
        <ClientProviders>{children}</ClientProviders>
      </body>
    </html>
  );
}
