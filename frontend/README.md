# Next.js Frontend - BiUrSite

A modern Next.js (TypeScript) frontend for the BiUrSite mini social media platform, replacing the Angular CLI application.

## 🚀 Features

- **Modern Stack**: Next.js 15 + React 19 + TypeScript + Tailwind CSS
- **GraphQL Integration**: Apollo Client with full type safety
- **Dark Mode Support**: Light/dark theme with localStorage persistence
- **Responsive Design**: Mobile-first approach with Tailwind CSS
- **Authentication**: JWT token-based auth with hooks
- **Real-time Updates**: SignalR integration for live notifications
- **Component Architecture**: Reusable, maintainable components
- **TypeScript**: Full type safety across the application

## 📋 Project Structure

```
src/
├── app/                          # Next.js App Router
│   ├── layout.tsx               # Root layout with providers
│   ├── page.tsx                 # Home/Feed page
│   ├── login/page.tsx           # Login page
│   ├── register/page.tsx        # Register page
│   ├── profile/page.tsx         # User profile page
│   └── people/page.tsx          # People discovery page
├── components/
│   ├── Header.tsx               # Navigation header
│   ├── Post.tsx                 # Post component
│   ├── Comment.tsx              # Comment component
│   ├── UserCard.tsx             # User card component
│   ├── CreatePostCard.tsx       # Create post form
│   ├── Providers.tsx            # Apollo + Theme providers
│   ├── layouts/
│   │   ├── MainLayout.tsx       # Main app layout
│   │   └── AuthLayout.tsx       # Auth pages layout
│   └── pages/
│       ├── Feed.tsx             # Feed component
│       ├── ProfilePage.tsx      # Profile page
│       ├── PeoplePage.tsx       # People page
│       └── Auth/
│           ├── LoginPage.tsx    # Login form
│           └── RegisterPage.tsx # Register form
├── hooks/                        # Custom React hooks
│   ├── useAuth.ts               # Authentication hook
│   ├── useGraphQLAuth.ts        # GraphQL auth hook
│   └── useLocalStorage.ts       # localStorage hook
├── lib/
│   ├── graphql/
│   │   └── queries.ts           # GraphQL queries/mutations
│   └── ThemeProvider.tsx        # Theme context provider
├── types/
│   └── index.ts                 # TypeScript type definitions
├── utils/
│   └── helpers.ts               # Utility functions
└── styles/
    └── globals.css              # Global styles + Tailwind
```

## 🛠️ Setup & Installation

### Prerequisites

- Node.js 18+
- npm or yarn

### Installation

1. **Clone and navigate to the project**:

   ```bash
   cd frontend
   npm install
   ```

2. **Configure environment variables**:

   ```bash
   # .env.local (already provided)
   NEXT_PUBLIC_API_URL=localhost:3000/
   ```

3. **Install dependencies**:

   ```bash
   npm install
   ```

