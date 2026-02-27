"use client";

import { useQuery, useMutation } from "@apollo/client";
import {
  ME_QUERY,
  USER_QUERY,
  USERS_QUERY,
  UPDATE_ME_MUTATION,
} from "@/lib/graphql/queries";
import { User } from "@/types";

export interface UpdateProfileInput {
  username: string;
  bio: string;
  phone?: string;
  data?: number[] | null;
  removeImage?: boolean;
}

export function useCurrentUser(skip = false) {
  const { data, loading, error, refetch } = useQuery(ME_QUERY, {
    skip,
    fetchPolicy: "cache-and-network",
  });

  return {
    user: data?.me as User | undefined,
    loading,
    error,
    refetch,
  };
}

export function useUser(userId: string | undefined) {
  const { data, loading, error, refetch } = useQuery(USER_QUERY, {
    variables: { id: userId },
    skip: !userId,
    fetchPolicy: "cache-and-network",
  });

  return {
    user: data?.user as User | undefined,
    loading,
    error,
    refetch,
  };
}

export function useUsers(pageNumber: number) {
  const { data, loading, error, refetch } = useQuery(USERS_QUERY, {
    variables: { pageNumber },
    fetchPolicy: "cache-and-network",
  });

  return {
    users: (data?.users as User[]) || [],
    loading,
    error,
    refetch,
  };
}

export function useUpdateProfile() {
  const [updateMe, { loading, error }] = useMutation(UPDATE_ME_MUTATION);

  const updateProfile = async (input: UpdateProfileInput): Promise<boolean> => {
    const result = await updateMe({
      variables: {
        username: input.username,
        bio: input.bio,
        phone: input.phone,
        data: input.data,
        removeImage: input.removeImage ?? false,
      },
    });
    return result.data?.updateMe ?? false;
  };

  return { updateProfile, loading, error };
}
