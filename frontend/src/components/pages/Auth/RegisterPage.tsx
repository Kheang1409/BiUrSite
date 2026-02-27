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

export function RegisterPage() {
  const router = useRouter();
  const { handleRegister } = useGraphQLAuth();
  const [email, setEmail] = useState("");
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");

    if (!email || !username || !password || !confirmPassword) {
      setError("All fields are required");
      return;
    }

    if (password !== confirmPassword) {
      setError("Passwords do not match");
      return;
    }

    setIsLoading(true);

    try {
      await handleRegister(username, email, password);
      router.push("/login?registered=true");
    } catch (err: unknown) {
      const errorMessage =
        err instanceof Error ? err.message : "Registration failed";
      setError(errorMessage);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <AuthFormContainer
      title="Join BiUrSite"
      subtitle="Create your account to get started"
      error={error || null}
      footer={
        <AuthLink
          prompt="Already have an account?"
          href="/login"
          linkText="Sign in"
        />
      }
    >
      <form onSubmit={handleSubmit} className="space-y-5">
        <FormInput
          type="text"
          label="Username"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
          placeholder="your_username"
          disabled={isLoading}
        />

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

        <FormInput
          type="password"
          label="Confirm Password"
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
          placeholder="••••••••"
          disabled={isLoading}
        />

        <SubmitButton
          isLoading={isLoading}
          loadingText="Creating account..."
          idleText="Create Account"
        />
      </form>
    </AuthFormContainer>
  );
}
