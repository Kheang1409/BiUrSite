export class ResetPassword {
    #newPassword: string = '';
    #confirmPassword: string = '';
    #otp: string = '';

    get newPassword(): string{
        return this.#newPassword;
    }

    get confirmPassword(): string{
        return this.#confirmPassword;
    }

    get otp(): string{
        return this.#otp;
    }

    set newPassword(newPassword: string) {
        this.#newPassword = newPassword;
    }

    set confirmPassword(confirmPassword: string) {
        this.#confirmPassword = confirmPassword;
    }

    set otp(otp: string) {
        this.#otp = otp;
    }

    isPasswordMatched(): boolean{
        return this.#newPassword === this.#confirmPassword;
    }

    jsonify(){
        return {
            otp: this.#otp,
            newPassword: this.#newPassword
        }
    }

}
