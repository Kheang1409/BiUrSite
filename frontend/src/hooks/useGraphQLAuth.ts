"use client";

import { useMutation, useQuery } from "@apollo/client";
import {
  LOGIN_MUTATION,
  REGISTER_MUTATION,
  ME_QUERY,
} from "@/lib/graphql/queries";
import { useAuth } from "./useAuth";
import { User } from "@/types";

export function useGraphQLAuth() {
  const { login, logout, token } = useAuth();

  const [loginMutation] = useMutation(LOGIN_MUTATION);

  const [registerMutation] = useMutation(REGISTER_MUTATION);

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

  return {
    handleLogin,
    handleRegister,
    logout,
    currentUser: meData?.me as User | undefined,
    isLoading: meLoading,
    error: meError,
    refetchCurrentUser: refetchMe,
  };
}
