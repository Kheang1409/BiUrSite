// User Types
export interface User {
  id: string;
  username: string;
  email: string;
  profile?: string;
  bio?: string;
  phone?: string;
  hasNewNotification?: boolean;
  role?: string;
  status?: string;
  banReason?: string;
  banEndDate?: string;
  createdAt?: string;
}

// Post Types
export interface Post {
  id: string;
  text: string;
  username: string;
  userId: string;
  userProfile: string;
  imageUrl?: string;
  createdDate: string;
  comments?: Comment[];
  commentCount?: number;
}

export interface PostDetail extends Omit<Post, "comments"> {
  comments?: Comment[];
}

export interface CreatePostInput {
  text: string;
  data?: number[];
}

export interface UpdatePostInput {
  text: string;
}

// Comment Types
export interface Comment {
  id: string;
  userId: string;
  text: string;
  username: string;
  userProfile?: string;
  postId?: string;
  createdDate: string;
}

export interface CreateCommentInput {
  postId: string;
  text: string;
}

// Notification Types
export interface Notification {
  id: string;
  postId: string;
  userId: string;
  username: string;
  userProfile: string;
  title: string;
  message: string;
  createdDate: string;
  relatedEntityId?: string;
}

// Auth Types
export interface AuthResponse {
  token: string;
  user?: User;
}

export interface LoginInput {
  email: string;
  password: string;
}

export interface RegisterInput {
  username: string;
  email: string;
  password: string;
}

// Pagination
export interface PaginationParams {
  pageNumber: number;
  pageSize?: number;
}

// Query Response Wrappers
export interface GraphQLError {
  message: string;
  locations?: Array<{ line: number; column: number }>;
  path?: Array<string | number>;
  extensions?: Record<string, unknown>;
}

export interface QueryResponse<T> {
  data?: T;
  errors?: GraphQLError[];
}

// SignalR Hub Types
export interface SignalRMessage {
  userId: string;
  message: string;
  timestamp: string;
}

export interface PostNotification {
  postId: string;
  userId: string;
  action: "created" | "updated" | "deleted" | "liked" | "commented";
  timestamp: string;
}
