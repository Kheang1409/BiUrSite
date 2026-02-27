"use client";

import { ReactNode } from "react";
import { Header } from "@/components/Header";

interface MainLayoutProps {
  children: ReactNode;
  sidebar?: ReactNode;
  rightPanel?: ReactNode;
}

export function MainLayout({ children, sidebar, rightPanel }: MainLayoutProps) {
  return (
    <div className="min-h-screen bg-gradient-dark flex flex-col">
      <Header />

      <main className="flex-1 flex">
        {sidebar && (
          <aside className="hidden lg:flex lg:w-64 lg:border-r lg:border-white/10 lg:flex-col lg:bg-white/5 lg:backdrop-blur-sm">
            {sidebar}
          </aside>
        )}

        <div className="flex-1 app-container py-8 px-4 sm:px-6 lg:px-8">
          {children}
        </div>

        {rightPanel && (
          <aside className="hidden xl:flex xl:w-80 xl:border-l xl:border-white/10 xl:flex-col xl:bg-white/5 xl:backdrop-blur-sm">
            {rightPanel}
          </aside>
        )}
      </main>
    </div>
  );
}
