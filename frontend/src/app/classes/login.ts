export class Login {
    #email: string = ''
    #password: string = ''

    set password(passowrd: string) {
        this.#password = passowrd
    }
    set email(email: string) {
        this.#email = email;
    }

    get email(): string {
        return this.#email;
    }
    
    get password(): string {
        return this.#password;
    }

    jsonify(){
        return {
            email : this.#email,
            password : this.#password
        }
    }
}
