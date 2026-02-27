"use client";

import { useEffect, useRef } from "react";
import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
  HttpTransportType,
  HubConnectionState,
} from "@microsoft/signalr";
import { useAuth } from "./useAuth";

type NotificationPayload = {
  id: string;
  postId: string;
  userId: string;
  username: string;
  userProfile: string;
  title: string;
  message: string;
  createdDate: string;
};

export function useNotificationHub(
  onNotification: (n: NotificationPayload) => void,
) {
  const { token, isAuthenticated } = useAuth();
  const connectionRef = useRef<HubConnection | null>(null);
  const isStartingRef = useRef(false);
  const shouldStopRef = useRef(false);

  useEffect(() => {
    if (!isAuthenticated) return;
    if (!token) return;

    const base = (process.env.NEXT_PUBLIC_API_URL || "").replace(/\/$/, "");
    const url = `${base}/notificationHub`;

    // Diagnostic: fetch negotiate endpoint to log status/body to help debug negotiation failures.
    (async () => {
      try {
        const negotiateUrl = `${url}/negotiate`;
        const res = await fetch(negotiateUrl, {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${token}`,
          },
          credentials: "include",
          body: JSON.stringify({}),
        });

        if (!res.ok) {
          let text = "";
          try {
            text = await res.text();
          } catch (e) {
            text = String(e);
          }
          // eslint-disable-next-line no-console
          console.warn(
            "SignalR negotiate diagnostic: status",
            res.status,
            "body:",
            text,
          );
        } else {
          try {
            const json = await res.json();
            // eslint-disable-next-line no-console
            console.log("SignalR negotiate diagnostic: success", json);
          } catch (e) {
            const txt = await res.text();
            // eslint-disable-next-line no-console
            console.log("SignalR negotiate diagnostic: ok non-json body", txt);
          }
        }
      } catch (e) {
        // eslint-disable-next-line no-console
        console.error("SignalR negotiate diagnostic fetch failed:", e);
      }
    })();

    const connection = new HubConnectionBuilder()
      .withUrl(url, { accessTokenFactory: () => token })
      .configureLogging(LogLevel.Warning)
      .withAutomaticReconnect()
      .build();

    connectionRef.current = connection;

    // Attach handlers before start to avoid missing messages
    connection.on(
      "ReceiveCommentNotification",
      (payload: NotificationPayload) => {
        try {
          onNotification(payload);
        } catch (e) {
          // eslint-disable-next-line no-console
          console.error("notification handler error", e);
        }
      },
    );

    connection.onclose((error) => {
      // eslint-disable-next-line no-console
      if (error) console.warn("SignalR connection closed with error:", error);
    });

    // Prevent concurrent starts/stops causing the negotiation race
    (async () => {
      if (isStartingRef.current) return;
      if (connection.state === HubConnectionState.Connected) return;

      isStartingRef.current = true;
      shouldStopRef.current = false;

      try {
        await connection.start();
        // If cleanup requested while starting, stop immediately and bail out
        if (shouldStopRef.current) {
          await connection.stop().catch(() => {});
          return;
        }
        // started successfully
      } catch (err) {
        // Provide richer logging to help debugging negotiation failures
        // eslint-disable-next-line no-console
        console.error(
          "SignalR connection failed:",
          err?.toString?.() || err,
          err,
        );

        // If negotiation failed, try again forcing WebSockets transport (helps some proxies)
        if (
          String(err).toLowerCase().includes("negotiation") ||
          String(err).toLowerCase().includes("stopped during negotiation")
        ) {
          // eslint-disable-next-line no-console
          console.warn(
            "Retrying SignalR start forcing WebSockets transport...",
          );
          const wsConnection = new HubConnectionBuilder()
            .withUrl(url, {
              accessTokenFactory: () => token,
              transport: HttpTransportType.WebSockets,
            })
            .configureLogging(LogLevel.Warning)
            .withAutomaticReconnect()
            .build();

          // attach handler to retry connection
          wsConnection.on(
            "ReceiveCommentNotification",
            (payload: NotificationPayload) => {
              try {
                onNotification(payload);
              } catch (e) {
                // eslint-disable-next-line no-console
                console.error("notification handler error", e);
              }
            },
          );

          try {
            await wsConnection.start();
            connectionRef.current = wsConnection;
          } catch (err2) {
            // eslint-disable-next-line no-console
            console.error("SignalR reconnect (WebSockets) also failed:", err2);
          }
        }
      } finally {
        isStartingRef.current = false;
      }
    })();

    return () => {
      // If a start is in progress, request a stop after it completes instead of stopping immediately
      if (isStartingRef.current) {
        shouldStopRef.current = true;
      } else {
        try {
          connection.off("ReceiveCommentNotification");
          connection.stop().catch(() => {});
        } catch {
          // ignore
        }
      }
      connectionRef.current = null;
    };
    // token intentionally included — recreate connection when token changes
  }, [isAuthenticated, token, onNotification]);
}
