export class Post {
  #id!: string;
  #userId!: string;
  #username!: string;
  #userProfile!: string;
  #text!: string;
  #createdDate!: Date;
  #imageUrl!: string;

  get id(): string {
    return this.#id;
  }

  get userId(): string {
    return this.#userId;
  }

  get username(): string {
    return this.#username;
  }

  get userProfile(): string {
    return this.#userProfile;
  }

  get text(): string {
    return this.#text;
  }

  get createdDate(): Date {
    return this.#createdDate;
  }

  get imageUrl(): string {
    return this.#imageUrl;
  }

  // Setters
  set id(value: string) {
    this.#id = value;
  }

  set userId(value: string) {
    this.#userId = value;
  }

  set username(value: string) {
    this.#username = value;
  }

  set userProfile(value: string) {
    this.#userProfile = value;
  }

  set text(value: string) {
    this.#text = value;
  }

  set createdDate(value: Date) {
    this.#createdDate = value;
  }

  set imageUrl(value: string) {
    this.#imageUrl = value;
  }

  static fromJSON(json: any): Post {
    const post = new Post();
    post.id = json.id;
    post.userId = json.userId;
    post.username = json.username;
    post.userProfile = json.userProfile;
    post.text = json.text;
    post.createdDate = new Date(json.createdDate);
    post.imageUrl = json.imageUrl;
    return post;
  }
}
