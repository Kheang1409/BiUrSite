import { Comment } from "./comment";
import { User } from "./user";

export class Post {
    #id!: number;
    #author!: User;
    #comments!: Comment[];
    #description!: string;
    #createdDate!: Date;

    get id(): number {
        return this.#id
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

    set id(id: number) {
        this.#id = id;
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

    jsonify(){
        return {
            description : this.#description
        }
    }
}
