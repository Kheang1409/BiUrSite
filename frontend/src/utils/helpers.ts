export const TokenService = {
  setToken: (token: string) => {
    if (typeof window !== "undefined") {
      localStorage.setItem("authToken", token);
    }
  },

  getToken: (): string | null => {
    if (typeof window !== "undefined") {
      return localStorage.getItem("authToken");
    }
    return null;
  },

  removeToken: () => {
    if (typeof window !== "undefined") {
      localStorage.removeItem("authToken");
    }
  },

  isTokenValid: (): boolean => {
    const token = TokenService.getToken();
    return !!token;
  },
};

interface GraphQLErrorLike {
  graphQLErrors?: Array<{ message: string }>;
  networkError?: Error | null;
  message?: string;
}

interface ApiErrorLike {
  response?: {
    status?: number;
    data?: {
      message?: string;
    };
  };
  message?: string;
}

export const ApiErrorHandler = {
  handleGraphQLError: (error: GraphQLErrorLike): string => {
    if (error.graphQLErrors && error.graphQLErrors.length > 0) {
      return error.graphQLErrors[0].message;
    }
    if (error.networkError) {
      return "Network error. Please check your connection.";
    }
    return error.message || "An error occurred";
  },

  handleApiError: (error: ApiErrorLike): string => {
    if (error.response?.data?.message) {
      return error.response.data.message;
    }
    if (error.response?.status === 401) {
      return "Unauthorized. Please log in.";
    }
    if (error.response?.status === 403) {
      return "Forbidden. You do not have permission.";
    }
    if (error.response?.status === 404) {
      return "Not found.";
    }
    if (error.response?.status && error.response.status >= 500) {
      return "Server error. Please try again later.";
    }
    return error.message || "An error occurred";
  },
};

export const FileUtils = {
  fileToBase64: (file: File): Promise<string> => {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.readAsArrayBuffer(file);
      reader.onload = () => {
        const bytes = new Uint8Array(reader.result as ArrayBuffer);
        const binary = String.fromCharCode.apply(null, Array.from(bytes));
        resolve(btoa(binary));
      };
      reader.onerror = reject;
    });
  },

  fileToByteArray: (file: File): Promise<number[]> => {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.readAsArrayBuffer(file);
      reader.onload = () => {
        const bytes = new Uint8Array(reader.result as ArrayBuffer);
        resolve(Array.from(bytes));
      };
      reader.onerror = reject;
    });
  },

  isImage: (file: File): boolean => {
    return file.type.startsWith("image/");
  },

  isValidFileSize: (file: File, maxSizeMB: number = 5): boolean => {
    return file.size <= maxSizeMB * 1024 * 1024;
  },
};

export const StringUtils = {
  truncate: (str: string, length: number = 100): string => {
    if (str.length <= length) return str;
    return str.slice(0, length) + "...";
  },

  formatUrl: (url: string): string => {
    if (url.startsWith("http://") || url.startsWith("https://")) {
      return url;
    }
    return `https://${url}`;
  },

  sanitizeHtml: (html: string): string => {
    const div = document.createElement("div");
    div.textContent = html;
    return div.innerHTML;
  },
};

export const DateUtils = {
  formatDate: (
    date: string | Date,
    format: string = "MMM DD, YYYY",
  ): string => {
    const d = new Date(date);
    return d.toLocaleDateString();
  },

  getRelativeTime: (date: string | Date): string => {
    const now = new Date();
    const target = new Date(date);
    const diff = now.getTime() - target.getTime();
    const seconds = Math.floor(diff / 1000);
    const minutes = Math.floor(seconds / 60);
    const hours = Math.floor(minutes / 60);
    const days = Math.floor(hours / 24);

    if (seconds < 60) return "just now";
    if (minutes < 60) return `${minutes}m ago`;
    if (hours < 24) return `${hours}h ago`;
    if (days < 7) return `${days}d ago`;
    return target.toLocaleDateString();
  },
};
