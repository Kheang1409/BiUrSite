import { User } from "./user";

export class Comment {
    #id!: number;
    #commenter!: User;
    #description!: string;
    #updatedDescription!: string;
    #createdDate!: Date;
    #isEditing!: boolean;

    get id(): number {
        return this.#id
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

    get isEditing(): boolean{
        return this.#isEditing;
    }

    set id(id: number) {
        this.#id = id;
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
    set isEditing(isEditing: boolean){
        this.#isEditing = isEditing;
    }

    jsonify(){
        return {
            description: this.#description
        }
    }
}
