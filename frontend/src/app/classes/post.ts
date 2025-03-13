import { Comment } from "./comment";
import { User } from "./user";

export class Post {
    #postId!: number;
    #author!: User;
    #comments!: Comment[];
    #description!: string;
    #createdDate!: Date;
    #modifiedDate!: Date;

    get postId(): number {
        return this.#postId
    }
    get author(): User {
        return this.#author
    }
    get comments(): Comment[]{
        return this.#comments;
    }
    get description(): string {
        return this.#description
    }
    get createdDate(): Date {
        return this.#createdDate
    }
    get modifiedDate(): Date {
        return this.#modifiedDate;
    }

    set postId(postId: number) {
        this.#postId = postId;
    }

    set author(author: User) {
        this.#author = author;
    }
    set comments(comments: Comment[]){
        this.#comments = comments;
    }

    set description(description: string) {
        this.#description = description;
    }
    set createdDate(createdDate: Date ){
        this.#createdDate = createdDate;
    }
    set modifiedDate(modifiedDate: Date ){
        this.#modifiedDate = modifiedDate;
    }

    jsonify(){
        return {
            description : this.#description
        }
    }
}
