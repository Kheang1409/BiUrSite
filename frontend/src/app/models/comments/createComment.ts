export class CreateComment {
  #text!: string;

  get text(): string {
    return this.#text;
  }

  set text(value: string) {
    this.#text = value;
  }

  toJSON() {
    return {
      text: this.#text,
    };
  }
}
