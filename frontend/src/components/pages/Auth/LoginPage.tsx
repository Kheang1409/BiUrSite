"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useGraphQLAuth } from "@/hooks/useGraphQLAuth";
import { FormInput } from "@/components/ui/atoms";
import {
  AuthFormContainer,
  AuthLink,
  SubmitButton,
} from "@/components/ui/molecules";

export function LoginPage() {
  const router = useRouter();
  const { handleLogin } = useGraphQLAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");

    if (!email || !password) {
      setError("Email and password are required");
      return;
    }

    setIsLoading(true);

    try {
      await handleLogin(email, password);
      router.push("/");
    } catch (err: unknown) {
      const errorMessage = err instanceof Error ? err.message : "Login failed";
      setError(errorMessage);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <AuthFormContainer
      title="Welcome Back"
      subtitle="Sign in to your BiUrSite account"
      error={error || null}
      footer={
        <AuthLink
          prompt="Don't have an account?"
          href="/register"
          linkText="Create one"
        />
      }
    >
      <form onSubmit={handleSubmit} className="space-y-5">
        <FormInput
          type="email"
          label="Email Address"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          placeholder="you@example.com"
          disabled={isLoading}
        />

        <FormInput
          type="password"
          label="Password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          placeholder="••••••••"
          disabled={isLoading}
        />

        <div className="text-right">
          <AuthLink
            prompt=""
            href="/forgot-password"
            linkText="Forgot password?"
          />
        </div>

        <SubmitButton
          isLoading={isLoading}
          loadingText="Signing in..."
          idleText="Sign In"
        />
      </form>
    </AuthFormContainer>
  );
}
