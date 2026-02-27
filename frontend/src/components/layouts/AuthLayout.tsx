"use client";

import { ReactNode } from "react";

export function AuthLayout({ children }: { children: ReactNode }) {
  return (
    <div className="min-h-screen bg-gradient-dark flex items-center justify-center">
      <div className="w-full max-w-md mx-4">{children}</div>
    </div>
  );
}
