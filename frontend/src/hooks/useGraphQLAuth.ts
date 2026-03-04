"use client";

import { useMutation, useQuery } from "@apollo/client";
import {
  LOGIN_MUTATION,
  REGISTER_MUTATION,
  ME_QUERY,
  FORGOT_PASSWORD_MUTATION,
  RESET_PASSWORD_MUTATION,
} from "@/lib/graphql/queries";
import { useAuth } from "./useAuth";
import { User } from "@/types";

export function useGraphQLAuth() {
  const { login, logout, token } = useAuth();

  const [loginMutation] = useMutation(LOGIN_MUTATION);

  const [registerMutation] = useMutation(REGISTER_MUTATION);
  const [forgotPasswordMutation] = useMutation(FORGOT_PASSWORD_MUTATION);
  const [resetPasswordMutation] = useMutation(RESET_PASSWORD_MUTATION);

  const {
    data: meData,
    loading: meLoading,
    error: meError,
    refetch: refetchMe,
  } = useQuery(ME_QUERY, {
    skip: !token,
  });

  const handleLogin = async (email: string, password: string) => {
    const result = await loginMutation({
      variables: { email, password },
    });

    // If login succeeded, handle token locally instead of using onCompleted
    const tokenValue = result?.data?.login?.token;
    if (tokenValue) login(tokenValue);

    return result;
  };

  const handleRegister = async (
    username: string,
    email: string,
    password: string,
  ) => {
    const result = await registerMutation({
      variables: { username, email, password },
    });
    return result;
  };

  const handleForgotPassword = async (email: string) => {
    const result = await forgotPasswordMutation({ variables: { email } });
    return result;
  };

  const handleResetPassword = async (
    email: string,
    password: string,
    otp: string,
  ) => {
    const result = await resetPasswordMutation({
      variables: { email, password, otp },
    });
    return result;
  };

  return {
    handleLogin,
    handleRegister,
    handleForgotPassword,
    handleResetPassword,
    logout,
    currentUser: meData?.me as User | undefined,
    isLoading: meLoading,
    error: meError,
    refetchCurrentUser: refetchMe,
  };
}
