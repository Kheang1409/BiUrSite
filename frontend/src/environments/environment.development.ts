export const environment = {
    oauth: {
        google:{
            clientId: '569718297749-17htkd89of3n8dqkt7f0oaval2220ic1.apps.googleusercontent.com' ,
            authorizationUrl: 'https://oauth-provider.com/authorize',
            tokenUrl: 'https://oauth-provider.com/token',
            redirectUri: 'https://localhost:4200/callback',
            scope: 'openid profile email',
            responseType: 'code',
        },
        facebook:{
            appId: '1016155900610040',
            redirectUri: 'https://localhost:4200/callback',
            scope: 'email public_profile',
            responseType: 'token',
            version: 'v22.0',
        },
    },
    params: {
        jobId: 'id'
    },
    urlShared: {
        login: 'login',
    },
    urlFrontend: {
        feed: 'feed',
        register: 'register',
        forgotPassword: 'forgot-password',
        confirmationRequired: 'confirmation-required',
        profile: 'profile',
        error: '**',
    },
    urlApi: {
        baseUrl: 'http://backend:5285/api/',
        userUrl: 'users/',
        jobUrl: 'posts/',
        subsetUrl: '/comments/',
        total: 'total',
        query: {
            pageNumber: 'pageNumber',
            keyword: 'keyword',
            userId: 'userId',
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