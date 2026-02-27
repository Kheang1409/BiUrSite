import React, { ReactNode } from "react";
import Link from "next/link";

interface AuthFormContainerProps {
  title: string;
  subtitle: string;
  error?: string | null;
  children: ReactNode;
  footer?: ReactNode;
}

export function AuthFormContainer({
  title,
  subtitle,
  error,
  children,
  footer,
}: AuthFormContainerProps) {
  return (
    <div className="min-h-screen bg-gradient-dark flex items-center justify-center px-4 py-6">
      <div className="w-full max-w-md">
        <div className="text-center mb-8">
          <h1 className="text-4xl font-bold text-white mb-2">{title}</h1>
          <p className="text-white/70 text-sm">{subtitle}</p>
        </div>

        <div className="bg-white/5 border border-white/10 rounded-2xl p-8">
          {error && (
            <div className="mb-6 p-4 bg-red-500/20 border border-red-500/50 rounded-lg">
              <p className="text-red-400 text-sm">{error}</p>
            </div>
          )}

          {children}

          {footer && <div className="mt-6 text-center">{footer}</div>}
        </div>
      </div>
    </div>
  );
}

interface AuthLinkProps {
  prompt: string;
  href: string;
  linkText: string;
}

export function AuthLink({ prompt, href, linkText }: AuthLinkProps) {
  return (
    <p className="text-white/70 text-sm">
      {prompt}{" "}
      <Link
        href={href}
        className="text-blue-400 hover:text-blue-300 font-semibold"
      >
        {linkText}
      </Link>
    </p>
  );
}

interface SubmitButtonProps {
  isLoading: boolean;
  loadingText: string;
  idleText: string;
}

export function SubmitButton({
  isLoading,
  loadingText,
  idleText,
}: SubmitButtonProps) {
  return (
    <button
      type="submit"
      disabled={isLoading}
      className="w-full py-3 px-4 bg-blue-600 hover:bg-blue-700 text-white font-semibold rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
    >
      {isLoading ? loadingText : idleText}
    </button>
  );
}
