export class Notification {
  #id!: string;
  #userId!: string;
  #postId!: string;
  #username!: string;
  #userProfile!: string;
  #title!: string;
  #message!: string;
  #createdDate!: Date;

  get id(): string {
    return this.#id;
  }

  get userId(): string {
    return this.#userId;
  }

  get postId(): string {
    return this.#postId;
  }

  get username(): string {
    return this.#username;
  }

  get userProfile(): string {
    return this.#userProfile;
  }

  get title(): string {
    return this.#title;
  }

  get message(): string {
    return this.#message;
  }

  get createdDate(): Date {
    return this.#createdDate;
  }

  set id(value: string) {
    this.#id = value;
  }

  set userId(value: string) {
    this.#userId = value;
  }

  set postId(value: string) {
    this.#postId = value;
  }

  set username(value: string) {
    this.#username = value;
  }

  set userProfile(value: string) {
    this.#userProfile = value;
  }

  set title(value: string) {
    this.#title = value;
  }

  set message(value: string) {
    this.#message = value;
  }

  set createdDate(value: Date) {
    this.#createdDate = value;
  }

  static fromJSON(json: any): Notification {
    const notification = new Notification();
    notification.id = json.id;
    notification.userId = json.userId;
    notification.postId = json.postId;
    notification.username = json.username;
    notification.userProfile = json.userProfile;
    notification.title = json.title;
    notification.message = json.message;
    notification.createdDate = new Date(json.createdDate);
    return notification;
  }
}