4. **Run development server**:

   ```bash
   npm run dev
   ```

   Open [http://localhost:3000](http://localhost:3000) in your browser.

## 📦 Available Scripts

| Command              | Description                       |
| -------------------- | --------------------------------- |
| `npm run dev`        | Start development server          |
| `npm run build`      | Build for production              |
| `npm run start`      | Start production server           |
| `npm run lint`       | Run ESLint                        |
| `npm run type-check` | Check TypeScript types            |
| `npm run codegen`    | Generate GraphQL types (optional) |

## 🎨 Styling & Theme

### Color Palette

The app uses a gradient-based color scheme matching the original Angular design:

```typescript
colors: {
  primary: {
    1: '#0682a5',  // Cyan-blue
    2: '#223056',  // Slate blue
    3: '#0f172a',  // Deep navy
  },
  secondary: { ... },
  tertiary: { ... },
  danger: { ... },
  success: { ... },
}
```

### Theme Switching

The theme provider (`ThemeProvider`) automatically:

- Detects system preference
- Loads saved preference from localStorage
- Applies light/dark mode classes to `<html>` element
- Persists user choice

**Usage in components**:

```typescript
import { useEffect } from "react";

// Components automatically adapt to dark/light mode
// using Tailwind's `dark:` modifier
<div className="bg-white dark:bg-slate-900">Content adapts to theme</div>;
```

## 🔐 Authentication

### Login Flow

```typescript
import { useGraphQLAuth } from "@/hooks/useGraphQLAuth";

const { handleLogin, logout, currentUser } = useGraphQLAuth();

// Login
await handleLogin(email, password);

// Logout
logout();

// Access current user
console.log(currentUser);
```

### Protected Routes

Wrap components with authentication checks:

```typescript
import { useAuth } from "@/hooks/useAuth";

export function ProtectedComponent() {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) return <Loading />;
  if (!isAuthenticated) return <Redirect to="/login" />;

  return <ProtectedContent />;
}
```

## 📡 GraphQL Integration

### Using Queries

```typescript
import { useQuery } from "@apollo/client";
import { POSTS_QUERY } from "@/lib/graphql/queries";

export function Feed() {
  const { data, loading, error } = useQuery(POSTS_QUERY, {
    variables: {
      pageNumber: 1,
      keywords: null,
    },
  });

  const posts = data?.posts || [];
  // ...
}
```

### Using Mutations

```typescript
import { useMutation } from "@apollo/client";
import { CREATE_POST_MUTATION } from "@/lib/graphql/queries";

export function CreatePost() {
  const [createPost, { loading }] = useMutation(CREATE_POST_MUTATION, {
    onCompleted: (data) => {
      console.log("Post created:", data);
    },
    onError: (error) => {
      console.error("Failed to create post:", error);
    },
  });

  const handleCreate = async (text: string) => {
    await createPost({
      variables: { text },
    });
  };

  // ...
}
```

### Available Queries/Mutations

See [src/lib/graphql/queries.ts](src/lib/graphql/queries.ts) for complete list:

**Queries**:

- `POSTS_QUERY` - Get feed posts
- `MY_POSTS_QUERY` - Get user's posts
- `POST_DETAIL_QUERY` - Get single post with comments
- `USERS_QUERY` - Get users list
- `USER_QUERY` - Get user profile
- `ME_QUERY` - Get current user
- `NOTIFICATIONS_QUERY` - Get notifications

**Mutations**:

- `LOGIN_MUTATION` - User login
- `REGISTER_MUTATION` - User registration
- `CREATE_POST_MUTATION` - Create post
- `EDIT_POST_MUTATION` - Edit post
- `DELETE_POST_MUTATION` - Delete post
- `CREATE_COMMENT_MUTATION` - Add comment
- `EDIT_COMMENT_MUTATION` - Edit comment
- `DELETE_COMMENT_MUTATION` - Delete comment

## 🧩 Component Usage

### Post Component

```typescript
import { Post } from "@/components/Post";

<Post
  post={postData}
  isOwner={currentUserId === post.userId}
  onEdit={(id, content) => handleEdit(id, content)}
  onDelete={(id) => handleDelete(id)}
  onLike={(id) => handleLike(id)}
  onComment={(id) => navigateToPost(id)}
/>;
```

### UserCard Component

```typescript
import { UserCard } from "@/components/UserCard";

<UserCard
  user={userData}
  isFollowing={false}
  onFollow={() => handleFollow(user.id)}
  onUnfollow={() => handleUnfollow(user.id)}
/>;
```

### CreatePostCard Component

```typescript
import { CreatePostCard } from "@/components/CreatePostCard";

<CreatePostCard onPostCreated={() => refetchPosts()} />;
```

## 🎯 Best Practices

### TypeScript

All components and utilities are fully typed:

```typescript
interface PostProps {
  post: Post;
  onEdit?: (postId: string, content: string) => void;
  isOwner?: boolean;
}

export function Post({ post, onEdit, isOwner }: PostProps) {
  // ...
}
```

### Component Structure

- **Functional components** only (hooks-based)
- **Clear separation of concerns**
- **Reusable and composable**
- **Proper error boundaries**

### State Management

- **Local state**: `useState` for component-level state
- **Global state**: Apollo Client cache for API data
- **Persistent state**: Custom `useLocalStorage` hook
- **Auth state**: Custom `useAuth` hook with localStorage

### Error Handling

```typescript
// GraphQL errors
const { data, loading, error } = useQuery(QUERY);
if (error) {
  return <ErrorBoundary error={error} />;
}

// Mutation errors
const [mutate] = useMutation(MUTATION, {
  onError: (error) => {
    const message = ApiErrorHandler.handleGraphQLError(error);
    showErrorToast(message);
  },
});
```

## 🚀 Deployment

### Build for Production

```bash
npm run build
npm start
```

### Docker

A `Dockerfile` is included for containerized deployment:

```bash
docker build -t biursite-frontend .
docker run -p 3000:3000 -e NEXT_PUBLIC_API_URL=https://api.example.com/ biursite-frontend
```

### Environment Variables

For production deployment, set:

```env
NEXT_PUBLIC_API_URL=https://your-api-domain.com/
```

## Troubleshooting

### Port 3000 already in use

```bash
npm run dev -- -p 3001
```

### Clear cache and reinstall

```bash
rm -rf node_modules .next
npm install
npm run dev
```

### TypeScript errors

```bash
npm run type-check
```

### Check Apollo Client connection

- Verify `NEXT_PUBLIC_API_URL` is correct
- Check browser DevTools Network tab for GraphQL requests
- Verify backend GraphQL endpoint is accessible

## 📚 Key Technologies

- **Next.js 15** - React framework with App Router
- **React 19** - UI library
- **TypeScript 5.3** - Type safety
- **Tailwind CSS 3.4** - Utility-first styling
- **Apollo Client 3.10** - GraphQL client
- **Zustand** - State management (optional)
- **DayJS** - Date manipulation
- **SignalR** - Real-time communication

## 🤝 Contributing

1. Follow the existing component structure
2. Maintain TypeScript types for all functions
3. Use Tailwind CSS for styling (no inline styles)
4. Keep components small and reusable
5. Test GraphQL queries in Apollo Studio

## 📄 License

Same as the parent project.

## 🆘 Support

For issues or questions:

1. Check existing components for examples
2. Review GraphQL queries and mutations
3. Verify environment configuration
4. Check browser console and DevTools

---

**Ready to use!** Install dependencies and run `npm run dev` to get started.
