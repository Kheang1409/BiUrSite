"use client";

import { ReactNode } from "react";
import { Providers } from "@/components/Providers";
import { AuthProvider } from "@/context/AuthContext";

export function ClientProviders({ children }: { children: ReactNode }) {
  return (
    <AuthProvider>
      <Providers>{children}</Providers>
    </AuthProvider>
  );
}
