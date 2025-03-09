import { User } from "./user";

export class Register {
    #email: string = ''
    #username: string = ''
    #password: string = ''
    #confirmPassword: string = ''

    get email(): string {
        return this.#email;
    }
    get username(): string {
        return this.#username;
    }
    get password(): string {
        return this.#password;
    }
    get confirmPassword(): string {
        return this.#confirmPassword;
    }
    
    set email(email: string) {
        this.#email = email;
    }
    set username(username: string) {
        this.#username = username
    }
    set password(passowrd: string) {
        this.#password = passowrd
    }
    set confirmPassword(confirmPassword: string) {
        this.#confirmPassword = confirmPassword
    }

    isPasswordMatched(): boolean{
        return this.#confirmPassword === this.#password;
    }

    jsonify() {
        return {
            email: this.email,
            username: this.username,
            password: this.password
        }
    }
}
