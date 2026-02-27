"use client";

import {
  ApolloClient,
  InMemoryCache,
  HttpLink,
  ApolloProvider,
} from "@apollo/client";
import { onError } from "@apollo/client/link/error";
import { setContext } from "@apollo/client/link/context";
import { ReactNode } from "react";
import { ThemeProvider } from "@/lib/ThemeProvider";

const errorLink = onError(({ graphQLErrors, networkError }) => {
  if (graphQLErrors) {
    graphQLErrors.forEach(({ message, locations, path }) =>
      console.log(
        `[GraphQL error]: Message: ${message}, Location: ${locations}, Path: ${path}`,
      ),
    );
  }
  if (networkError) {
    console.log(`[Network error]: ${networkError}`);
  }
});

const httpLink = new HttpLink({
  uri: `${process.env.NEXT_PUBLIC_API_URL}graphql`,
  credentials: "include",
  headers: {
    "Content-Type": "application/json",
  },
});

const authLink = setContext((_, { headers }) => {
  if (typeof window === "undefined") {
    return { headers };
  }

  const rawToken = window.localStorage.getItem("authToken");
  if (!rawToken) {
    return { headers };
  }

  let token: string | null = null;
  try {
    token = JSON.parse(rawToken) as string | null;
  } catch {
    token = rawToken;
  }

  if (!token) {
    return { headers };
  }

  return {
    headers: {
      ...headers,
      Authorization: `Bearer ${token}`,
    },
  };
});

export const apolloClient = new ApolloClient({
  link: errorLink.concat(authLink).concat(httpLink),
  cache: new InMemoryCache({
    typePolicies: {
      Query: {
        fields: {
          posts: {
            keyArgs: ["keywords"],
            merge(
              existing: any[] = [],
              incoming: any[] = [],
              { args, readField },
            ) {
              const pageNumber = (args as any)?.pageNumber ?? 1;
              const base = pageNumber === 1 ? [] : existing;

              const merged = [...base, ...incoming];
              const seen = new Set<string>();
              const deduped: any[] = [];

              for (const item of merged) {
                const id =
                  (readField("id", item) as string | undefined) ?? item?.id;
                if (!id) {
                  deduped.push(item);
                  continue;
                }
                if (seen.has(id)) continue;
                seen.add(id);
                deduped.push(item);
              }

              return deduped;
            },
          },
          myPosts: {
            keyArgs: false,
            merge(
              existing: any[] = [],
              incoming: any[] = [],
              { args, readField },
            ) {
              const pageNumber = (args as any)?.pageNumber ?? 1;
              const base = pageNumber === 1 ? [] : existing;

              const merged = [...base, ...incoming];
              const seen = new Set<string>();
              const deduped: any[] = [];

              for (const item of merged) {
                const id =
                  (readField("id", item) as string | undefined) ?? item?.id;
                if (!id) {
                  deduped.push(item);
                  continue;
                }
                if (seen.has(id)) continue;
                seen.add(id);
                deduped.push(item);
              }

              return deduped;
            },
          },
        },
      },
    },
  }),
  defaultOptions: {
    watchQuery: {
      fetchPolicy: "network-only",
    },
    query: {
      fetchPolicy: "network-only",
    },
  },
});

export function Providers({ children }: { children: ReactNode }) {
  return (
    <ThemeProvider>
      <ApolloProvider client={apolloClient}>{children}</ApolloProvider>
    </ThemeProvider>
  );
}
