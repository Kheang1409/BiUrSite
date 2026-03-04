"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useGraphQLAuth } from "@/hooks/useGraphQLAuth";
import { FormInput } from "@/components/ui/atoms";
import { AuthFormContainer, SubmitButton } from "@/components/ui/molecules";

export function ForgotPasswordPage() {
  const router = useRouter();
  const { handleForgotPassword } = useGraphQLAuth();
  const [email, setEmail] = useState("");
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setMessage("");
    if (!email) {
      setError("Email is required");
      return;
    }

    setIsLoading(true);
    try {
      await handleForgotPassword(email);
      setMessage("If the email exists, an OTP was sent. Check your inbox.");
      // redirect user to reset page and prefill email
      router.push(`/reset-password?email=${encodeURIComponent(email)}`);
    } catch (err: unknown) {
      const errorMessage =
        err instanceof Error ? err.message : "Request failed";
      setError(errorMessage);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <AuthFormContainer
      title="Reset Password"
      subtitle="Enter your email to receive an OTP"
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

        <SubmitButton
          isLoading={isLoading}
          loadingText="Sending..."
          idleText="Send OTP"
        />
      </form>
    </AuthFormContainer>
  );
}
