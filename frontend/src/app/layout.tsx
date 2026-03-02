import type { Metadata } from "next";
import { ClientProviders } from "@/components/ClientProviders";
import "@/styles/globals.css";

export const metadata: Metadata = {
  title: "BiUrSite - Social Media Platform",
  description: "A mini social media platform for connecting with others",
  icons: {
    icon: [
      { url: "/favicon-32.png", sizes: "32x32", type: "image/png" },
      { url: "/favicon-16.png", sizes: "16x16", type: "image/png" },
      { url: "/favicon.svg", type: "image/svg+xml" },
    ],
  },
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en" suppressHydrationWarning>
      <head>
        {/* Favicon links - PNG fallbacks for maximum browser compatibility */}
        <link
          rel="icon"
          type="image/png"
          sizes="32x32"
          href="/favicon-32.png"
        />
        <link
          rel="icon"
          type="image/png"
          sizes="16x16"
          href="/favicon-16.png"
        />
        <link rel="icon" href="/favicon.svg" type="image/svg+xml" />
        <link rel="shortcut icon" href="/favicon-32.png" />
        <meta name="theme-color" content="#0A1628" />
      </head>
      <body className="bg-gradient-dark text-white" suppressHydrationWarning>
        <ClientProviders>{children}</ClientProviders>
      </body>
    </html>
  );
}
