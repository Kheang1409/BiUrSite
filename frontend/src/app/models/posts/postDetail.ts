import { Comment } from '../comments/comment';
import { Post } from './post';

export class PostDetail {
  #id!: string;
  #userId!: string;
  #username!: string;
  #userProfile!: string;
  #text!: string;
  #createdDate!: Date;
  #imageUrl!: string;
  #comments!: Comment[];

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

  get comments(): Comment[] {
    return this.#comments;
  }

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
    this.#userId = value;
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

  set comments(value: Comment[]) {
    this.#comments = value;
  }

  toPost(): Post {
    const post = new Post();
    post.id = this.id;
    post.userId = this.userId;
    post.username = this.username;
    post.userProfile = this.userProfile;
    post.text = this.text;
    post.createdDate = this.createdDate;
    post.imageUrl = this.imageUrl;
    return post;
  }
}
