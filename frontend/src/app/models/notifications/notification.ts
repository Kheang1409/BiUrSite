export class Notification {
    #notificationId!: number;
    #userId!: number;
    #message!: string;
    #postId!: number;
    #commentId!: number;
    #createdDate!: Date;

    get notificationId(): number {
        return this.#notificationId;
    }

    get userId(): number {
        return this.#userId;
    }

    get message(): string {
        return this.#message;
    }

    get postId(): number {
        return this.#postId;
    }

    get commentId(): number {
        return this.#commentId;
    }

    get createdDate(): Date {
        return this.#createdDate;
    }

    set notificationId(notificationId: number) {
        this.#notificationId = notificationId;
    }

    set userId(userId: number) {
        this.#userId = userId;
    }

    set message(message: string) {
        this.#message = message;
    }

    set postId(postId: number) {
        this.#postId = postId;
    }

    set commentId(commentId: number) {
        this.#commentId = commentId;
    }

    set createdDate(createdDate: Date) {
        this.#createdDate = createdDate;
    }

    jsonify() {
        return {
        userId: this.#userId,
        message: this.#message,
        postId: this.#postId,
        commentId: this.#commentId,
        };
    }
}
