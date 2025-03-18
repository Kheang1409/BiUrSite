export const environment = {
    params: {
        jobId: 'id'
    },
    urlShared: {
        login: 'login',
        forgotPassword: 'forgot-password',
        resetPassword: 'reset-password'
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
        notificationHub: 'notificationHub',
        subsetUrl: '/comments/',
        totalPost: 'total-post',
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