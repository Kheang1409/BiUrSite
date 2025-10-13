export class Comment {
  #id!: string;
  #userId!: string;
  #username!: string;
  #userProfile!: string;
  #text!: string;
  #createdDate!: Date;

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

  static fromJSON(json: any): Comment {
    const comment = new Comment();
    comment.id = json.id;
    comment.userId = json.userId;
    comment.username = json.username;
    comment.userProfile = json.userProfile;
    comment.text = json.text;
    comment.createdDate = new Date(json.createdDate);
    return comment;
  }
}
