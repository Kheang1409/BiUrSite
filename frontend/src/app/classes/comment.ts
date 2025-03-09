import { User } from "./user";

export class Comment {
    #commentId!: number;
    #commenter!: User;
    #description!: string;
    #createdDate!: Date;
    #modifiedDate!: Date;

    get commentId(): number {
        return this.#commentId
    }

    get commenter(): User {
        return this.#commenter
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

    set commentId(commentId: number) {
        this.#commentId = commentId;
    }
    set commenter(commenter: User) {
        this.#commenter = commenter;
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
            description: this.#description
        }
    }
}
