export class UserUpdate {
  #username!: string;
  #bio!: string;
  #data!: Uint8Array;

  get username(): string {
    return this.#username;
  }
  set username(value: string) {
    this.#username = value;
  }
  get bio(): string {
    return this.#bio;
  }
  set bio(value: string) {
    this.#bio = value;
  }

  get data(): Uint8Array {
    return this.#data;
  }

  set data(value: Uint8Array) {
    this.#data = value;
  }

  toJSON() {
    return {
      username: this.#username,
      bio: this.#bio,
      data: this.#data,
    };
  }
}
