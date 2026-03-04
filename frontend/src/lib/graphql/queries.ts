import { gql } from "@apollo/client";

export const POSTS_QUERY = gql`
  query Posts($keywords: String, $pageNumber: Int!) {
    posts(keywords: $keywords, pageNumber: $pageNumber) {
      id
      text
      username
      userId
      userProfile
      imageUrl
      createdDate
    }
  }
`;

export const MY_POSTS_QUERY = gql`
  query MyPosts($pageNumber: Int!) {
    myPosts(pageNumber: $pageNumber) {
      id
      text
      username
      userId
      userProfile
      imageUrl
      createdDate
    }
  }
`;

export const POST_DETAIL_QUERY = gql`
  query Post($id: UUID!) {
    post(id: $id) {
      id
      text
      username
      userId
      userProfile
      imageUrl
      commentCount
      createdDate
    }
  }
`;

export const COMMENTS_QUERY = gql`
  query Comments($postId: UUID!, $pageNumber: Int!) {
    comments(postId: $postId, pageNumber: $pageNumber) {
      id
      userId
      text
      username
      userProfile
      createdDate
    }
  }
`;

export const USERS_QUERY = gql`
  query Users($pageNumber: Int!) {
    users(pageNumber: $pageNumber) {
      id
      username
      email
      profile
      bio
    }
  }
`;

export const USER_QUERY = gql`
  query User($id: UUID!) {
    user(id: $id) {
      id
      username
      email
      profile
      bio
      phone
    }
  }
`;

export const ME_QUERY = gql`
  query Me {
    me {
      id
      username
      email
      profile
      bio
      phone
      hasNewNotification
      role
      status
    }
  }
`;

export const UPDATE_ME_MUTATION = gql`
  mutation UpdateMe(
    $username: String!
    $bio: String!
    $phone: String
    $data: [Byte!]
    $removeImage: Boolean = false
  ) {
    updateMe(
      username: $username
      bio: $bio
      phone: $phone
      data: $data
      removeImage: $removeImage
    )
  }
`;

export const NOTIFICATIONS_QUERY = gql`
  query Notifications($pageNumber: Int!) {
    notifications(pageNumber: $pageNumber) {
      id
      postId
      userId
      username
      userProfile
      title
      message
      createdDate
    }
  }
`;

export const MARK_NOTIFICATIONS_AS_READ = gql`
  mutation MarkNotificationsAsRead {
    markNotificationsAsRead
  }
`;

export const LOGIN_MUTATION = gql`
  mutation Login($email: String!, $password: String!) {
    login(email: $email, password: $password) {
      token
    }
  }
`;

export const FORGOT_PASSWORD_MUTATION = gql`
  mutation ForgotPassword($email: String!) {
    forgotPassword(email: $email)
  }
`;

export const RESET_PASSWORD_MUTATION = gql`
  mutation ResetPassword($email: String!, $password: String!, $otp: String!) {
    resetPassword(input: { email: $email, password: $password, otp: $otp })
  }
`;

export const REGISTER_MUTATION = gql`
  mutation Register($username: String!, $email: String!, $password: String!) {
    register(
      input: { username: $username, email: $email, password: $password }
    ) {
      id
      username
      email
    }
  }
`;

export const CREATE_POST_MUTATION = gql`
  mutation CreatePost($text: String!, $data: [Byte!]) {
    createPost(text: $text, data: $data) {
      id
      userId
      text
      username
      userProfile
      imageUrl
      createdDate
    }
  }
`;

export const EDIT_POST_MUTATION = gql`
  mutation EditPost(
    $id: UUID!
    $text: String!
    $data: [Byte!]
    $removeImage: Boolean = false
  ) {
    editPost(id: $id, text: $text, data: $data, removeImage: $removeImage)
  }
`;

export const DELETE_POST_MUTATION = gql`
  mutation DeletePost($id: UUID!) {
    deletePost(id: $id)
  }
`;

export const CREATE_COMMENT_MUTATION = gql`
  mutation CreateComment($postId: UUID!, $text: String!) {
    createComment(postId: $postId, text: $text) {
      id
      userId
      text
      username
      userProfile
      createdDate
    }
  }
`;

export const EDIT_COMMENT_MUTATION = gql`
  mutation EditComment($postId: UUID!, $id: UUID!, $text: String!) {
    editComment(postId: $postId, id: $id, text: $text)
  }
`;

export const DELETE_COMMENT_MUTATION = gql`
  mutation DeleteComment($postId: UUID!, $id: UUID!) {
    deleteComment(postId: $postId, id: $id)
  }
`;

export const BAN_USER_MUTATION = gql`
  mutation BanUser($userId: UUID!, $reason: String, $durationMinutes: Int) {
    banUser(userId: $userId, reason: $reason, durationMinutes: $durationMinutes)
  }
`;

export const UNBAN_USER_MUTATION = gql`
  mutation UnbanUser($userId: UUID!) {
    unbanUser(userId: $userId)
  }
`;

export const ADMIN_DELETE_POST_MUTATION = gql`
  mutation AdminDeletePost($postId: UUID!, $reason: String) {
    adminDeletePost(postId: $postId, reason: $reason)
  }
`;

export const ADMIN_DELETE_COMMENT_MUTATION = gql`
  mutation AdminDeleteComment(
    $postId: UUID!
    $commentId: UUID!
    $reason: String
  ) {
    adminDeleteComment(postId: $postId, commentId: $commentId, reason: $reason)
  }
`;

export const ADMIN_USERS_QUERY = gql`
  query AdminUsers($pageNumber: Int!) {
    adminUsers(pageNumber: $pageNumber) {
      id
      username
      email
      profile
      bio
      role
      status
      banReason
      banEndDate
      createdDate
    }
  }
`;

export const ADMIN_POSTS_QUERY = gql`
  query AdminPosts($keywords: String, $pageNumber: Int!) {
    posts(keywords: $keywords, pageNumber: $pageNumber) {
      id
      text
      username
      userId
      userProfile
      imageUrl
      createdDate
    }
  }
`;
