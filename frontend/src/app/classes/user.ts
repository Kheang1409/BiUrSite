export class User {
    #id: number = 0;
    #email: string = ''
    #username: string = ''
    #profile: string = ''
    

    get id(): number {
        return this.#id;
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
    
    set id(id: number) {
        this.#id = id;
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
}
