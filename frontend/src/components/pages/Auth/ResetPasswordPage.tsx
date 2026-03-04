"use client";

import { useState, useEffect } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { useGraphQLAuth } from "@/hooks/useGraphQLAuth";
import { FormInput } from "@/components/ui/atoms";
import { AuthFormContainer, SubmitButton } from "@/components/ui/molecules";

export function ResetPasswordPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { handleResetPassword } = useGraphQLAuth();

  const [email, setEmail] = useState("");
  const [otp, setOtp] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    const e = searchParams?.get("email");
    if (e) setEmail(e);
  }, [searchParams]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    if (!email || !otp || !password || !confirmPassword) {
      setError("All fields are required");
      return;
    }
    if (password !== confirmPassword) {
      setError("Passwords do not match");
      return;
    }

    setIsLoading(true);
    try {
      await handleResetPassword(email, password, otp);
      router.push("/login");
    } catch (err: unknown) {
      const errorMessage = err instanceof Error ? err.message : "Reset failed";
      setError(errorMessage);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <AuthFormContainer
      title="Set New Password"
      subtitle="Enter the OTP you received and set a new password"
      error={error || null}
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
          type="text"
          label="OTP"
          value={otp}
          onChange={(e) => setOtp(e.target.value)}
          placeholder="Enter OTP"
          disabled={isLoading}
        />

        <FormInput
          type="password"
          label="New Password"
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
          loadingText="Resetting..."
          idleText="Reset Password"
        />
      </form>
    </AuthFormContainer>
  );
}
