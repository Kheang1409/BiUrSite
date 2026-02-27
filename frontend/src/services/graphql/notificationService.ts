"use client";

import { useQuery, useMutation } from "@apollo/client";
import {
  NOTIFICATIONS_QUERY,
  MARK_NOTIFICATIONS_AS_READ,
} from "@/lib/graphql/queries";
import { Notification } from "@/types";

export function useNotifications(pageNumber: number, skip = false) {
  const { data, loading, error, refetch } = useQuery(NOTIFICATIONS_QUERY, {
    variables: { pageNumber },
    skip,
    fetchPolicy: "network-only",
  });

  return {
    notifications: (data?.notifications as Notification[]) || [],
    loading,
    error,
    refetch,
  };
}

export function useMarkNotificationsRead() {
  const [markRead, { loading, error }] = useMutation(
    MARK_NOTIFICATIONS_AS_READ,
  );

  const markNotificationsAsRead = async (): Promise<boolean> => {
    const result = await markRead();
    return result.data?.markNotificationsAsRead ?? false;
  };

  return { markNotificationsAsRead, loading, error };
}
