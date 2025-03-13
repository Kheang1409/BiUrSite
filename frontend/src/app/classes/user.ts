export class User {
    #userId: number = 0;
    #email: string = ''
    #username: string = ''
    #profile: string = ''
    #password: string = ''
    

    get userId(): number {
        return this.#userId;
    }
    get email(): string {
        return this.#email;
    }
    get username(): string {
        return this.#username;
    }
    get profile(): string{
        return this.#profile;
    }
    get password(): string {
        return this.#password;
    }
    set userId(userId: number) {
        this.#userId = userId;
    }
    set email(email: string) {
        this.#email = email;
    }
    set profile(profile: string) {
        this.#profile = profile;
    }
    set username(username: string) {
        this.#username = username
    }
    set password(passowrd: string) {
        this.#password = passowrd
    }
}
