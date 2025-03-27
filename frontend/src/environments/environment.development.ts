export const environment = {
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