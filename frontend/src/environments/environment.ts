export const environment = {
  params: {
    jobId: 'id',
  },
  urlShared: {
    login: 'login',
    forgotPassword: 'forgot-password',
    resetPassword: 'reset-password',
    register: 'register',
    google: 'signin-oauth/Google',
    facebook: 'signin-oauth/Facebook',
  },
  urlFrontend: {
    feed: '',
    privacy: 'privacy',
    confirmationRequired: 'confirmation-required',
    profile: 'profile',
    people: 'people',
    error: '**',
  },
  urlApi: {
    baseUrl: 'http://localhost:5000/',
    authUrl: 'api/auth/',
    userUrl: 'api/users/',
    postUrl: 'api/posts/',
    notificationUrl: 'api/notifications/',
    feedHub: 'feedHub',
    notificationHub: 'notificationHub',
    subsetUrl: '/comments/',
    me: 'me',
    query: {
      pageNumber: 'PageNumber',
      keywords: 'Keywords',
    },
  },
  message: {
    updateFailMessage: 'Update unsuccessfully!',
    createFailMessage: 'Create unsuccessfully!',
    unauthorizedMessage: 'Unauthorized!',
    missingUsernamePassword: 'Missing Email or Password!',
    passwordMissedMatch: 'Password missed match!',
    filledInTheBlank: 'Filled in the Blank!',
  },
  numbers: {
    page: 1,
  },
  keys: {
    tokenKey: 'authToken',
    pageNumberKey: 'pageNumber',
  },
};
