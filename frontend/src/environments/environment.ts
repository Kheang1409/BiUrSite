export const environment = {
    oauth: {
        google:{
            clientId: '569718297749-17htkd89of3n8dqkt7f0oaval2220ic1.apps.googleusercontent.com' ,
            authorizationUrl: 'https://oauth-provider.com/authorize',
            tokenUrl: 'https://oauth-provider.com/token',
            cookiepolicy: 'single_host_origin',
            redirectUri: 'https://localhost:4200/callback',
            scope: 'openid profile email',
            responseType: 'code',
            message: 'Google Auth initialized',
            error: 'Error initializing Google Auth',
        },
        facebook:{
            appId: '1016155900610040',
            redirectUri: 'https://localhost:4200/callback',
            scope: 'email public_profile',
            responseType: 'token',
            version: 'v22.0',
            message: 'Facebook Auth initialized',
            error: 'Error initializing Facebook Auth',
            initialized : 'Facebook SDK initialized',
            notInitialized: "Facebook SDK not initialized.",
            urlSDK : 'https://connect.facebook.net/en_US/sdk.js'
        }
        
    },
    params: {
        jobId: 'id'
    },
    urlShared: {
        login: 'login',
        forgotPassword: 'forgot-password',
        resetPassword: 'reset-password',
        oauth: 'auth/external-login'
    },
    urlFrontend: {
        feed: 'feed',
        register: 'register',
        confirmationRequired: 'confirmation-required',
        profile: 'profile',
        error: '**',
    },
    urlApi: {
        baseUrl: 'http://localhost:5000/',
        userUrl: 'api/users/',
        postUrl: 'api/posts/',
        notificationUrl: 'api/notifications/',
        notificationHub: 'notificationHub',
        subsetUrl: '/comments/',
        total: 'total',
        query: {
            pageNumber: 'pageNumber',
            keyword: 'keyword',
            userId: 'userId',
            isRead: 'isRead'
        },
    },
    message: {
        updateFailMessage: 'Update unsuccessfully!',
        createFailMessage: 'Create unsuccessfully!',
        unauthorizedMessage: 'Unauthorized!',
        missingUsernamePassword: 'Missing Email or Password!',
        passwordMissedMatch: 'Password missed match!',
        filledInTheBlank: 'Filled in the Blank!'
    },
    numbers: {
        page: 1,
        limit: 5,
    },
    keys: {
        tokenKey: 'authToken',
        pageNumberKey: 'pageNumber'
    }
};