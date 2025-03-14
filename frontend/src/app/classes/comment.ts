import { User } from "./user";

export class Comment {
    #commentId!: number;
    #commenter!: User;
    #description!: string;
    #updatedDescription!: string;
    #createdDate!: Date;
    #modifiedDate!: Date;
    #isEditing!: boolean;

    get commentId(): number {
        return this.#commentId
    }

    get commenter(): User {
        return this.#commenter
    }

    get description(): string {
        return this.#description
    }
    get updatedDescription(): string {
        return this.#updatedDescription
    }
    get createdDate(): Date {
        return this.#createdDate
    }
    get modifiedDate(): Date {
        return this.#modifiedDate;
    }

    get isEditing(): boolean{
        return this.#isEditing;
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
    set updatedDescription(updatedDescription: string) {
        this.#updatedDescription = updatedDescription;
    }
    set createdDate(createdDate: Date ){
        this.#createdDate = createdDate;
    }
    set modifiedDate(modifiedDate: Date ){
        this.#modifiedDate = modifiedDate;
    }
    set isEditing(isEditing: boolean){
        this.#isEditing = isEditing;
    }

    jsonify(){
        return {
            description: this.#description
        }
    }
}
