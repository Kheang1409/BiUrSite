export class User {
  #id!: string;
  #username!: string;
  #email!: string;
  #profile!: string;
  #bio!: string;
  #createdDate!: Date;
  get id(): string {
    return this.#id;
  }

  set id(value: string) {
    this.#id = value;
  }

  get username(): string {
    return this.#username;
  }

  set username(value: string) {
    this.#username = value;
  }

  get email(): string {
    return this.#email;
  }
  set email(value: string) {
    this.#email = value;
  }

  get profile(): string {
    return this.#profile;
  }
  set profile(value: string) {
    this.#profile = value;
  }

  get bio(): string {
    return this.#bio;
  }
  set bio(value: string) {
    this.#bio = value;
  }

  get createdDate(): Date {
    return this.#createdDate;
  }
  set createdDate(value: Date) {
    this.#createdDate = value;
  }

  static fromJson(obj: any): User {
    const u = new User();
    u.id = obj?.id ?? obj?.userId ?? '';
    u.username = obj?.username ?? obj?.name ?? '';
    u.email = obj?.email ?? '';
    u.profile = obj?.profile ?? obj?.avatarUrl ?? '';
    u.bio = obj?.bio ?? '';
    u.createdDate = obj?.createdDate ? new Date(obj.createdDate) : new Date();
    return u;
  }
}
